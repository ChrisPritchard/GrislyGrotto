using System;
using System.Net;
using System.IO;

namespace Shell.FtpUploader
{
    static class Program
    {
        private const string ftpUrl = "ftp://ftp.grislygrotto.co.nz/httpdocs/UserContent/";
        private const string ftpUsername = "zgrislyg4721nz";
        private const string ftpPassword = "54j8F6";

        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Arguments: <Local File>");
                return;
            }

            var dataToWrite = File.ReadAllBytes(args[0]);

            var ftpConnection = (FtpWebRequest) WebRequest.Create(new Uri(ftpUrl + Path.GetFileName(args[0])));
            ftpConnection.Credentials = new NetworkCredential(ftpUsername, ftpPassword);

            ftpConnection.Method = WebRequestMethods.Ftp.UploadFile;
            using(var stream = ftpConnection.GetRequestStream())
            {
                stream.Write(dataToWrite, 0, dataToWrite.Length);
            }

            Console.WriteLine("File Uploaded Successfully");
        }
    }
}
