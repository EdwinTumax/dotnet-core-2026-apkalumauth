using System.Text;
using ApiKalumAuth.DBContext;
using ApiKalumAuth.DTOs;
using ApiKalumAuth.Entities;
using ApiKalumAuth.Middlewares;
using ApiKalumAuth.Repositories.Interfaces;
using ApiKalumAuth.Shared.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

//Configuration Logger to File
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "{Message:lj}{NewLine}")
    .WriteTo.Logger(lc => lc.Filter.ByIncludingOnly(le => le.Level == LogEventLevel.Information && le.Properties.ContainsKey("SourceContext") && le.Properties["SourceContext"].ToString().Contains("ApiKalumAuth.Shared.Helpers"))
    .WriteTo.File("logs/logEventKalumAuthApi.out", outputTemplate: "{Message:lj}{NewLine}", rollingInterval: RollingInterval.Day))
    .WriteTo.Logger(lc => lc.Filter.ByIncludingOnly(le => le.Level == LogEventLevel.Error && le.Properties.ContainsKey("SourceContext") && le.Properties["SourceContext"].ToString().Contains("ApiKalumAuth.Shared.Helpers"))
    .WriteTo.File("logs/logEventKalumAuthApi.error", outputTemplate: "{Message:lj}{NewLine}", rollingInterval: RollingInterval.Day))
    .CreateLogger();
// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Host.UseSerilog();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowKalumApp", policy =>
    {
       policy.AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod(); 
    });
});
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddTransient<IUtils,Utils>();
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddDbContext<KalumAuthContext>(options => options.UseSqlServer("name=KalumAuth"));
builder.Services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<KalumAuthContext>().AddDefaultTokenProviders();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetValue<string>("LocalServer:Auth:Key"))),
        ClockSkew = TimeSpan.Zero
    };
    options.Events = new JwtBearerEvents
    {
      OnChallenge = async context =>
      {
          context.HandleResponse();
          context.Response.StatusCode = StatusCodes.Status401Unauthorized;
          context.Response.ContentType = "Application/json";
          var response = new ApiResponseDTO<Object>
          {
            Success = false,
            Message = "No esta autenticado",
            Data = null,
            Errors = new List<string>
            {
                "Debe enviar un token JWT válido"
            }  
          };
          await context.Response.WriteAsJsonAsync(response);
      }  
    };
});

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
      var errors = context.ModelState
        .Where(ms => ms.Value?.Errors.Count > 0)
        .SelectMany(ms => ms.Value!.Errors)
        .Select(e => e.ErrorMessage)
        .ToList();
        var response = ApiResponseDTO<Object>.Fail("Error de validacion", errors);
        return new BadRequestObjectResult(response);  
    };
});

var app = builder.Build();
app.UseCors("AllowKalumApp");
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
//app.UseHttpsRedirection();
app.MapControllers();
app.Run();

