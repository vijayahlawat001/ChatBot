using CustomAuthenticationMVC.CustomAuthentication;
using Newtonsoft.Json;
using SuperManagerChatBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;

namespace SuperManagerChatBot
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
        protected void Application_PostAuthenticateRequest(Object sender, EventArgs e)
        {
            HttpCookie authCookie = Request.Cookies["SuperManagerAuth"];
            if (authCookie != null)
            {
                FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie.Value);

                var serializeModel = JsonConvert.DeserializeObject<UserModel>(authTicket.UserData);
                CustomPrincipal principal = new CustomPrincipal(authTicket.Name);
                principal.UserId = serializeModel.EmailID;
                principal.Email = serializeModel.EmailID;
                principal.FirstName = serializeModel.UserName;
                principal.Roles = new[] { serializeModel.RoleName };

                HttpContext.Current.User = principal;
            }
        }
    }
}
