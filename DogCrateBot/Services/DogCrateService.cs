namespace DogCrateBot.Services
{
    public class DogCrateService
    {
        private readonly DiscordSocketClient _client;
        private readonly ILogger _logger;
        private readonly IConfiguration _config;
        private readonly List<UserPolicyConfig> _userPolicies;

        public DogCrateService(
            DiscordSocketClient client,
            ILogger<DogCrateService> logger,
            IConfiguration config)
        {
            _client = client;
            _logger = logger;
            _config = config;
            _userPolicies = _config.GetSection("UsersPolicyConfig").Get<List<UserPolicyConfig>>()!;

            _client.UserVoiceStateUpdated += OnUserVoiceStateUpdated;

            var clt = new CancellationTokenSource();
            var token = clt.Token;
            _ = Run(DisconnectUser, TimeSpan.FromSeconds(10), token);
        }

        private Task OnUserVoiceStateUpdated(SocketUser user, SocketVoiceState state1, SocketVoiceState state2)
        {
            if (user is not SocketGuildUser guildUser)
            {
                return Task.CompletedTask;
            }

            if (state1.VoiceChannel is null &&
                state2.VoiceChannel is not null &&
                IsTargetUser(user.Id) &&
                IsTimeToSleep(user.Id))
            {
                guildUser.ModifyAsync(x =>
                {
                    x.Channel = null;
                });
            }

            return Task.CompletedTask;
        }

        private void DisconnectUser()
        {
            var guild = _client.GetGuild(ulong.Parse(_config["TargetGuildId"]!));

            foreach (var userPolicy in _userPolicies)
            {
                if (!IsTimeToSleep(userPolicy.Id))
                {
                    return;
                }

                foreach (var channel in guild.VoiceChannels)
                {
                    foreach (var user in channel.Users)
                    {
                        if (user.Id == userPolicy.Id)
                        {
                            user.ModifyAsync(x => x.Channel = null);
                        }
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

        private bool IsTimeToSleep(ulong id)
        {
            var timeZome = int.Parse(_config["TimeZone"]!);
            var userPolicy = _userPolicies.Find(u => u.Id == id)!;
            var currentDayOfWeek = DateTime.Now.AddHours(timeZome).DayOfWeek;
            var currentHour = DateTime.UtcNow.AddHours(timeZome).Hour;

            var ignoredDays = userPolicy.IgnoredDaysOfWeek.Contains(currentDayOfWeek);

            if (userPolicy.IgnoredDaysOfWeek.Contains(currentDayOfWeek))
            {
                return false;
            }

            return currentHour >= userPolicy.FromHour && currentHour < userPolicy.ToHour;
        }

        private bool IsTargetUser(ulong id)
        {
            foreach (var userPolicy in _userPolicies)
            {
                if (userPolicy.Id == id)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
