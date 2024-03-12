using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using UI.Models;



public class RdvController : Controller
{
    private readonly HttpClient _httpClient;


    public RdvController(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.BaseAddress = new Uri("https://localhost:5001"); // Assurez-vous de mettre le bon port pour votre API Rdv
    }


    public async Task<IActionResult> Index(string nomPraticien, int praticienId, DateTime jourDisponible, int? annee, int? mois)
    //  public async Task<IActionResult> Index(int? praticienId, DateTime jourDisponible, int? annee, int? mois)
    {
        /// 1 Création d'un dictionnaire pour transmettre des données à la cue
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

            //Ces ViewBag permettrons d'afficher dans la vue l'année et le mois selectionnés ainsi que le nombre de jours dans ce mois spécifique
            ViewBag.Annees = new SelectList(anneesDisponibles, anneeSelectionnee);
        ViewBag.Mois = new SelectList(moisDisponibles, moisSelectionne);
        ViewBag.NombreDeJoursDansMois = nombreDeJoursDansMois;



        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync("/api/Rdv");

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

                // Filtrer les jours disponibles en fonction des rendez-vous
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
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erreur lors de la requête : {ex.Message}");
        }
    }
    private bool EstBissextile(int annee)
    {
        return (annee % 4 == 0 && annee % 100 != 0) || (annee % 400 == 0);
    }

    //public IActionResult Create(int praticienId, string nomPraticien)

    //{
    //    ViewData["PraticienId"] = praticienId;
    //    ViewData["NomPraticien"] = nomPraticien;
    //    // Affiche le formulaire de création de rendez-vous
    //    return View();
    //}

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
            await CreateRendezVousAsync(rdv);
            // Crée un objet de type UI.Models.Rdv avec les données nécessaires
            UI.Models.Rdv model = new UI.Models.Rdv
            {
                NomPraticien = rdv.NomPraticien,
                Id = rdv.Id,
                Date = rdv.Date
            };
            // Ajoutez le message de confirmation à TempData
            TempData["ConfirmationMessage"] = $"Votre rendez-vous du {model.Date} avec le docteur  {model.NomPraticien}, a été enregistré avec succès.";

            // Redirige vers l'action "Index" avec le modèle correct
            return RedirectToAction("Index", model);
          
        }
        catch (Exception ex)
        {
            return View("Error", ex.Message);

        }
    }
    // Ajoutez d'autres actions selon les besoins de votre application

    private async Task CreateRendezVousAsync(Rdv rdv)
    {
        var response = await _httpClient.PostAsJsonAsync("api/Rdv", rdv); // Assurez-vous que l'URL est correcte

        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await response.Content.ReadAsStringAsync();
            throw new Exception(errorMessage);
        }
    }

}



