using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Task06_Web
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            XElement db = XElement.Load("../../Databases/db3.xml");

            app.Run(async (context) =>
            {
                XElement html = new XElement("html", 
                    new XElement("head", new XElement("meta", new XAttribute("charset", "utf-8"), " ")),
                    new XElement("body",
                        new XElement("h1", "Start Web test"),
                        new XElement("div", "DateTime: " + DateTime.Now.ToString()),
                        new XElement("div", "Всего элементов: " + db.Elements().Count()),
                    null));
                
                await context.Response.WriteAsync(html.ToString());
            });
        }
    }
}
