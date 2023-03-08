namespace HoU.GuildBot.Keycloak;

public static class ServiceCollectionExtensions
{
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static IServiceCollection AddKeycloak(this IServiceCollection services)
    {
        services.AddHttpClient("keycloak")
                .ConfigureHttpClient(client =>
                 {
                     client.DefaultRequestHeaders.Accept.Clear();
                     client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                 });

        services.AddSingleton<IBearerTokenManager<KeycloakBaseClient>, BearerTokenManager<KeycloakBaseClient>>()
                .AddTransient<IKeycloakUserReader, KeycloakUserReader>()
                .AddTransient<IKeycloakUserWriter, KeycloakUserWriter>()
                .AddTransient<IKeycloakUserEraser, KeycloakUserEraser>()
                .AddTransient<IKeycloakGroupReader, KeycloakGroupReader>()
                .AddTransient<IKeycloakUserGroupWriter, KeycloakUserGroupWriter>()
                .AddTransient<IKeycloakSyncService, KeycloakSyncService>()
                .AddTransient<IKeycloakUserGroupAggregator, KeycloakUserGroupAggregator>()
                .AddTransient<IKeycloakUserGroupAssigner, KeycloakUserGroupAssigner>()
                .AddTransient<IKeycloakUserCreator, KeycloakUserCreator>()
                .AddTransient<IKeycloakUserUpdater, KeycloakUserUpdater>()
                .AddTransient<IKeycloakUserCleaner, KeycloakUserCleaner>()
                .AddTransient<IKeycloakDiscordComparer, KeycloakDiscordComparer>();
        
        return services;
    }    
}