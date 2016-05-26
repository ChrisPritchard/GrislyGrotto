
namespace GrislyGrotto.ViewModels.Shared
{
    public class TagCountViewModel
    {
        public string TagName { get; set; }
        public string DisplayTagName { get { return TagName.Replace('_', ' '); } }
        public int Count { get; set; }
    }
}