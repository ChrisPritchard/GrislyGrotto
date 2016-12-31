using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrislyGrotto
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