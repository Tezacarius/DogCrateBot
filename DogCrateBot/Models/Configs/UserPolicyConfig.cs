namespace DogCrateBot.Models.Configs
{
    public class UserPolicyConfig
    {
        public ulong Id { get; set; }
        public List<DayOfWeek> IgnoredDaysOfWeek { get; set; } = new();
        public int FromHour { get; set; }
        public int ToHour { get; set; }
    }
}
