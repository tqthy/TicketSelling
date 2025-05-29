using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Secret"] ?? throw new InvalidOperationException("JWT Secret not found."))),
        };
        options.ClaimsIssuer = builder.Configuration["JwtSettings:Issuer"];
    });

builder.Services.AddOcelot(builder.Configuration);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // By default, ASP.NET Core trusts localhost. 
    // In a Docker Swarm, Traefik's IP is not localhost.
    // The safest approach is to clear known networks and proxies.
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

app.UseForwardedHeaders();
// app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>(); // Or a more specific logger category
    logger.LogWarning("--- CUSTOM AUTH DEBUG ---");
    if (context.User.Identity != null)
    {
        logger.LogWarning($"User.Identity.IsAuthenticated: {context.User.Identity.IsAuthenticated}");
        logger.LogWarning($"User.Identity.AuthenticationType: {context.User.Identity.AuthenticationType}");
        logger.LogWarning("User.Claims:");
        foreach (var claim in context.User.Claims)
        {
            logger.LogWarning($"  - Type: {claim.Type}, Value: {claim.Value}, Issuer: {claim.Issuer}");
        }
    }
    else
    {
        logger.LogWarning("User.Identity IS NULL");
    }
    logger.LogWarning("--- END CUSTOM AUTH DEBUG ---");
    await next(context);
});

app.MapControllers();

// Ocelot middleware
await app.UseOcelot();

app.Run();