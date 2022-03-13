using Service.Hubs;
using Service;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();
builder.Services.AddCors(o => 
{
    o.AddDefaultPolicy(b =>
    {
        b.WithOrigins("http://localhost:3000", "http://192.168.0.120:3000")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});
builder.Services.AddSingleton<IDictionary<string, UserConnection>>(o => new Dictionary<string, UserConnection>());
var app = builder.Build();

app.UseCors();
app.MapHub<ChatHub>("/chat");

app.Run();
