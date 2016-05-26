using GrislyGrotto.Models;
using System.Collections.Generic;

namespace GrislyGrotto.ViewModels
{
    public class ViewViewModel
    {
        public Post Post { get; set; }

        public List<CommentViewModel> GetCommentsExpanded()
        {
            return CommentViewModel.GetCommentsExpanded(Post);
        }

        public CommentViewModel NewComment { get; set; }
    }
}