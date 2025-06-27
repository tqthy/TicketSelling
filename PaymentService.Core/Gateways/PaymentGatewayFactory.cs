using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaymentService.Core.Contracts.Gateways;
using PaymentService.Core.Exceptions;

namespace PaymentService.Core.Gateways;

public interface IPaymentGatewayFactory
{
    IPaymentGateway CreateGateway(string gatewayName);
    IPaymentGateway GetGateway(string gatewayName);
    bool TryGetGateway(string gatewayName, out IPaymentGateway gateway);
}

public class PaymentGatewayFactory : IPaymentGatewayFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PaymentGatewayFactory> _logger;
    private readonly Dictionary<string, Type> _gatewayTypes;
    private readonly Dictionary<string, IPaymentGateway> _gatewayInstances = new();
    private readonly object _lock = new();

    public PaymentGatewayFactory(
        IServiceProvider serviceProvider,
        ILogger<PaymentGatewayFactory> logger,
        IEnumerable<KeyValuePair<string, Type>> gatewayTypes)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _gatewayTypes = gatewayTypes?.ToDictionary(kv => kv.Key, kv => kv.Value) 
            ?? throw new ArgumentNullException(nameof(gatewayTypes));
    }

    public IPaymentGateway CreateGateway(string gatewayName)
    {
        if (string.IsNullOrWhiteSpace(gatewayName))
            throw new ArgumentException("Gateway name cannot be null or whitespace.", nameof(gatewayName));

        var normalizedGatewayName = gatewayName.Trim().ToLowerInvariant();

        lock (_lock)
        {
            if (_gatewayInstances.TryGetValue(normalizedGatewayName, out var existingGateway))
                return existingGateway;

            if (!_gatewayTypes.TryGetValue(normalizedGatewayName, out var gatewayType))
                throw new PaymentGatewayNotSupportedException(gatewayName);

            try
            {
                var gateway = (IPaymentGateway)_serviceProvider.GetRequiredService(gatewayType);
                _gatewayInstances[normalizedGatewayName] = gateway;
                _logger.LogInformation("Created payment gateway instance for {GatewayName}", gatewayName);
                return gateway;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create payment gateway for {GatewayName}", gatewayName);
                throw new PaymentGatewayException(
                    $"Failed to create payment gateway '{gatewayName}'. See inner exception for details.",
                    "GATEWAY_CREATION_FAILED",
                    gatewayName,
                    true, // Mark as transient
                    ex);
            }
        }
    }

    public IPaymentGateway GetGateway(string gatewayName)
    {
        if (TryGetGateway(gatewayName, out var gateway))
            return gateway;

        throw new PaymentGatewayNotSupportedException(gatewayName);
    }

    public bool TryGetGateway(string gatewayName, out IPaymentGateway gateway)
    {
        if (string.IsNullOrWhiteSpace(gatewayName))
        {
            gateway = null;
            return false;
        }

        var normalizedGatewayName = gatewayName.Trim().ToLowerInvariant();

        lock (_lock)
        {
            if (_gatewayInstances.TryGetValue(normalizedGatewayName, out gateway))
                return true;

            if (_gatewayTypes.TryGetValue(normalizedGatewayName, out var gatewayType))
            {
                gateway = CreateGateway(gatewayName);
                return true;
            }

            return false;
        }
    }
}
