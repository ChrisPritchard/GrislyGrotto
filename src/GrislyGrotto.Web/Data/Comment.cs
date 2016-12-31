using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrislyGrotto
{
    public class Comment
    {
        public int Id { get; set; }
        [ForeignKey("Post_Key")]
        public virtual Post Post { get; set; }

        [Required]
        public string Author { get; set; }
        [Required]
        public string Content { get; set; }
        public DateTime Date { get; set; }

        public string DateFormatted => Date.Add(Program.NzTimeZone).ToString("hh:mm tt, dddd dd MMMM, yyyy");
    }
}