using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace GrislyGrotto
{
    public class Post
    {
        [Key]
        public string Key { get; set; }
        [Required]
        public string Title { get; set; }
        [ForeignKey("Author_Username")]
        public virtual Author Author { get; set; }
        public DateTime Date { get; set; }

        [Required, Column(TypeName="ntext"), MaxLength]
        public string Content { get; set; }
        public int WordCount { get; set; }
        public bool IsStory { get; set; }
        
        public virtual List<Comment> Comments { get; set; }

        public void UpdateWordCount()
        {
            var stripped = Regex.Replace(Content, "<[^>]*>", string.Empty);
            WordCount = stripped.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length;
        }

        public string TitleAsKey()
        {
            return Regex.Replace(Title.Replace(" ", "-"), "[^A-Za-z0-9 -]+", string.Empty).ToLower();
        }

        public string DateFormatted => Date.Add(Program.NzTimeZone).ToString("hh:mm tt, dddd dd MMMM, yyyy");
    }
}