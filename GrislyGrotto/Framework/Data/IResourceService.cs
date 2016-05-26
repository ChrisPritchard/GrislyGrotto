using System.Web;
using GrislyGrotto.Framework.Data.Primitives;

namespace GrislyGrotto.Framework.Data
{
    public interface IResourceService
    {
        Quote RandomQuote();
        void ReturnFile(HttpContext fileRequestContext);
    }
}