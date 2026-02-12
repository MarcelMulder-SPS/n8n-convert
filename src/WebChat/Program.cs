using Microsoft.Identity.Client;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configure MSAL PublicClientApplication
builder.Services.AddSingleton<IPublicClientApplication>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var clientId = config["AzureAd:ClientId"];
    var authority = config["AzureAd:Authority"];

    if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(authority))
    {
        throw new InvalidOperationException("AzureAd configuration is missing");
    }

    return PublicClientApplicationBuilder
        .Create(clientId)
        .WithAuthority(authority)
        .WithRedirectUri("http://localhost") // For PublicClient, this is standard
        .Build();
});

var app = builder.Build();

app.UseCors();
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapControllers();

app.Run();
