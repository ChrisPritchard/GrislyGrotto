using GrislyGrotto.Models;
using Microsoft.Azure.Search.Models;
using System.Collections.Generic;
using System.Web.Mvc;

namespace GrislyGrotto.ViewModels
{
    public class SearchViewModel
    {
        public string FreeText { get; set; }
        public string Author { get; set; }
        public SelectListItem[] AuthorOptions = 
        {
            new SelectListItem { Text = "(All)", Value = "" },
            new SelectListItem { Text = "Christopher", Value = "Christopher" },
            new SelectListItem { Text = "Peter", Value = "Peter" }
        };
        public bool StoriesOnly { get; set; }
    }
}