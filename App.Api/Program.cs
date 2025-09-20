using App.Api.Configuration;
using App.Api.Middleware;
using App.Api.Validation;
using App.Application.Interfaces.Repositories;
using App.Application.Service;
using App.Application.Service.App.Application.Services;
using App.Application.Services;
using App.Infrastructure.Repository;
using DotNetEnv;
using FluentValidation;
using FluentValidation.AspNetCore;

internal class Program
{
    private static void Main(string[] args)
    {
        try
        {
            //Env.Load("development.env");
            var builder = WebApplication.CreateBuilder(args);

            // Force to use Railway port (default 8080)
            //var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
            //builder.WebHost.UseUrls($"http://0.0.0.0:{8080}");

            //   Make sure env vars are part of IConfiguration
            //builder.Configuration.AddEnvironmentVariables();

            builder.Services.AddAppDependencies();
            builder.Services.AddConfiguredDbContexts(builder.Configuration);

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddControllers();
            builder.Services.AddFluentValidationAutoValidation()
                .AddFluentValidationClientsideAdapters();
            builder.Services.AddValidatorsFromAssemblyContaining<UsersValidator>();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();


            var app = builder.Build();


            app.UseMiddleware<ExceptionHandlingMiddleware>();
           
                app.UseSwagger();
                app.UseSwaggerUI();
            



            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
}