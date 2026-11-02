using AskYourBlog.Api.Composition;
using AskYourBlog.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.Services.AddRag(builder.Configuration);

var app = builder.Build();
app.MapDefaultEndpoints();
app.MapAsk();
app.MapIngest(app.Configuration);

app.Run();