using System.Collections.Generic;

namespace GrislyGrotto.Framework.Handlers
{
    public interface IHandler
    {
        IEnumerable<object> Get(RequestData requestData);

        IEnumerable<object> Post(RequestData requestData);
    }
}