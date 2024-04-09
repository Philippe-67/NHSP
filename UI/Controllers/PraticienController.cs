using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using UI.Models;
using Microsoft.EntityFrameworkCore;

namespace UI.Controllers
{
   // [Authorize(Roles = "praticien,admin")]
    public class PraticienController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IHttpContextAccessor _contextAccessor;

        public PraticienController(IHttpClientFactory httpClientFactory, SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager, IHttpContextAccessor contextAccessor)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri("https://LocalHost:6001");
            _signInManager = signInManager;
            _userManager = userManager;
            _contextAccessor = contextAccessor;

        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                // Récupération du  jeton JWT de la session HTTP stocker dans la méthode Login de AuthenticationController.cs
                var token = _contextAccessor.HttpContext.Session.GetString("token");
                // Ajouter le jeton JWT dans l'en-tête d'autorisation de votre HttpClient
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
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
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de la requête : {ex.Message}");
            }
        }
            [HttpGet]
            public async Task<IActionResult> IndexAdmin()
            {
                try
                {
                    // Récupération du  jeton JWT de la session HTTP stocker dans la méthode Login de AuthenticationController.cs
                    var token = _contextAccessor.HttpContext.Session.GetString("token");
                    // Ajouter le jeton JWT dans l'en-tête d'autorisation de votre HttpClient
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
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
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Erreur lors de la requête : {ex.Message}");
                }
            }

        [HttpGet]
        public IActionResult Create(int praticienId, string nomPraticien)
        {
            ViewData["PraticienId"] = praticienId;
            ViewData["NomPraticien"] = nomPraticien;
            // Affiche le formulaire de création de praticien
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Praticien praticien)
        {
            //try
            //{

                var response = await _httpClient.PostAsJsonAsync("api/Praticien", praticien);
                //_context.Praticiens.Add(praticien);
                //await _context.SaveChangesAsync();

                // Ajoutez le message de confirmation à TempData
                TempData["ConfirmationMessage"] = $"Le praticien {praticien.NomPraticien} a été créé avec succès.";

                // Redirige vers l'action "Index" ou une autre action pertinente
                return RedirectToAction("Index");
            }
            //catch (Exception ex)
            //{
            //    return View("Error", ex.Message);
            //}
        }
    }


