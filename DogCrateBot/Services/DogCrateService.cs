using Newtonsoft.Json.Linq;

namespace DogCrateBot.Services
{
    public class DogCrateService
    {
        private readonly DiscordSocketClient _client;
        private readonly ILogger _logger;
        private readonly IConfiguration _config;

        public DogCrateService(
            DiscordSocketClient client,
            ILogger<DogCrateService> logger,
            IConfiguration config)
        {
            _client = client;
            _logger = logger;
            _config = config;

            _client.UserVoiceStateUpdated += OnUserVoiceStateUpdated;

            var clt = new CancellationTokenSource();
            var token = clt.Token;
            _ = Run(() => DisconnectUser(ulong.Parse(_config["TargetUserId"]!)), TimeSpan.FromSeconds(10), token);
        }

        private Task OnUserVoiceStateUpdated(SocketUser user, SocketVoiceState state1, SocketVoiceState state2)
        {
            if (user is not SocketGuildUser guildUser)
            {
                return Task.CompletedTask;
            }

            if (user.Id == ulong.Parse(_config["TargetUserId"]!) &&
                state1.VoiceChannel is null &&
                state2.VoiceChannel is not null &&
                IsTimeToSleep())
            {
                guildUser.ModifyAsync(x =>
                {
                    x.Channel = null;
                });
            }

            return Task.CompletedTask;
        }

        private void DisconnectUser(ulong id)
        {
            if (!IsTimeToSleep())
            {
                return;
            }

            var guild = _client.GetGuild(ulong.Parse(_config["TargetGuildId"]!));

            foreach (var channel in guild.VoiceChannels)
            {
                foreach (var user in channel.Users)
                {
                    if (user.Id == id)
                    {
                        user.ModifyAsync(x => x.Channel = null);
                    }
                }
            }
        }

        private async Task Run(Action action, TimeSpan period, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(period, cancellationToken);

                if (!cancellationToken.IsCancellationRequested)
                {
                    action();
                }
            }
        }

        private bool IsTimeToSleep()
        {
            var currentDayOfWeek = DateTime.Now.AddHours(2).DayOfWeek;

            if (currentDayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            {
                return false;
            }

            int hour = DateTime.UtcNow.AddHours(2).Hour;

            return hour is >= 1 and <= 5;
        }
    }
}
