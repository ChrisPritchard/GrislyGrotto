using System.ComponentModel.DataAnnotations;

namespace GrislyGrotto.App.Data
{
    public class Author
    {
        [Required, Key]
        public string Username { get; set; }
        [Required]

        public string Password { get; set; }
        public string DisplayName { get; set; }
        public string ImageUrl { get; set; }
    }
}