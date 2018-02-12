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
                string id = context.Request.Query["id"];
                html = new XElement("html",
                    new XElement("head", new XElement("meta", new XAttribute("charset", "utf-8"), " ")),
                    new XElement("body",
                        new XElement("h1", "Database Web viewer"),
                        id == null ? 
                            new XElement("div",
                                new XElement("div", "Всего элементов: " + db.Elements().Count()),
                                db.Elements().Take(100).Select(el => new XElement("div",
                                    new XElement("a", new XAttribute("href", "?id=" + el.Attribute("id").Value), el.Element("name").Value))))
                          : new XElement("div", 
                                db.Elements()
                                .Select(el => new object[] { el.Attribute("id").Value, el.Element("name").Value, el.Element("age").Value })
                                .Where(tri => (string)tri[0] == id)
                                .Select(tri => new XElement("div", 
                                    new XElement("div", "id=" + (string)tri[0]),
                                    new XElement("div", "name=" + (string)tri[1]),
                                    new XElement("div", " age=" + (string)tri[2])))
                                .First()) 
                          ,
                    null));
                await context.Response.WriteAsync(html.ToString());
            });
        }
    }
}
