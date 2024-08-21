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
    public class MoviesControllerTests
    {
        private readonly Mock<IMovieService> _movieServiceMock;
        private readonly MoviesController _controller;

        public MoviesControllerTests()
        {
            _movieServiceMock = new Mock<IMovieService>();
            _controller = new MoviesController(_movieServiceMock.Object);
        }

        [Fact]
        public async Task Get_ReturnsOkResult_WithAListOfMovies()
        {
            var movies = new List<Movie> { new Movie { Id = 1, Title = "Movie 1" }, new Movie { Id = 2, Title = "Movie 2" } };
            _movieServiceMock.Setup(service => service.GetAll()).ReturnsAsync(movies);

            var result = await _controller.Get();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<Movie>>(okResult.Value);
            Assert.Equal(2, returnValue.Count());
        }

        [Theory]
        [InlineData(null, "")]
        [InlineData("Action,Drama", "Avengers")]
        public async Task Search_ReturnsOkResult_WithFilteredMovies(string genres, string query)
        {
            var movies = new List<Movie> { new Movie { Id = 1, Title = "Avengers" } };
            _movieServiceMock.Setup(service => service.SearchMovies(It.IsAny<string>(), It.IsAny<string[]>())).ReturnsAsync(movies);

            var result = await _controller.Search(genres, query);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<Movie>>(okResult.Value);
            Assert.Single(returnValue);
        }

        [Fact]
        public async Task GetById_ReturnsOkResult_WithMovie()
        {
            var movie = new Movie { Id = 1, Title = "Movie 1" };
            _movieServiceMock.Setup(service => service.GetById(1, It.IsAny<bool?>(), It.IsAny<bool?>())).ReturnsAsync(movie);

            var result = await _controller.GetById(1, null, null);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<Movie>(okResult.Value);
            Assert.Equal(1, returnValue.Id);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenMovieNotFound()
        {
            _movieServiceMock.Setup(service => service.GetById(1, It.IsAny<bool?>(), It.IsAny<bool?>())).ReturnsAsync((Movie)null);

            var result = await _controller.GetById(1, null, null);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Post_ReturnsCreatedAtAction_WithNewMovie()
        {
            var movie = new Movie { Id = 1, Title = "New Movie" };
            _movieServiceMock.Setup(service => service.CreateMovieAsync(It.IsAny<Movie>())).ReturnsAsync(movie);

            var result = await _controller.Post(movie);

            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnValue = Assert.IsType<Movie>(createdAtActionResult.Value);
            Assert.Equal(1, returnValue.Id);
        }

        [Fact]
        public async Task Delete_ReturnsOk_WhenMovieIsDeleted()
        {
            var result = await _controller.Delete(1);

            Assert.IsType<OkResult>(result);
            _movieServiceMock.Verify(service => service.DeleteAsync(1, true), Times.Once);
        }

        [Fact]
        public async Task Post_ThrowsException_WhenActorIdsAreInvalid()
        {
            var movie = new Movie
            {
                Id = 1,
                Title = "Invalid Movie",
                Actors = new List<Actor>
                {
                    new Actor { Id = int.MaxValue },
                    new Actor { Id = int.MaxValue - 1 }
                }
            };

            _movieServiceMock.Setup(service => service.CreateMovieAsync(It.IsAny<Movie>()))
                .ThrowsAsync(new Exception("Invalid actor IDs"));

            var exception = await Assert.ThrowsAsync<Exception>(() => _controller.Post(movie));
            Assert.Equal("Invalid actor IDs", exception.Message);
        }

        [Fact]
        public async Task Post_ThrowsException_WhenMovieDataIsInvalid()
        {
            var movie = new Movie
            {
                Id = 1,
                Title = ""
            };

            _movieServiceMock.Setup(service => service.CreateMovieAsync(It.IsAny<Movie>()))
                .ThrowsAsync(new Exception("Invalid movie data"));

            var exception = await Assert.ThrowsAsync<Exception>(() => _controller.Post(movie));
            Assert.Equal("Invalid movie data", exception.Message);
        }
    }
}