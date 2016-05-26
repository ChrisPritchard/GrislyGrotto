using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using GrislyGrotto.Framework.Handlers;

namespace GrislyGrotto.Framework
{
    public class RequestData
    {
        public string[] Segments { get; private set; }
        public Dictionary<string, string> FormCollection { get; private set; }

        private HttpMethod Method { get; set; }

        public RequestData(HttpRequest httpRequest)
        {
            Segments = httpRequest.Url.Segments
                .Where(s => !s.Equals("/"))
                .Select(s => s.Replace("/", string.Empty))
                .ToArray();

            FormCollection = new Dictionary<string, string>();
            foreach (var key in httpRequest.Form.AllKeys)
                FormCollection.Add(key, httpRequest.Form[key]);

            Method = (HttpMethod)Enum.Parse(typeof(HttpMethod), httpRequest.HttpMethod.ToUpper());
        }

        public XElement ProcessUsingHandler(IHandler handler)
        {
            var handlerResponse = new XElement(handler.GetType().Name.Replace("Handler", string.Empty));

            var results = Method == HttpMethod.GET ? handler.Get(this) : handler.Post(this);
            foreach(var result in results)
            {
                if(result.GetType().Equals(typeof(string)))
                    handlerResponse.Add(new XElement((string)result));
                if (result.GetType().Equals(typeof(KeyValuePair<string, string>)))
                {
                    var flag = (KeyValuePair<string, string>) result;
                    handlerResponse.Add(new XElement(flag.Key, flag.Value));
                }
                else if (typeof(IEnumerable).IsAssignableFrom(result.GetType()))
                {
                    foreach (var o in (IEnumerable)result)
                        handlerResponse.Add(o.AsXml());
                }
                else
                    handlerResponse.Add(result.AsXml());
            }

            return handlerResponse;
        }

        public enum HttpMethod
        {
            GET, POST
        }
    }
}