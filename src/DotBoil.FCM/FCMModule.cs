using DotBoil.Configuration;
using DotBoil.Dependency;
using DotBoil.FCM.Configuration;
using DotBoil.FCM.Endpoints;
using DotBoil.FCM.Persistence;
using DotBoil.FCM.Services;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DotBoil.FCM
{
    internal class FCMModule : Module
    {
        // Host uygulamanın kendi default FirebaseApp'i ile çakışmaması için isimli instance kullanılır
        private const string FirebaseAppName = "DotBoil.FCM";

        public override string Name => "FCM";
        public override IEnumerable<string> DependsOn { get; } = Enumerable.Empty<string>();
        public override int Order { get; } = 17;

        public override Task AddModule()
        {
            var options = DotBoilApp.Configuration.GetConfigurations<FCMOptions>();

            if (string.IsNullOrWhiteSpace(options.Persistence?.ConnectionString))
                throw new InvalidOperationException("DotBoil:FCM:Persistence:ConnectionString is required.");

            DotBoilApp.Services.AddSingleton(options);

            DotBoilApp.Services.AddDbContext<FCMDbContext>(dbOptions =>
            {
                dbOptions.UseMySQL(options.Persistence.ConnectionString);
            });

            DotBoilApp.Services.AddSingleton(FirebaseMessaging.GetMessaging(createFirebaseApp(options.Firebase)));

            // Constructor internal olduğu için DI kaydı factory ile yapılır
            DotBoilApp.Services.AddScoped(sp => new FCMService(
                sp.GetRequiredService<FCMDbContext>(),
                sp.GetRequiredService<FirebaseMessaging>()));
            DotBoilApp.Services.AddScoped<IFCMService>(sp => sp.GetRequiredService<FCMService>());

            return Task.CompletedTask;
        }

        public override async Task UseModule()
        {
            using var scope = DotBoilApp.Host.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<FCMDbContext>();

            try
            {
                await context.Database.MigrateAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            if (DotBoilApp.Host is WebApplication app)
            {
                app.MapFCMEndpoints();
            }
        }

        private static FirebaseApp createFirebaseApp(FCMFirebaseOptions options)
        {
            var existing = FirebaseApp.GetInstance(FirebaseAppName);
            if (existing is not null)
                return existing;

            var credentialJson = options?.CredentialJson;

            if (string.IsNullOrWhiteSpace(credentialJson))
            {
                if (string.IsNullOrWhiteSpace(options?.CredentialPath))
                    throw new InvalidOperationException(
                        "DotBoil:FCM:Firebase:CredentialPath or DotBoil:FCM:Firebase:CredentialJson is required.");

                if (!File.Exists(options.CredentialPath))
                    throw new FileNotFoundException(
                        "Firebase service account file could not be found.", options.CredentialPath);

                credentialJson = File.ReadAllText(options.CredentialPath);
            }

            var credential = CredentialFactory
                .FromJson<ServiceAccountCredential>(credentialJson)
                .ToGoogleCredential();

            return FirebaseApp.Create(new AppOptions { Credential = credential }, FirebaseAppName);
        }
    }
}
