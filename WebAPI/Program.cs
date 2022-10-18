using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.OpenApi.Models;
using WebAPI.Controllers;
using WebAPI.Repository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions( 
    x => x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter())
);
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
        
builder.Services.AddScoped<IUserRepository<ProfileController>, UserRepository>();
builder.Services.AddScoped<IForumRepository, ForumRepository>();


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
        app.UseHttpsRedirection();
    }
        
    app.UseSwaggerUI( c =>
    {
        c.InjectStylesheet("../css/swagger.css");
        c.SwaggerEndpoint("/swagger/cv/swagger.json", "API v1");
        c.RoutePrefix = "api";
    });
}



app.UseAuthorization();

app.UseStaticFiles();
    
app.MapControllers();

app.Run();