using System.IO;
using System.Reflection;
using System.Xml.Linq;

namespace GG7.DataExtractor
{
    class Program
    {
        static void Main()
        {
            var currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var dataAccess = DataAccess.Default(currentDirectory);

            XElement.Parse(dataAccess.RetrieveDataSet("select * from posts").GetXml()).Save(currentDirectory + @"\posts.xml");
            XElement.Parse(dataAccess.RetrieveDataSet("select * from comments").GetXml()).Save(currentDirectory + @"\comments.xml");
            XElement.Parse(dataAccess.RetrieveDataSet("select * from users").GetXml()).Save(currentDirectory + @"\users.xml");
        }
    }
}
