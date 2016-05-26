
namespace GrislyGrotto.ViewModels.Shared
{
    public class SingleTagViewModel
    {
        public int ID { get; set; }
        public string Text { get; set; }
        public string DisplayText
        {
            get { return Text.Replace('_', ' '); }
        }
    }
}