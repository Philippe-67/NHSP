using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UI.Models;
using UI.Services;
namespace UI.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly IAuthService _authService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly ILogger<AuthenticationController> _logger;

        public AuthenticationController(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            //  IConfiguration configuration,
            IAuthService authService,
            IHttpContextAccessor contextAccessor,
            ILogger<AuthenticationController> logger)
        {

            _userManager = userManager;
            _roleManager = roleManager;
            //   _configuration = configuration; 
            _authService = authService;
            _contextAccessor = contextAccessor;
            _logger = logger;
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        // public async Task<IActionResult> RegisterAsync([FromBody] Register model)
        public async Task<IActionResult> RegisterAsync(Register model)
        {

            _logger.LogInformation($"Tentative d'inscription ");
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
            _logger.LogInformation($"Tentative de connexion pour l'utilisateur de login");
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _authService.LoginAsync(model);

            var jwtToken = result.Token;


            if (result.StatusCode == 1 && jwtToken != string.Empty)
            {
                // log messages de débogage
                _logger.LogInformation($"Utilisateur  authentifié avec succès : {model.Email}");
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




