using DocumentGenerator.Business.Interfaces;
using DocumentGenerator.Business.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace DocumentGenerator.Business;

public static class ServiceRegistration
{
    public static IServiceCollection AddBusinessLayer(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITemplateService, TemplateService>();
        services.AddScoped<ILookupService, LookupService>();
        services.AddScoped<IAssetService, AssetService>();

        services.AddValidatorsFromAssembly(typeof(ServiceRegistration).Assembly);

        return services;
    }
}
