using Budget.DAL;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.WebApi;
using System.Web.Http;
using Unity.WebApi;


namespace Budget.API
{
    public static class UnityConfig
    {
        public static void RegisterComponents()
        {
			var container = new UnityContainer();
            
            // register all your components with the container here
            // it is NOT necessary to register your controllers

            // e.g. container.RegisterType<ITestService, TestService>();
            container.RegisterType<ApplicationDbContext>();

            GlobalConfiguration.Configuration.DependencyResolver = new UnityHierarchicalDependencyResolver(container);
        }
    }
}