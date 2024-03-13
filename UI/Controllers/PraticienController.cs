using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using UI.Models;

namespace UI.Controllers
{
    public class PraticienController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;


        public PraticienController(IHttpClientFactory httpClientFactory, SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri("https://LocalHost:6001");
            _signInManager = signInManager;
            _userManager = userManager;

        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            //try
           // {
                HttpResponseMessage response = await _httpClient.GetAsync("/api/Praticien");

                if (response.IsSuccessStatusCode)
                {
                    // Lecture et traitement des données
                    string responseData = await response.Content.ReadAsStringAsync();
                    // Désérialise la chaîne JSON en une liste d'objets
                    var praticiens = JsonConvert.DeserializeObject<List<Praticien>>(responseData);
                    // Utilise les données comme nécessaire, peut-être passer à la vue
                    ViewBag.Praticiens = praticiens;
                    return View();
                }
                else
                {
                    return StatusCode((int)response.StatusCode, $"Erreur HTTP: {response.StatusCode}");
                }
            //}
            //catch (Exception ex)
            //{
            //    return StatusCode(500, $"Erreur lors de la requête : {ex.Message}");
            //}
        }
    }
}
