using System.Threading.Channels;

namespace DogCrateBot.Services
{
    public class DiscordStartupService : IHostedService
    {
        private readonly DiscordSocketClient _client;
        private readonly DogCrateService _dogCrate;
        private readonly IConfiguration _config;

        public DiscordStartupService(
            DiscordSocketClient client,
            DogCrateService dogCrate,
            IConfiguration config,
            ILogger<DiscordSocketClient> logger)
        {
            _client = client;
            _dogCrate = dogCrate;
            _config = config;

            _client.Log += msg => LogHelper.OnLogAsync(logger, msg);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _client.LoginAsync(TokenType.Bot, _config["DISCORD_DOG_CRATE_TOKEN"]);
            await _client.StartAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _client.LogoutAsync();
            await _client.StopAsync();
        }
    }
}
