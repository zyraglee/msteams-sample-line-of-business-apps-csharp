using System.Configuration;
namespace Microsoft.Teams.Samples.HelloWorld.Web.Helper
{
    public static class ApplicationSettings
    {
        public static string BaseUrl { get; set; }

        public static string ConnectionName { get; set; }

        static ApplicationSettings()
        {
            ConnectionName = ConfigurationManager.AppSettings["ConnectionName"];
            BaseUrl = ConfigurationManager.AppSettings["BaseUri"];
        }
    }
}