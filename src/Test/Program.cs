using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", (HttpRequest request) => 
{
    XElement db = XElement.Load("Database.xml");
    string renderBody =
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
return new HtmlResult(
@$"<html>
    <head>
        <meta charset='utf-8' >
    </head>
    <body>
        {renderBody}
    </body>
</html>"    
);}
    );

app.Run();
