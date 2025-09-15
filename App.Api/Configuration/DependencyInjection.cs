using App.Application.Interfaces;
using App.Application.Interfaces.Repositories;
using App.Application.Interfaces.Services;
using App.Application.Service;
using App.Application.Service.App.Application.Services;
using App.Application.Services;
using App.Infrastructure.DBContext;
using App.Infrastructure.Repository;
using App.SharedConfigs.DBContext;
using System.Net;
using System.Net.Security;
using System.Security.Authentication;

namespace App.Api.Configuration
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddAppDependencies(this IServiceCollection services)
        {
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork<ApplicationDbContext>>();
            services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
            services.AddSingleton<Func<IServiceProvider, string>>(sp =>
            {
                var configDb = sp.GetRequiredService<MasterDbContext>();
                var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
                var httpContext = httpContextAccessor.HttpContext ?? throw new Exception("No active HTTP context found.");

                // Get industry name from header
                var industryName = httpContext.Request.Headers["Industry-Name"].FirstOrDefault();

                //var targetConnectionString = configDb.IndustryConfig
                //    .Where(c => c.Name == industryName)
                //    .Select(c => c.ConnectionString)
                //    .FirstOrDefault();

                //if (string.IsNullOrEmpty(targetConnectionString))
                //{
                //    throw new Exception($"AppDb connection string not found for industry '{industryName}'.");
                //}

                return "";
            });


            services.AddTransient<IDoctorRagService, DoctorRagService>();
            services.AddTransient<IMedicalTaxonomyService, MedicalTaxonomyService>();
            services.AddTransient<ISymptomAnalysisService, SymptomAnalysisService>();
            services.AddTransient<ISymptomEmbeddingRepository, SymptomEmbeddingRepository>();
            services.AddTransient<IDoctorRepository, DoctorRepository>();

            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;

            services.AddHttpClient<IEmbeddingService, EmbeddingService>()
                .ConfigurePrimaryHttpMessageHandler(() =>
                new HttpClientHandler
                {
                    SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => errors == SslPolicyErrors.None
                });

            return services;
        }
    }
}
