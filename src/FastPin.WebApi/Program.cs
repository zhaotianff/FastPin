using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var app = builder.Build();

app.MapOpenApi("/openapi/v1.json");

app.MapGet("/health", () => Results.Ok(new
{
    status = "ok",
    environment = app.Environment.EnvironmentName,
    timestamp = DateTimeOffset.UtcNow
}));

app.MapGet("/debug", (HttpContext context) =>
{
    var baseUrl = $"{context.Request.Scheme}://{context.Request.Host}";
    var html = $$"""
                 <!DOCTYPE html>
                 <html lang="en">
                 <head>
                   <meta charset="utf-8" />
                   <meta name="viewport" content="width=device-width, initial-scale=1" />
                   <title>FastPin Web API Debug</title>
                   <style>
                     body { font-family: sans-serif; margin: 24px; }
                     code, pre { background: #f5f5f5; padding: 2px 6px; border-radius: 4px; }
                     .ok { color: #0b7a31; }
                   </style>
                 </head>
                 <body>
                   <h1>FastPin Web API Debug</h1>
                   <p>Environment: <strong>{{app.Environment.EnvironmentName}}</strong></p>
                   <ul>
                     <li>Health endpoint: <a href="/health"><code>/health</code></a></li>
                     <li>OpenAPI document: <a href="/openapi/v1.json"><code>/openapi/v1.json</code></a></li>
                   </ul>
                   <h2>Live health response</h2>
                   <pre id="health-result">loading...</pre>
                   <script>
                     fetch('{{baseUrl}}/health')
                       .then(r => r.json())
                       .then(d => {
                         document.getElementById('health-result').textContent = JSON.stringify(d, null, 2);
                       })
                       .catch(e => {
                         document.getElementById('health-result').textContent = 'Failed: ' + e;
                       });
                   </script>
                 </body>
                 </html>
                 """;

    return Results.Content(html, "text/html", Encoding.UTF8);
});

app.MapGet("/", () => Results.Redirect("/debug"));

app.Run();
