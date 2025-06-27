using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PaymentService.Core.Gateways;


namespace PaymentService.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPaymentGateways(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));
        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));

        // Register VNPay gateway
        services.Configure<PaymentGatewayOptions>(
            VnPayGateway.GatewayName,
            configuration.GetSection($"PaymentGateways:{VnPayGateway.GatewayName}"));

        services.AddTransient<VnPayGateway>();
        
        // Register the gateway factory
        services.AddSingleton<IPaymentGatewayFactory>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<PaymentGatewayFactory>>();
            var gateways = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
            {
                { VnPayGateway.GatewayName, typeof(VnPayGateway) }
                // Add more gateways here as needed
            };
            
            return new PaymentGatewayFactory(provider, logger, gateways);
        });

        return services;
    }
}
