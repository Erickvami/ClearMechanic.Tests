using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClearMechanic.Api.Controllers;
using ClearMechanic.Data.Models;
using ClearMechanic.Data.Services;

namespace ClearMechanic.Tests
{
    public class GenresControllerTests
    {
        private readonly Mock<IGenreService> _genreServiceMock;
        private readonly GenresController _controller;

        public GenresControllerTests()
        {
            _genreServiceMock = new Mock<IGenreService>();
            _controller = new GenresController(_genreServiceMock.Object);
        }

         [Fact]
        public async Task GetReturnsOkResultWithAListOfGenres()
        {
            var genres = new List<Genre> { new Genre { Id = 1, Name = "Action" }, new Genre { Id = 2, Name = "Drama" } };
            _genreServiceMock.Setup(service => service.GetAll()).ReturnsAsync(genres);

            var result = await _controller.Get();

            var actionResult = Assert.IsType<ActionResult<IEnumerable<Genre>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            Console.WriteLine(okResult.Value);
            Assert.NotNull(okResult.Value);

            var returnValue = Assert.IsAssignableFrom<IEnumerable<Genre>>(okResult.Value);
            Assert.Equal(2, returnValue.Count());
        }
    }
}
