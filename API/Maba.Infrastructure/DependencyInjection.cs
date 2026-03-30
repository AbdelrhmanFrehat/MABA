using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Maba.Application.Common.Interfaces;
using Maba.Infrastructure.Data;
using Maba.Infrastructure.Data.Seeding;

namespace Maba.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Data Source=.;Integrated Security=true;Database=EasyStudy;TrustServerCertificate=true;Encrypt=false";

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
        });

        services.AddScoped<IApplicationDbContext>(provider => 
            provider.GetRequiredService<ApplicationDbContext>());

        // Register file storage service
        services.AddScoped<Maba.Application.Common.Interfaces.IFileStorageService, Services.LocalFileStorageService>();

        // Register AI service
        var openAiSection = configuration.GetSection("OpenAi");
        services.Configure<Services.OpenAiSettings>(openAiSection);
        
        var aiSection = configuration.GetSection("Ai");
        var useMock = aiSection.GetValue<bool>("UseMock", false);
        if (useMock)
        {
            services.AddSingleton<Maba.Application.Common.Interfaces.IAiService, Services.MockAiService>();
        }
        else
        {
            services.AddHttpClient<Services.OpenAiService>();
            services.AddScoped<Maba.Application.Common.Interfaces.IAiService, Services.OpenAiService>();
        }

        // Register email service: use SMTP when configured, otherwise mock
        services.Configure<Services.SmtpSettings>(configuration.GetSection(Services.SmtpSettings.SectionName));
        var smtpHost = configuration["Smtp:Host"];
        if (!string.IsNullOrWhiteSpace(smtpHost))
        {
            services.AddScoped<Maba.Application.Common.Interfaces.IEmailService, Services.SmtpEmailService>();
        }
        else
        {
            services.AddScoped<Maba.Application.Common.Interfaces.IEmailService, Services.MockEmailService>();
        }

        // Register notification service
        services.AddScoped<Maba.Application.Common.Interfaces.INotificationService, Services.NotificationService>();

        // Register audit service
        services.AddScoped<Maba.Application.Common.Interfaces.IAuditService, Services.AuditService>();

        // Register background workers
        services.AddHostedService<Workers.SlicingJobWorker>();
        services.AddHostedService<Workers.PrintJobWorker>();

        return services;
    }

    public static async Task SeedDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        await DatabaseSeeder.SeedAsync(context);
    }
}

