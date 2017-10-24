using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace GrislyGrotto.Web
{
    public class WrimoController : Controller
    {
        [Route("Wrimo/{username}")]
        public async Task<IActionResult> Wrimo(string username)
        {
            const string url = "http://nanowrimo.org/wordcount_api/wchistory/";

            using (var httpClient = new HttpClient())
            {
                var xml = await httpClient.GetStringAsync(url + username);
                var serializer = new XmlSerializer(typeof(wchistory));
                var objects = serializer.Deserialize(new StringReader(xml));
                return new JsonResult(objects);
            }
        }
        
        public class wchistory
        {
            public string uname { get; set; }
            public int user_wordcount { get; set; }
            public bool winner { get; set; }
            public wcentry[] wordcounts { get; set; }
        }

        public class wcentry
        {
            public int wc { get; set; }
            public string wcdate { get; set; }
        }
    }
}