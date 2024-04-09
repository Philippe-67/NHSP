using Microsoft.EntityFrameworkCore;
using ApiRdv.Data;
using ApiRdv.Models;
using Microsoft.AspNetCore.Mvc;
using Xunit;


namespace VotreNamespaceDeTests
{
    
    public class RdvControllerTests
    {
        [Fact]
        public async Task GetRdvs_ReturnsRdvs()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<RdvDbContext>()
                .UseInMemoryDatabase(databaseName: "InMemoryRdvs")
                .Options;

            // Utilisez une base de données en mémoire pour les tests
            using (var context = new RdvDbContext(options))
            {
                // Ajoutez des rendez-vous de test à la base de données en mémoire
                context.Rdvs.Add(new Rdv { Id = 1, NomPatient = "Patient1", NomPraticien = "Praticien1", Date = DateTime.Now });
                context.Rdvs.Add(new Rdv { Id = 2, NomPatient = "Patient2", NomPraticien = "Praticien2", Date = DateTime.Now });
                context.SaveChanges();
            }

            // Act
            using (var context = new RdvDbContext(options))
            {
                var controller = new RdvController(context);
                var result = await controller.GetRdvs();

                // Assert
                var rdvs = result.Value;
                Assert.NotNull(rdvs);  // Vérifie que la liste de rendez-vous n'est pas nulle
                Assert.Equal(2, rdvs.Count());  // Vérifie que la liste contient le bon nombre de rendez-vous
            }
        }
        [Fact]
        public async Task GetRdv_ReturnsRdv()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<RdvDbContext>()
                .UseInMemoryDatabase(databaseName: "InMemoryRdvs")
                .Options;

            // Utilisez une base de données en mémoire pour les tests
            using (var context = new RdvDbContext(options))
            {
                // Ajoutez un rendez-vous de test à la base de données en mémoire avec un ID spécifique
                context.Rdvs.Add(new Rdv { Id = 5, NomPatient = "Patient5", NomPraticien = "Praticien5", Date = DateTime.Now });
                context.SaveChanges();
            }

            // Act
            using (var context = new RdvDbContext(options))
            {
                var controller = new RdvController(context);
                var result = await controller.GetRdv(5);  // Appel de la méthode GetRdv avec l'ID 5

                // Assert
                Assert.IsType<Rdv>(result.Value);   // Vérifie que la valeur renvoyée est de type Rdv
                var rdv = result.Value;
                Assert.Equal(5, rdv.Id);  // Vérifie que l'ID du rendez-vous correspond à celui demandé
            }
        }
        [Fact]
        public async Task CreateRdv_ReturnsCreatedResult()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<RdvDbContext>()
                .UseInMemoryDatabase(databaseName: "InMemoryRdvs")
                .Options;

            // Act
            using (var context = new RdvDbContext(options))
            {
                var controller = new RdvController(context);
                var newRdv = new Rdv { Id = 6, NomPatient = "NouveauPatient", NomPraticien = "NouveauPraticien", Date = DateTime.Now };

                var result = await controller.Create(newRdv);

                // Assert
                
                // Vérifie que le résultat est de type CreatedAtActionResult
                Assert.IsType<CreatedAtActionResult>(result.Result);
                var createdResult = (CreatedAtActionResult)result.Result;
                Assert.Equal(201, createdResult.StatusCode);  // Vérifie le code de statut HTTP 201 (Created)
                Assert.Equal("GetRdv", createdResult.ActionName);  // Vérifie que l'action appelée est "GetRdv"
                Assert.Equal(6, createdResult.RouteValues["id"]);  // Vérifie que l'ID du nouveau rendez-vous est inclus dans les valeurs de route
            }
        }

        [Fact]
        public async Task DeleteRdv_ReturnsNoContentResult()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<RdvDbContext>()
                .UseInMemoryDatabase(databaseName: "InMemoryRdvs")
                .Options;

            using (var context = new RdvDbContext(options))
            {
                context.Rdvs.Add(new Rdv { Id = 3, NomPatient = "Patient1", NomPraticien = "Praticien1", Date = DateTime.Now });
                context.Rdvs.Add(new Rdv { Id = 4, NomPatient = "Patient2", NomPraticien = "Praticien2", Date = DateTime.Now });
                context.SaveChanges();
            }

            // Act
            using (var context = new RdvDbContext(options))
            {
                var controller = new RdvController(context);
                var result = await controller.Delete(4);

                // Assert
                Assert.IsType<NoContentResult>(result);
                
            }
        }
    }
}