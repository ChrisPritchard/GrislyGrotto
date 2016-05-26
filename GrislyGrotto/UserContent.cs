using System.IO;
using System.Linq;
using System.Web;

namespace GrislyGrotto
{
    public class UserContent : IHttpHandler
    {
        const string handlerPath = "/UserContent/";

        public void ProcessRequest(HttpContext context)
        {
            var data = new GrislyGrottoEntitiesAzure();

            if (context.Request.HttpMethod == "POST")
            {
                TryUploadFile(context, data);
                return;
            }

            string key;
            if (context.Request.Path.Length <= handlerPath.Length
                || string.IsNullOrWhiteSpace(key = context.Request.Path.Substring(handlerPath.Length)))
            {
                WriteAssetsList(context, data);
                return;
            }

            key = key.ToLower();

            var file = data.Assets.FirstOrDefault(f => f.Key.ToLower().Equals(key) || f.Key.ToLower().Equals(key.Replace("/", "\\")));
            if (file == null)
                WriteAssetsList(context, data);
            else
            {
                context.Response.ContentType = GetContentTypeFor(Path.GetExtension(file.Key));
                context.Response.BinaryWrite(file.Data);
            }
        }

        private void TryUploadFile(HttpContext context, GrislyGrottoEntitiesAzure data)
        {
            if (context.Request.Files == null || context.Request.Files.Count == 0)
            {
                context.Response.Redirect("/upload.htm?success=false");
                return;
            }

            var file = context.Request.Files[0];
            var content = new BinaryReader(file.InputStream).ReadBytes((int)file.InputStream.Length);

            var fileName = Path.GetFileName(file.FileName);
            var customFilename = context.Request.Form["customFilename"];
            if (!string.IsNullOrWhiteSpace(customFilename))
                fileName = Path.HasExtension(customFilename) ? customFilename : Path.Combine(customFilename, fileName);
            if (fileName.StartsWith("/"))
                fileName = fileName.Substring(1);

            var existing = data.Assets.FirstOrDefault(a => a.Key.ToLower().Equals(fileName.ToLower()));
            if(existing != null)
                existing.Data = content;
            else
                data.Assets.Add(new Asset { Key = fileName, Data = content });

            data.SaveChanges();

            context.Response.Redirect("/upload.htm?success=true&key="+fileName);
        }

        private void WriteAssetsList(HttpContext context, GrislyGrottoEntitiesAzure data)
        {
            const string basicLink = "<a href='{0}'>{0}</a><br/>";

            var allAssets = data.Assets.Select(a => a.Key).OrderBy(a => a).ToArray()
                .Select(a => string.Format(basicLink, "/UserContent/" + a.Replace("\\", "/"))).ToArray();
            context.Response.Write(string.Concat(allAssets));
        }

        private string GetContentTypeFor(string extension)
        {
            if (extension.StartsWith(".") && extension.Length > 1)
                extension = extension.Substring(1);
            switch (extension.ToLower())
            {
                case "xap":
                    return "application/x-silverlight-app";
                case "jpg":
                case "jpeg":
                    return "image/jpeg";
                case "bmp":
                    return "image/bitmap";
                case "gif":
                    return "image/gif";
                case "png":
                    return "image/png";
                case "txt":
                    return "text/plain";
                default:
                    return "text/html";
            }
        }

        public bool IsReusable
        {
            get { return true; }
        }
    }
}