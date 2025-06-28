using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaymentService.Core.Contracts.Gateways;
using PaymentService.Core.Gateways;
using PaymentService.Core.Exceptions;

namespace PaymentService.Core.Extensions;

public static class ServiceCollectionExtensions
{
    // Dictionary mapping gateway names to their implementation types
    private static readonly Dictionary<string, Type> GatewayTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        { VnPayGateway.GatewayName, typeof(VnPayGateway) },
        // Add new gateways here:
        // { "MomoGateway", typeof(MomoGateway) },
        // { "PayPalGateway", typeof(PayPalGateway) },
    };

    public static IServiceCollection AddPaymentGateways(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));
        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));

        // Get logger for extension method
        var serviceProvider = services.BuildServiceProvider();
        var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
        var logger = loggerFactory?.CreateLogger("PaymentService.Core.Extensions.ServiceCollectionExtensions");

        try
        {
            logger?.LogInformation("Starting to configure payment gateways...");

            // Get the gateways section from config
            var gatewaysSection = configuration.GetSection("PaymentGateways");
            if (!gatewaysSection.Exists())
            {
                throw new PaymentGatewayConfigurationException(
                    "No 'PaymentGateways' section found in configuration.", "Global");
            }

            var enabledGateways = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

            // Register each gateway
            foreach (var gatewayEntry in GatewayTypes)
            {
                var gatewayName = gatewayEntry.Key;
                var gatewayType = gatewayEntry.Value;
                var configSection = gatewaysSection.GetSection(gatewayName);
                
                if (!configSection.Exists())
                {
                    var error = $"Configuration section 'PaymentGateways:{gatewayName}' not found. This gateway will be disabled.";
                    logger?.LogWarning(error);
                    continue;
                }

                // Register the gateway options with named options
                services.Configure<PaymentGatewayOptions>(gatewayName, options =>
                {
                    configSection.Bind(options);
                    
                    // Log the bound options
                    logger?.LogInformation("Bound options for {GatewayName}: " +
                        "BaseUrl={BaseUrl}, " +
                        "ReturnUrl={ReturnUrl}, " +
                        "TmnCode={TmnCode}, " +
                        "Locale={Locale}, " +
                        "TimeoutInSeconds={TimeoutInSeconds}",
                        gatewayName,
                        options.BaseUrl ?? "[NULL]",
                        options.ReturnUrl ?? "[NULL]",
                        options.TmnCode ?? "[NULL]",
                        options.Locale ?? "[NULL]",
                        options.TimeoutInSeconds);
                });
                
                // Register the gateway implementation with named options
                var normalizedGatewayName = gatewayName.Trim().ToLowerInvariant();
                
                // Register a factory that resolves the named options
                services.AddTransient(gatewayType, provider =>
                {
                    var optionsMonitor = provider.GetRequiredService<IOptionsMonitor<PaymentGatewayOptions>>();
                    var options = optionsMonitor.Get(gatewayName); // Get the named options
                    
                    var loggerType = typeof(ILogger<>).MakeGenericType(gatewayType);
                    var gatewayLogger = provider.GetRequiredService(loggerType);

                    // Create the gateway instance with the resolved options
                    // The Activator will now find the constructor that takes IOptions<PaymentGatewayOptions> and ILogger<VnPayGateway>
                    var gateway = Activator.CreateInstance(
                        gatewayType,
                        Options.Create(options), // Wrap the options in IOptions
                        gatewayLogger) as IPaymentGateway;
                    
                    if (gateway == null)
                    {
                        throw new InvalidOperationException($"Failed to create instance of {gatewayType.Name}");
                    }
                    
                    return gateway;
                });
                
                // Add to enabled gateways
                enabledGateways[normalizedGatewayName] = gatewayType;
                
                // Verify the options were registered correctly
                using (var scope = services.BuildServiceProvider().CreateScope())
                {
                    try
                    {
                        var optionsMonitor = scope.ServiceProvider.GetRequiredService<IOptionsMonitor<PaymentGatewayOptions>>();
                        var options = optionsMonitor.Get(gatewayName);
                        
                        if (string.IsNullOrWhiteSpace(options.BaseUrl))
                        {
                            logger?.LogWarning("BaseUrl is null or empty for gateway: {GatewayName}", gatewayName);
                        }
                        
                        if (string.IsNullOrWhiteSpace(options.TmnCode))
                        {
                            logger?.LogWarning("TmnCode is null or empty for gateway: {GatewayName}", gatewayName);
                        }
                        
                        logger?.LogInformation("Successfully verified options for {GatewayName}", gatewayName);
                    }
                    catch (Exception ex)
                    {
                        logger?.LogError(ex, "Failed to verify options for gateway: {GatewayName}", gatewayName);
                        throw;
                    }
                }
                
                logger?.LogInformation("Registered payment gateway: {GatewayName} (Normalized: {NormalizedName})", 
                    gatewayName, normalizedGatewayName);
            }

            // Register the gateway factory
            services.AddSingleton<PaymentGatewayFactory>(provider => 
            {
                var factoryLogger = provider.GetRequiredService<ILogger<PaymentGatewayFactory>>();
                return new PaymentGatewayFactory(provider, factoryLogger, enabledGateways);
            });
            
            // Register the interface for dependency injection
            services.AddSingleton<IPaymentGatewayFactory>(provider => 
                provider.GetRequiredService<PaymentGatewayFactory>());

            logger?.LogInformation("Successfully configured {GatewayCount} payment gateways", enabledGateways.Count);
            return services;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Failed to configure payment gateways");
            throw;
        }
    }
}
