using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add ocelot configuration
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
builder.Services.AddOcelot(builder.Configuration);

// Add jwt bearer
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            RequireExpirationTime = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration.GetSection("JwtConfig:Issuer").Value,
            ValidAudience = builder.Configuration.GetSection("JwtConfig:Audience").Value,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8
                .GetBytes(builder.Configuration.GetSection("JwtConfig:Secret").Value))
        };
    });

var app = builder.Build();

app.UseOcelot().Wait();

app.Run();


//using Ocelot.DependencyInjection;
//using Ocelot.Middleware;
//using Microsoft.AspNetCore.Authentication.JwtBearer;
//using System.Text;
//using Microsoft.IdentityModel.Tokens;




//namespace OcelotBasic
//{
//    public class Program
//    {
//        public static void Main(string[] args)
//        {
//            new WebHostBuilder()
//            .UseKestrel()
//            .UseContentRoot(Directory.GetCurrentDirectory())
//            .ConfigureAppConfiguration((hostingContext, config) =>
//            {
//                config
//                    .SetBasePath(hostingContext.HostingEnvironment.ContentRootPath)
//                    .AddJsonFile("appsettings.json", true, true)
//                    .AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", true, true)
//                    .AddJsonFile("ocelot.json")
//                    .AddEnvironmentVariables();
//            })
//            .ConfigureServices(s =>
//            {
//                s.AddOcelot();
//                // Configure your logging here
//                s.AddLogging(logging =>
//                {
//                    logging.ClearProviders();
//                    logging.AddConsole(); // Add console logging, you can add other providers as needed
//                                          // Add other logging providers here
//                });
//            })
//            .ConfigureLogging((hostingContext, logging) =>
//            {
//                //add your logging
//            })
//            .UseIISIntegration()
//            .Configure(app =>
//            {
//                app.UseOcelot().Wait();
//            })
//            .Build()
//            .Run();
//        }
//    }
//}
