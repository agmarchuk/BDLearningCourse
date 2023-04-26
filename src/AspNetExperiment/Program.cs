using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", (HttpRequest request) => 
{
    string renderBody = "";
    if (request.Query["p"] == "generate")
    {
        XElement db = new XElement("db", Enumerable.Range(0, 100_000)
            .Select(i => new XElement("person", new XAttribute("id", i),
                new XElement("name", "Иванов" + i),
                new XElement("age", 33)
                )));
        db.Save("Database.xml");
    } 
    else if (request.Query["ss"].Count != 0)
    {
    XElement db = XElement.Load("Database.xml");
    string searchstring = request.Query["ss"];
    renderBody = 
    "<div>" + 
    db.Elements().Where(x => x.Element("name").Value.StartsWith(searchstring))
        .Select(x => 
            $@"<div>
                <a href='/?id={x.Attribute("id").Value}'> {x.Element("name").Value} </a>
            </div>")
        .Aggregate((sum, s) => sum + s) +
    "</div>";    
    }
    else if (request.Query["id"].Count != 0)
    {
        XElement db = XElement.Load("Database.xml");
        string id = request.Query["id"];
        var element = db.Elements().FirstOrDefault(x => x.Attribute("id").Value == id);
        if (element != null) 
        {
            string name = element.Element("name").Value;
            string age = element.Element("age").Value;
            renderBody = @$"
            {id} {name} {age}
            ";
        }
    }  
    else
    {
    XElement db = XElement.Load("Database.xml");
    renderBody =
    "<table>" + 
    db.Elements()
        .Select(x => 
            $@"<tr>
                <td> {x.Attribute("id").Value} </td>
                <td> {x.Element("name").Value} </td>
                <td> {x.Element("age").Value} </td>
            </tr>")
        .Aggregate((sum, s) => sum + s) +
    "</table>";    
    }


    return new HtmlResult(
@$"<html>
    <head>
        <meta charset='utf-8' >
    </head>
    <body>
        <div>
            <form method='get' action='/'>
                <input type='text' name='ss'/> 
            </form> 
        </div>
        {renderBody}
    </body>
</html>"    
);
}
);
app.Run();



