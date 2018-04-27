using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Nancy.Owin;

namespace AP.Search.Web.Nancy
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {            
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseOwin(x => x.UseNancy());
        }
    }
}
