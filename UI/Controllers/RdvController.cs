using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.Data;
using System.Net.Http;
using System.Net.Http.Headers;
using UI.Models;


//[Authorize(Roles = "admin")]
public class RdvController : Controller
{
    private readonly HttpClient _httpClient;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly ILogger<RdvController> _logger;


    public RdvController(
        IHttpClientFactory httpClientFactory,
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        IHttpContextAccessor contextAccessor,
        ILogger<RdvController> logger)
    {
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.BaseAddress = new Uri("https://localhost:6001"); // Assurez-vous de mettre le bon port pour votre API Rdv
        _userManager = userManager;
        _signInManager = signInManager;
        _contextAccessor = contextAccessor;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string nomPraticien, int praticienId, DateTime jourDisponible, int? annee, int? mois)
    //  public async Task<IActionResult> Index(int? praticienId, DateTime jourDisponible, int? annee, int? mois)
    {

        /// 1 Création d'un dictionnaire pour transmettre des données à la vue
        ViewData["NomPraticien"] = nomPraticien;
        ViewData["PraticienId"] = praticienId;
        ViewData["Date"] = jourDisponible;
        //  ViewData["AnneeSelectionnee"] = annee;

        ///////////////////////////////////////////////////////////////////////////////////////
        ///         2 implémentation pour l'affichage d'un pseudo calendrier                ///
        ///////////////////////////////////////////////////////////////////////////////////////

        // 2.1 Récupération de  l'année et le mois actuels par défaut
        int anneeActuelle = DateTime.Now.Year;
        int moisActuel = DateTime.Now.Month;

        // 2.2 si l'utlisateur ne sélectionne pas d'année ou de mois spécifiques, utilisation des valeurs actuelles par défaut
        int anneeSelectionnee = annee ?? anneeActuelle;
        int moisSelectionne = mois ?? moisActuel;

        //  2.3 Instanciation d'une  listes d'années et d'une liste de mois 
        List<int> anneesDisponibles = Enumerable.Range(anneeActuelle, 10).ToList(); // 10 années à partir de l'année actuelle
        List<int> moisDisponibles = Enumerable.Range(1, 12).ToList(); // Les 12 mois

        //  2.4 Calcul du nombre de jours dans le mois sélectionné
        /// 2.4.1  prise en compte des années bissextiles
        int nombreDeJoursDansMois = moisSelectionne == 2 && EstBissextile(anneeSelectionnee) ? 29 : DateTime.DaysInMonth(anneeSelectionnee, moisSelectionne);
        /// 2.4.2 implémantation de la liste de date dans le mois sélectioné
        List<DateTime> datesDuMois = new List<DateTime>();
        //et
        /// >>>>> 2.4.3 Création de la liste de dates  disponibles ("joursDiponibles") pour le mois sélectionné, en excluant les dimanches
        List<DateTime> joursDisponibles = new List<DateTime>();

        for (int jour = 1; jour <= nombreDeJoursDansMois; jour++)
        {
            DateTime date = new DateTime(anneeSelectionnee, moisSelectionne, jour);
            if (date.DayOfWeek != DayOfWeek.Sunday) // Exclure les dimanches
            {
                joursDisponibles.Add(date);
            }
            datesDuMois.Add(date);
        }
        //Ces ViewBags permettrons d'afficher dans la vue l'année et le mois selectionné ainsi que le nombre de jours dans ce mois spécifique
        ViewBag.Annees = new SelectList(anneesDisponibles, anneeSelectionnee);
        ViewBag.Mois = new SelectList(moisDisponibles, moisSelectionne);
        ViewBag.NombreDeJoursDansMois = nombreDeJoursDansMois;



        //try
        {
            // Récupération du  jeton JWT de la session HTTP stocker dans la méthode Login de AuthenticationController.cs
            var token = _contextAccessor.HttpContext.Session.GetString("token");

            // Ajouter le jeton JWT dans l'en-tête d'autorisation de votre HttpClient
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            HttpResponseMessage response = await _httpClient.GetAsync("/api/Rdv");
            //var response = await _httpClient.GetAsync("/api/Rdv");

            if (response.IsSuccessStatusCode)
            {
                // Lecture et traitement des données
                string responseData = await response.Content.ReadAsStringAsync();
                // Désérialise la chaîne JSON en une liste d'objets
                var listeRdvs = JsonConvert.DeserializeObject<List<Rdv>>(responseData);

                // Filtrer les rendez-vous en fonction de nomPraticien
                if (!string.IsNullOrEmpty(nomPraticien))
                {
                    listeRdvs = listeRdvs.Where(rdv => rdv.NomPraticien == nomPraticien).ToList();
                }
                // Filtrer les rendez-vous en fonction de praticienId s'il est spécifié

                // Trier les rendez-vous par ordre chronologique
                listeRdvs = listeRdvs.OrderBy(rdv => rdv.Date).ToList();

                // filtre les dates disponibles en retirant celles qui correspondent aux dates des rendez-vous.
                joursDisponibles = joursDisponibles.Except(listeRdvs.Select(rdv => rdv.Date.Date)).ToList();

                // Utilise les données comme nécessaire, peut-être passer à la vue
                ViewBag.Rdvs = listeRdvs;
                ViewBag.JoursDisponibles = joursDisponibles;

                // Récupérez le message de confirmation depuis TempData
                ViewBag.ConfirmationMessage = TempData["ConfirmationMessage"] as string;

                return View();
            }
            else
            {
                return StatusCode((int)response.StatusCode, $"Erreur HTTP: {response.StatusCode}");
            }

            //catch (Exception ex)
            //{
            //    return StatusCode(500, $"Erreur lors de la requête : {ex.Message}");
            //}
        }
    }
        

        private bool EstBissextile(int annee)
    {
        return (annee % 4 == 0 && annee % 100 != 0) || (annee % 400 == 0);
    }


    [HttpGet]
    public IActionResult Create(DateTime jourDisponible, int praticienId, string nomPraticien)

    {
        ViewData["PraticienId"] = praticienId;
        ViewData["NomPraticien"] = nomPraticien;
        ViewData["Date"] = jourDisponible;
        // Affiche le formulaire de création de rendez-vous
        return View();
    }

    //  [Authorize (Roles ="praticien")]
    [HttpPost]

    public async Task<IActionResult> Create(Rdv rdv)
    {

        try
        {
            // Formate la date au format jj/mm/yy
            string formattedDate = rdv.Date.ToString("dd/MM/yyyy");

            await CreateRendezVousAsync(rdv);
            // Crée un objet de type UI.Models.Rdv avec les données nécessaires
            UI.Models.Rdv model = new UI.Models.Rdv
            {
                NomPraticien = rdv.NomPraticien,
                //   Id = rdv.Id,
                Date = rdv.Date,
                NomPatient = rdv.NomPatient,
            };
            // Ajoutez le message de confirmation à TempData
            TempData["ConfirmationMessage"] = $"Votre rendez-vous du {formattedDate} avec le docteur  {model.NomPraticien}, a été enregistré avec succès.";

            // Redirige vers l'action "Index" avec le modèle correct
            return RedirectToAction("Index", model);

        }
        catch (Exception ex)
        {
            return View("Error", ex.Message);

        }
    }
   
    public async Task<IActionResult> ListeDesRdv(int praticienId, string nomPraticien, string nomPatient, DateTime date)
    {
        ViewData["PraticienId"] = praticienId;
        ViewData["NomPraticien"] = nomPraticien;
        ViewData["NomPatient"] = nomPatient;
        ViewData["DateTime"] = date;

        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync("/api/Rdv");

            if (response.IsSuccessStatusCode)
            {
                string responseData = await response.Content.ReadAsStringAsync();
                var listeRdvs = JsonConvert.DeserializeObject<List<Rdv>>(responseData);

                // Filtrer les rendez-vous en fonction de nomPraticien
                if (!string.IsNullOrEmpty(nomPraticien))
                {
                    listeRdvs = listeRdvs.Where(rdv => rdv.NomPraticien == nomPraticien).ToList();
                }
                // Autres filtres en fonction des autres paramètres

                // Trier les rendez-vous par ordre chronologique
                listeRdvs = listeRdvs.OrderBy(rdv => rdv.Date).ToList();
                listeRdvs=listeRdvs.Where(rdv=>rdv.Date.Date >= DateTime.Today.Date).ToList();
                // Passer les données à la vue
                return View(listeRdvs);
            }
            else
            {
                // Gérer le cas où la réponse n'est pas un succès
                // Peut-être renvoyer une vue d'erreur ou afficher un message à l'utilisateur
                return View("Error");
            }
        }
        catch (Exception ex)
        {
            // Gérer les exceptions
            // Peut-être renvoyer une vue d'erreur ou afficher un message à l'utilisateur
            return View("Error");
        }
    }

    public async Task CreateRendezVousAsync(Rdv rdv)
    {
        var response = await _httpClient.PostAsJsonAsync("api/Rdv", rdv); // Assurez-vous que l'URL est correcte
        _logger.LogInformation($"création d'un rdv pour {rdv.NomPatient}");
        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Rendez-vous non créé");

            throw new Exception(errorMessage);

        }
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            // Récupération du jeton JWT de la session HTTP stocké dans la méthode Login de AuthenticationController.cs
            var token = _contextAccessor.HttpContext.Session.GetString("token");

            // Ajouter le jeton JWT dans l'en-tête d'autorisation de votre HttpClient
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Envoi de la requête DELETE à l'API pour supprimer le rendez-vous avec l'ID spécifié
            HttpResponseMessage response = await _httpClient.DeleteAsync($"/api/Rdv/{id}");

            if (response.IsSuccessStatusCode)
            {
                

                // Ajoutez le message de confirmation à TempData
                TempData["ConfirmationMessage"] = $"Le rendez-vous a bien été annulé.";


                // Redirection vers l'action "Index" après la suppression
                return RedirectToAction("Index");
                //return View();
            }
            else
            {
                // Si la suppression échoue, retourner une erreur avec le code de statut HTTP
                return StatusCode((int)response.StatusCode, $"Erreur HTTP lors de la suppression : {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            // En cas d'erreur, retourner une réponse avec le code de statut HTTP 500 (Erreur interne du serveur) et le message d'erreur
            return StatusCode(500, $"Erreur lors de la suppression du rendez-vous : {ex.Message}");
        }
    }


}

