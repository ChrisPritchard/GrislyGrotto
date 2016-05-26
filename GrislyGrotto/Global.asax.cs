using AutoMapper;
using GrislyGrotto.Data;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using GrislyGrotto.ViewModels.Shared;

namespace GrislyGrotto
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            Mapper.CreateMap<Post, PostViewModel>();
            Mapper.CreateMap<Comment, CommentViewModel>();
            Mapper.CreateMap<PostType, PostTypeViewModel>();
            Mapper.CreateMap<User, UserViewModel>();
            Mapper.CreateMap<Tag, SingleTagViewModel>();
            Mapper.CreateMap<Quote, QuoteViewModel>();
        }
    }
}