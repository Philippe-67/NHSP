using Microsoft.AspNetCore.Mvc;
using UI.Models;
using UI.Services;
using System.Net.Http.Headers;
namespace UI.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IHttpContextAccessor _contextAccessor;

        public AuthenticationController(IAuthService authService,IHttpContextAccessor contextAccessor)
        { 
            _authService = authService;
            _contextAccessor = contextAccessor;
        } 

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
       // public async Task<IActionResult> RegisterAsync([FromBody] Register model)
          public async Task<IActionResult> RegisterAsync( Register model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _authService.RegisterAsync(model);
            TempData["msg"] = result.StatusMessage;

            return RedirectToAction(nameof(Register));
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(Login model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _authService.LoginAsync(model);

           var jwtToken = result.Token;

            if (result.StatusCode == 1 && jwtToken != string.Empty)
            {
                // Ajoutez des messages de débogage
                Console.WriteLine("Connexion réussie. Redirection vers la page d'accueil.");
                // Stockage (SetString) du jeton JWT dans la session HTTP à l'aide de IHttpContextAccessor mis en place dans le contructeur
                _contextAccessor.HttpContext.Session.SetString("token", jwtToken);

                return RedirectToAction("Index", "Home", new { token = jwtToken });
            }
            else
            {
                TempData["msg"] = result.StatusMessage;
                // Ajoutez un message de débogage en cas d'échec de connexion
                Console.WriteLine($"Échec de la connexion : {result.StatusMessage}");

                return RedirectToAction(nameof(Login));
            }
        }

        // [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _authService.LogoutAsync();

            return RedirectToAction(nameof(Login));
        }
    }
}




