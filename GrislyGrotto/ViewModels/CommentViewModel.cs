using GrislyGrotto.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace GrislyGrotto.ViewModels
{
	public class CommentViewModel
	{
        [Required]
		public string Author { get; set; }
		public DateTimeOffset Date { get; set; }
        [Required]
		public string Content { get; set; }

        public CommentViewModel()
        {
            Date = DateTimeOffset.Now.LocalDateTime;
        }

        public static List<CommentViewModel> GetCommentsExpanded(Post post)
        {
            if (post.Comments == null)
                return new List<CommentViewModel>();
            return JsonConvert.DeserializeObject<CommentViewModel[]>(post.Comments).ToList();
        }
	}
}