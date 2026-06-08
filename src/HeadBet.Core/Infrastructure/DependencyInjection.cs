using FluentValidation;
using Headsoft.Core.Data.Extensions;
using Headsoft.Core.Extensions;
using Headsoft.Core.Interfaces.Repositories;
using Headsoft.Messaging.Extensions;
using HeadBet.Core.Domain.Interfaces;
using HeadBet.Core.Infrastructure.Data;
using HeadBet.Core.Infrastructure.Http;
using HeadBet.Core.Infrastructure.Identity;
using HeadBet.Core.Infrastructure.Localization;
using HeadBet.Core.Infrastructure.Scoring;
using HeadBet.Core.Infrastructure.Tournament;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HeadBet.Core.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCore(this IServiceCollection services, IConfiguration config)
    {
        // Assembly marker do Core — usado pra descobrir handlers, profiles e validators.
        var assembly = typeof(CoreAssemblyMarker).Assembly;

        // EF Core + SQLite
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(config.GetConnectionString(nameof(AppDbContext))));

        // Unit of Work
        services.AddUnitOfWork<AppUnitOfWork>();

        // Repositories
        services.AddImplementations<IRepository, CoreAssemblyMarker>();

        // Password hashing (interface em Domain, impl em Infrastructure)
        services.AddSingleton<IPasswordHasher, PasswordHasher>();

        // AI
        services.AddSingleton<IAgentFactory, AI.AgentFactory>();

        // Settings
        services.AddScoped<ISettingsProvider, SettingsProvider>();

        // E-mail (SMTP via System.Net.Mail) — config na seção "Email" do appsettings
        var emailSettings = new Email.EmailSettings
        {
            Host = config["Email:Host"] ?? string.Empty,
            Port = int.TryParse(config["Email:Port"], out var emailPort) ? emailPort : 587,
            EnableSsl = !bool.TryParse(config["Email:EnableSsl"], out var emailSsl) || emailSsl,
            User = config["Email:User"] ?? string.Empty,
            Password = config["Email:Password"] ?? string.Empty,
            From = config["Email:From"] ?? string.Empty,
            FromName = config["Email:FromName"] ?? "HeadBet",
            TimeoutSeconds = int.TryParse(config["Email:TimeoutSeconds"], out var emailTimeout) ? emailTimeout : 30,
        };
        services.AddSingleton(emailSettings);
        // Singleton: SmtpEmailSender é stateless (só EmailSettings + logger).
        services.AddSingleton<IEmailSender, Email.SmtpEmailSender>();
        // Fila de e-mail: handlers enfileiram (instantâneo) e o BackgroundService drena/envia.
        services.AddSingleton<Email.EmailQueue>();
        services.AddSingleton<IEmailQueue>(sp => sp.GetRequiredService<Email.EmailQueue>());
        services.AddHostedService<Email.EmailBackgroundService>();

        // Scoring
        services.AddScoped<IMatchScoringService, MatchScoringService>();

        // Tournament import (compartilhado entre fluxo IA e wizard)
        services.AddScoped<ITournamentImporter, TournamentImporter>();

        // Notification translation (translate keys → pt-BR titles)
        services.AddSingleton<INotificationTranslator, NotificationTranslator>();

        // Chat em tempo real (pub/sub em memória; um processo, um circuito por usuário)
        services.AddSingleton<IChatBroadcaster, Chat.ChatBroadcaster>();

        // Football Data API
        services.AddHttpClient<IFootballDataClient, FootballDataClient>(client =>
        {
            client.BaseAddress = new Uri("https://api.football-data.org/");
            client.DefaultRequestHeaders.Add("X-Auth-Token", config["FootballData:ApiKey"]);
        });

        // AutoMapper (Profiles ficam em Core)
        services.AddAutoMapper(assembly);

        // Mediator
        services.AddMessaging<CoreAssemblyMarker>();

        // FluentValidation
        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}
