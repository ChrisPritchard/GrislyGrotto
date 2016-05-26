using GrislyGrotto.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace GrislyGrotto.ViewModels
{
    public class EditorViewModel
    {
        [Required]
        public string Title { get; set; }
        [Required, MinLength(500, ErrorMessage = "Content must be at least 500 characters")]
        public string Content { get; set; }
        public bool IsStory { get; set; }

        public int WordCount
        {
            get 
            {
                var stripped = Regex.Replace(Content, "<[^>]*>", string.Empty);
                return stripped.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                    .Length;
            }
        }

        public string Key
        {
            get
            {
                return Regex.Replace(Title.Replace(" ", "-"), "[^A-Za-z0-9 -]+", string.Empty)
                    .ToLowerInvariant();
            }
        }
    }
}