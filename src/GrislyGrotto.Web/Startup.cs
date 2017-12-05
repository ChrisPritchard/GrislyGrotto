using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace GrislyGrotto
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var databaseConnectionString = Configuration["connectionStrings:database"];
            services.AddDbContext<GrislyGrottoDbContext>(options => options.UseSqlServer(databaseConnectionString));

            services.Configure<RazorViewEngineOptions>(options => 
            {
                options.ViewLocationExpanders.Add(new ShallowViewLocationExpander());
            });

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(o => o.LoginPath = new PathString("/login"));

            services.AddMvc();
            services.AddSession();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddDebug();
            app.UseDeveloperExceptionPage(); // even in prod - my site, want to see my bugs

            app.UseAuthentication();

            app.UseStaticFiles();
            app.UseSession();
            app.UseMvc();
        }
    }

    public class ShallowViewLocationExpander: IViewLocationExpander
    {
        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            // {2} is area, {1} is controller, {0} is the action
            return new[] { "/Views/{0}.cshtml" };
        }

        public void PopulateValues(ViewLocationExpanderContext context) 
        {
            context.Values["customviewlocation"] = nameof(ShallowViewLocationExpander);
        }
    }
}