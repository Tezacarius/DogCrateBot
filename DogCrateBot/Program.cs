var builder = Host.CreateApplicationBuilder(args);

builder.Configuration
    .AddEnvironmentVariables()
    .AddJsonFile("appsettings.json");

DiscordSocketConfig discordSocketConfig = new();
builder.Configuration.GetSection(nameof(DiscordSocketConfig)).Bind(discordSocketConfig);
var discordSocketClient = new DiscordSocketClient(discordSocketConfig);

builder.Services
    .AddSingleton(discordSocketClient)
    .AddSingleton<DogCrateService>()
    .AddHostedService<DiscordStartupService>()
    .AddHostedService<HttpListenerService>();

using var host = builder.Build();

await host.RunAsync();
