using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;

[assembly: OwinStartupAttribute(typeof(GrislyGrotto.OwinStartup), "Configure")]
namespace GrislyGrotto
{
    public class OwinStartup
    {
        public void Configure(IAppBuilder app)
        {
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = "Cookie",
                LoginPath = new PathString("/Account/Login")
            });
        }
    }
}