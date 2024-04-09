using ApiPraticien.Controllers;
using ApiPraticien.Data;
using ApiPraticien.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace TestsPraticien
{
    public class PraticienControllerTests
    {
       

        [Fact]
        public async Task GetPraticiens_ReturnsPraticiens()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<PraticienDbContext>()
                .UseInMemoryDatabase(databaseName: "InMemoryPraticiens")
                .Options;

            using (var context = new PraticienDbContext(options))
            {
                context.Praticiens.Add(new Praticien { Id = 1, NomPraticien = "Dr. Smith" ,Specialite="généraliste" });
                context.Praticiens.Add(new Praticien { Id = 2, NomPraticien = "Dr. Johnson", Specialite ="généraliste" });
                context.SaveChanges();
            }

            using (var context = new PraticienDbContext(options))
            {
                var controller = new PraticienController(context, new LoggerFactory().CreateLogger<PraticienController>());

                // Act
                var result = await controller.GetPraticiens();

                // Assert
                var actionResult = Assert.IsType<ActionResult<IEnumerable<Praticien>>>(result);
                var praticiens = Assert.IsAssignableFrom<IEnumerable<Praticien>>(actionResult.Value);
                Assert.Equal(2, praticiens.Count());
            
                context.Database.EnsureDeleted();
            }
        }
        [Fact]
        public async Task Create_WhenPraticienIsValid_ReturnsCreatedResult()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<PraticienDbContext>()
                .UseInMemoryDatabase(databaseName: "InMemoryPraticiens")
                .Options;

            using (var context = new PraticienDbContext(options))
            {
                var controller = new PraticienController(context, new LoggerFactory().CreateLogger<PraticienController>());
                
                var newPraticien = new Praticien { Id = 8, NomPraticien = "Dr. Smiths",Specialite = "généraliste" };

                // Act
                var result = await controller.Create(newPraticien);

                // Assert
                Assert.IsType<CreatedAtActionResult>(result.Result);
                var createdResult = (CreatedAtActionResult)result.Result;
                Assert.Equal(201, createdResult.StatusCode);
                Assert.Equal("GetPraticiens", createdResult.ActionName);
                Assert.Equal(8, createdResult.RouteValues["Id"]);
           
            }
            using (var context = new PraticienDbContext(options))
            {
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async Task PutPraticien_WhenPraticienExists_ReturnsNoContent()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<PraticienDbContext>()
                .UseInMemoryDatabase(databaseName: "InMemoryPraticiens")
                .Options;

            using (var context = new PraticienDbContext(options))
            {
                context.Praticiens.Add(new Praticien { Id = 1, NomPraticien = "Dr. Smith", Specialite = "généraliste" });
                context.SaveChanges();
            }

            using (var context = new PraticienDbContext(options))
            {
                var controller = new PraticienController(context, new LoggerFactory().CreateLogger<PraticienController>());
                var existingPraticien = new Praticien { Id = 1, NomPraticien = "Dr. Smith" ,Specialite = "généraliste" };  // Modifiez les propriétés du praticien selon le scénario de test

                // Act
                var result = await controller.PutPraticien(1, existingPraticien);

                // Assert
                Assert.IsType<NoContentResult>(result);
            
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async Task DeletePraticien_WhenPraticienExists_ReturnsNoContent()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<PraticienDbContext>()
                .UseInMemoryDatabase(databaseName: "InMemoryPraticiens")
                .Options;

            using (var context = new PraticienDbContext(options))
            {
                context.Praticiens.Add(new Praticien { Id = 1, NomPraticien = "Dr. Smith",Specialite="généraliste" });
                context.SaveChanges();
            }

            using (var context = new PraticienDbContext(options))
            {
                var controller = new PraticienController(context, new LoggerFactory().CreateLogger<PraticienController>());

                // Act
                var result = await controller.DeletePraticien(1);

                // Assert
                Assert.IsType<NoContentResult>(result);
            }
            using (var context = new PraticienDbContext(options))
            {
                context.Database.EnsureDeleted();
            }
        }

       

    }
}