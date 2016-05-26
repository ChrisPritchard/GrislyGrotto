using System;
using System.Web;
using GrislyGrotto.Core;
using Microsoft.Practices.Unity;

namespace GrislyGrotto.Website
{
    public class Global : HttpApplication
    {
        public static IUnityContainer Container { get; private set; }

        protected void Application_Start(object sender, EventArgs e)
        {
            if(Container != null)
                return;

            Container = new UnityContainer();

            var connection = string.Format("Data Source={0};Pooling=true;FailIfMissing=true", 
                HttpContext.Current.Server.MapPath(@"App_Data\GrislyGrotto.db3"));
            Container.RegisterInstance(typeof(DAL.SQLite.ConnectionInfo), new DAL.SQLite.ConnectionInfo { ConnectionString = connection });
            Container.RegisterType(typeof(IUserService), typeof(DAL.SQLite.UserService));
            Container.RegisterType(typeof(IPostService), typeof(DAL.SQLite.PostService));
        }
    }
}