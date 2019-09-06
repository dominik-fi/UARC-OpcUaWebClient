using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Uarc.Core;

namespace Uarc.Web
{
    public class Program
    {
        public static UarcCoreAPI UarcCollector { get;  internal set; }

        public static void Main(string[] args)
        {
            UarcCollector = new UarcCoreAPI();

            CreateWebHostBuilder(args)
                //.UseSetting("https_port", "8080")
                .Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            //var pathToExe = Process.GetCurrentProcess().MainModule.FileName;
            //var pathToContentRoot = Path.GetDirectoryName(pathToExe);

            return WebHost.CreateDefaultBuilder(args)
                   .UseStartup<Startup>()
                   //.UseContentRoot(pathToContentRoot)
                   .UseDefaultServiceProvider(options => options.ValidateScopes = false);
        }
    }
}
