namespace GrislyGrotto.ViewModels.Shared
{
    public class UserViewModel
    {
        public int ID { get; set; }

        public string DisplayName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Salt { get; set; }
    }
}