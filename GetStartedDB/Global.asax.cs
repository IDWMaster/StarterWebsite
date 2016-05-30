using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.IO;
using System.Reflection;

namespace GetStartedDB
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        private System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string fileName = args.Name.Substring(0, args.Name.IndexOf(","));
            if (Environment.Is64BitProcess)
            {
                var ably = Assembly.LoadFile(HttpContext.Current.Request.PhysicalApplicationPath + "bin/x64/" + fileName + ".dll");
                return ably;
            }
            else
            {
                var ably = Assembly.LoadFile(HttpContext.Current.Request.PhysicalApplicationPath + "bin/x86/" + fileName + ".dll");
                return ably;
            }
        }
    }
}
