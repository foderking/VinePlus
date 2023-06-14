using System.Reflection;
using System.Text.Json.Serialization;
using ComicVine.API;
// using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
// using ComicVine.API;
// using ComicVine.API.Database;
using Comicvine.Core;
using Comicvine.Database;
using Comicvine.Polling;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true); // https://stackoverflow.com/questions/69961449/net6-and-datetime-problem-cannot-write-datetime-with-kind-utc-to-postgresql-ty

builder.Services.AddControllers().AddJsonOptions( 
    x => x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter())
);
builder.Services.AddHostedService<PollingWorker>(); // adds the pollng background service
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(
    c =>
    {
        c.SwaggerDoc("cv",
            new OpenApiInfo { Title = "Comicvine API", Description = "Comicvine Unofficial API", Version = "v1" });
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFile));
    });
builder.Services.AddRazorPages();
builder.Services.RegisterDataServices(builder.Configuration); // inject dbcontext
// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis?view=aspnetcore-6.0#authorization
// builder.Services.AddDbContext<ComicvineContext>(o => 
//     // o.UseNpgsql(builder.Configuration.GetConnectionString("comicvine_db"))
// );
// builder.Services.AddStackExchangeRedisCache(o => { o.Configuration = builder.Configuration["RedisCacheUrl"]; });

// builder.Services.AddSingleton<ComicvineContextFactory>();
var app = builder.Build();

var port = Environment.GetEnvironmentVariable("PORT");

if (!string.IsNullOrWhiteSpace(port)) {
    app.Urls.Add("http://*:" + port);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsProduction()) {
    app.UseSwagger();

    if (app.Environment.IsProduction()) {
        app.UseExceptionHandler("/error");
        // app.UseHttpsRedirection();
    }
        
    app.UseSwaggerUI( c =>
    {
        c.InjectStylesheet("../css/swagger.css");
        c.SwaggerEndpoint("/swagger/cv/swagger.json", "API v1");
        c.RoutePrefix = "api";
    });
}

// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/openapi?view=aspnetcore-7.0
app.UseSwaggerUI();
app.UseSwagger();

// app.UseAuthorization();

app.UseStaticFiles();

app.MapRazorPages();
app.AddEndpoints();

app.Run();