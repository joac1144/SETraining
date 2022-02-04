using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SETraining.Server.Controllers;
using SETraining.Server.Repositories;
using SETraining.Shared.DTOs;
using Xunit;
using SETraining.Shared;
using SETraining.Shared.Models;

namespace Server.Controllers.Tests;

public class ArticleControllerTest
{
    [Fact]
    public async Task Create_creates_Article()
    {
        var logger = new Mock<ILogger<ArticleController>>();
        var toCreate = new ArticleCreateDTO();
        var created = new ArticleDTO(1, "Dette er en title", ArticleType.Written, DateTime.Today, null, null, DifficultyLevel.Expert, null, "Article", null);
        var repository = new Mock<IArticleRepository>();
        repository.Setup(m => m.CreateAsync(toCreate)).ReturnsAsync(created);
        var controller = new ArticleController(logger.Object, repository.Object);

        var result = await controller.Post(toCreate) as CreatedAtActionResult;

        Assert.Equal(created, result?.Value);
        Assert.Equal("Get", result?.ActionName);
        Assert.Equal(KeyValuePair.Create("Id", (object?)1), result?.RouteValues?.Single());
    }

    [Fact]
    public async Task GetAll_returns_articles()
    {
        var logger = new Mock<ILogger<ArticleController>>();
        var expected = Array.Empty<ArticlePreviewDTO>();
        var repository = new Mock<IArticleRepository>();
        repository.Setup(m => m.ReadAsync()).ReturnsAsync(expected);
        var controller = new ArticleController(logger.Object, repository.Object);

        var actual = await controller.Get();

        Assert.Equal(expected, actual.Value);
    }

    [Fact]
    public async Task GetAll_no_articles_returns_NotFound()
    {
        var logger = new Mock<ILogger<ArticleController>>();
        var repository = new Mock<IArticleRepository>();
        repository.Setup(m => m.ReadAsync()).ReturnsAsync(default(Option<IEnumerable<ArticlePreviewDTO>>));
        var controller = new ArticleController(logger.Object, repository.Object);

        var actual = await controller.Get();

        Assert.IsType<NotFoundResult>(actual.Result);
    }

    [Fact]
    public async Task Get_given_non_existing_id_returns_NotFound()
    {
        var logger = new Mock<ILogger<ArticleController>>();
        var repository = new Mock<IArticleRepository>();
        repository.Setup(m => m.ReadFromIdAsync(42)).ReturnsAsync(default(ArticleDTO));
        var controller = new ArticleController(logger.Object, repository.Object);

        var actual = await controller.GetFromId(42);

        Assert.IsType<NotFoundResult>(actual.Result);
    }

    [Fact]
    public async Task Get_given_existing_id_returns_Article()
    {
        var logger = new Mock<ILogger<ArticleController>>();
        var expected = new ArticleDTO(1, "Dette er en title", ArticleType.Written, DateTime.Today, null, null, DifficultyLevel.Expert, null, "Article", null);
        var repository = new Mock<IArticleRepository>();
        repository.Setup(m => m.ReadFromIdAsync(1)).ReturnsAsync(expected);
        var controller = new ArticleController(logger.Object, repository.Object);

        var actual = await controller.GetFromId(1);

        Assert.Equal(expected, actual.Value);
    }

    [Fact]
    public async Task Get_given_non_existing_parameters_returns_NotFound()
    {
        var logger = new Mock<ILogger<ArticleController>>();
        var repository = new Mock<IArticleRepository>();
        var created = new List<ArticlePreviewDTO> { new(1, "This is a title", ArticleType.Written, DateTime.Today, null, null, DifficultyLevel.Expert, null) };
        repository.Setup(m => m.ReadFromParametersAsync("DOES_NOT_EXIST", "2", new string[] { "java" })).ReturnsAsync(created);
        var controller = new ArticleController(logger.Object, repository.Object);

        var actual = await controller.GetFromParameters("DOES_NOT_EXIST", "2", null);

        Assert.IsType<NotFoundResult>(actual.Result);
    }

    [Fact]
    public async Task Get_given_existing_parameters_returns_Articles()
    {
        var logger = new Mock<ILogger<ArticleController>>();
        var expected = new List<ArticlePreviewDTO> { new ArticlePreviewDTO(1, "This is a title", ArticleType.Written, DateTime.Today, null, new string[] { "Java" }, DifficultyLevel.Expert, null) };
        var repository = new Mock<IArticleRepository>();
        repository.Setup(m => m.ReadFromParametersAsync("title", "3", new string[] { "Java" })).ReturnsAsync(expected);
        var controller = new ArticleController(logger.Object, repository.Object);

        var actual = await controller.GetFromParameters("title", "3", new string[] { "Java" });

        Assert.Equal(expected, actual.Value);
    }

    [Fact]
    public async Task Put_given_existing_article_updates_article()
    {
        var logger = new Mock<ILogger<ArticleController>>();
        var article = new ArticleUpdateDTO();
        var repository = new Mock<IArticleRepository>();
        repository.Setup(m => m.UpdateAsync(1, article)).ReturnsAsync(Status.Updated);
        var controller = new ArticleController(logger.Object, repository.Object);

        var response = await controller.Put(1, article);

        Assert.IsType<NoContentResult>(response);
    }

    [Fact]
    public async Task Put_given_non_existing_article_returns_NotFound()
    {
        var logger = new Mock<ILogger<ArticleController>>();
        var article = new ArticleUpdateDTO();
        var repository = new Mock<IArticleRepository>();
        repository.Setup(m => m.UpdateAsync(1, article)).ReturnsAsync(Status.NotFound);
        var controller = new ArticleController(logger.Object, repository.Object);

        var response = await controller.Put(1, article);

        Assert.IsType<NotFoundResult>(response);
    }

    [Fact]
    public async Task Delete_given_existing_id_deletes_article()
    {
        var logger = new Mock<ILogger<ArticleController>>();
        var repository = new Mock<IArticleRepository>();
        var created = new ArticleDTO(1, "Dette er en title", ArticleType.Written, DateTime.Today, null, null, DifficultyLevel.Expert, null, "Article", null);
        repository.Setup(m => m.DeleteAsync(created.Id)).ReturnsAsync(Status.Deleted);
        var controller = new ArticleController(logger.Object, repository.Object);

        var response = await controller.Delete(created.Id);
        var deletedArticle = await controller.GetFromId(created.Id);

        Assert.IsType<NoContentResult>(response);
        Assert.IsType<NotFoundResult>(deletedArticle.Result);
    }

    [Fact]
    public async Task Delete_given_non_existing_id_returns_NotFound()
    {
        var logger = new Mock<ILogger<ArticleController>>();
        var repository = new Mock<IArticleRepository>();
        repository.Setup(m => m.DeleteAsync(98)).ReturnsAsync(Status.NotFound);
        var controller = new ArticleController(logger.Object, repository.Object);

        var response = await controller.Delete(98);

        Assert.IsType<NotFoundResult>(response);
    }
}