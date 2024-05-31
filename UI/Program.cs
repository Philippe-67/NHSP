using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
using UI.Data;
using UI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add database connection
var connectionString = builder.Configuration.GetConnectionString("UserConnection");
builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseSqlServer(connectionString));

// Add identitypour configurer l'infrastructure d'authentification et d'autorisation basé sur les rôles et les users
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<UserDbContext>()
    .AddDefaultTokenProviders();//--> fournisseur de jetons



builder.Services.AddScoped<IAuthService, AuthService>();

//configure l'authentification basée sur les jetons JWT (JSON Web Tokens) en utilisant le schéma d'authentification JWTBearer.
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

//--> ajoute le client HTTP dans le conteneur d'injection de dépendances, ce qui permet à l' application de faire des requêtes HTTP.
builder.Services.AddHttpClient();

//-->ajoute la prise en charge de la session dans l' application, ce qui permet de stocker des données de session utilisateur entre les requêtes HTTP.
builder.Services.AddSession();

//ajoute un accesseur pour accéder à l'objet HttpContext dans votre application, ce qui permet d'accéder à des informations
//spécifiques à la requête HTTP en cours.
builder.Services.AddHttpContextAccessor();

///ajoute le support pour les contrôleurs MVC avec les vues dans votre application, ce qui permet de créer des routes 
///et de gérer les requêtes HTTP entrantes en fonction des actions des contrôleurs.
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

//configuration des logs
var configuration = builder.Configuration;
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .WriteTo.Console()
    .CreateLogger();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();//necessaire
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
// pattern: "{controller=Home}/{action=Index}/{id?}");
pattern: "{controller=Authentication}/{action=login}/{id?}");//ouvre l'application sur la page de login

app.Run();
