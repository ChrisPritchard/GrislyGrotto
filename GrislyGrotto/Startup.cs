using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

[assembly: UserSecretsId("aspnet-grislygrotto-b1df08ac-697f-446b-a583-604dd8754733")]

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

            if(env.IsDevelopment())
                builder.AddUserSecrets<Startup>();

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

            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
                app.UseExceptionHandler("/Error");

            app.UseCookieAuthentication(new CookieAuthenticationOptions()
            {
                AuthenticationScheme = "Cookie",
                LoginPath = new PathString("/Login"),
                AccessDeniedPath = new PathString("/Login"),
                AutomaticAuthenticate = true,
                AutomaticChallenge = true
            });

            app.UseStaticFiles();
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