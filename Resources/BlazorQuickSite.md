# BlazorQuickSite Эксперимент по быстрому (одно занятие) построению Blazor-сайта

Марчук А.Г., профессор

## Введение

Основная цель практикума показать нетипичную работу по формированию информационной системы. Нетипичность заключается в использовании технологий Asp.net/Blazor для сборки систеы, XML - для базы данных. Проект назовем BlazorQuickSite. 

## Эксперимент шаг за шагом

Создаем проект ASP.NET. В командной строке это делается созданием директории и запуском
```
dotnet new
```
Запускаем сгенерированный проект dotnet run и запускаем клиента, настроив его на соответсвующий протокол и порт.

Следующий шаг - обеспечить прием параметров. Это делается заменой пустых скобок в лямбда-выражении на передачу параметра. Чтобы выдать значение параметра, воспользуемся методом request.Query["p"]. Все вместе это будет

```
app.MapGet("/", (HttpRequest request) => 
    $"Привет {request.Query["p"]}!");
```

Следующий шаг: выдачу оформить как HTML. Для этого, результат надо выдавать не как строку, а как объект HtmlResult. Заменим строчку выдаюи на что-нибудь вроде:
```
new HtmlResult(
@$"<html>
    <head></head>
    <body>
        <h1>Привет {request.Query["p"]}!</h1>
    </body>
</html>    
);
```
Если не хватает, то добавим описание для HtmlResult.cs:
```
using System.Net.Mime;
using System.Text;

class HtmlResult : IResult
{
    private readonly string _html;

    public HtmlResult(string html)
    {
        _html = html;
    }

    public Task ExecuteAsync(HttpContext httpContext)
    {
        httpContext.Response.ContentType = MediaTypeNames.Text.Html;
        httpContext.Response.ContentLength = Encoding.UTF8.GetByteCount(_html);
        return httpContext.Response.WriteAsync(_html);
    }
}
```

Следующим шагом будет создание базы данных. Сделаем ее максимально простой, набор записей о персонах, состоящих из полей идентификации, имени и возраста. Сделаем это через файл Database.xml, помещенный в директорию wwwroot:
```
<database>
    <person id="1">
        <name>Иванов</name>
    </person>
    <person id="2">
        <name>Петров</name>
    </person>
    <person id="3">
        <name>Сидоров</name>
    </person>
</database>
```
В вычисляющем фрагменте надо базу данных прочитать и что-то из нее выделить. Для этого, лямбда-функцию снова расширим:
```
app.MapGet("/", (HttpRequest request) => 
{
    string renderBody = "Что-то новое ";
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
```
А в блоке перед return теперь можно помещать дополнительные операторы. 
Теперь в этом месте загрузим базу данных и выдадим ее как таблицу:
```
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
```
Также добавим пространство имен System.Xml.Linq

Теперь будем делать разные странички. Странички будут отличаться идентификатором страниы p. Сделаем страницу с генерацией и сохранением базы данных p=generate.

Теперь программа удлинится:
```
    string renderBody = "";
    if (request.Query["p"] == "generate")
    {
        XElement db = new XElement("db", Enumerable.Range(0, 100)
            .Select(i => new XElement("person", new XAttribute("id", i),
                new XElement("name", "Иванов" + i),
                new XElement("age", 33)
                )));
        db.Save("Database.xml");
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
```

Следующий шаг будет создании системы поиска по имени. Для этого, потребуется сделать форму ввода запроса, ее можно сделать для всех страниц прямо в начальном шаблоне. Сделаем там простую форму, которая будет посылать данные в режиме get
```

``` 