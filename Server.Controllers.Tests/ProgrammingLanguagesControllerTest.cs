using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SETraining.Server.Controllers;
using SETraining.Server.Repositories;
using SETraining.Shared.DTOs;
using Xunit;

namespace Server.Controllers.Tests;

public class ProgrammingLanguagesControllerTest
{
    [Fact]
    public async Task Get_returns_ProgrammingLanguages_from_repo()
    {
        var logger = new Mock<ILogger<ProgrammingLanguagesController>>();
        var expected = Array.Empty<ProgrammingLanguageDTO>();
        var repository = new Mock<IProgrammingLanguagesRepository>();
        repository.Setup(m => m.ReadAsync()).ReturnsAsync(expected);

        var controller = new ProgrammingLanguagesController(logger.Object, repository.Object);

        var actual = await controller.Get();

        Assert.Equal(expected, actual.Value);
    }

    [Fact]
    public async Task Get_given_non_existing_name_returns_NotFound()
    {
        var logger = new Mock<ILogger<ProgrammingLanguagesController>>();
        var repository = new Mock<IProgrammingLanguagesRepository>();
        repository.Setup(m => m.ReadAsync("NotALanguage")).ReturnsAsync(default(ProgrammingLanguageDTO));
        var controller = new ProgrammingLanguagesController(logger.Object, repository.Object);

        var response = await controller.Get("NotALanguage");
        Assert.IsType<NotFoundResult>(response.Result);
    }

    [Fact]
    public async Task Get_given_existing_name_returns_ProgrammingLanguage()
    {
        var logger = new Mock<ILogger<ProgrammingLanguagesController>>();
        var expected = new ProgrammingLanguageDTO("Java");
        var repository = new Mock<IProgrammingLanguagesRepository>();
        repository.Setup(m => m.ReadAsync("Java")).ReturnsAsync(expected);
        var controller = new ProgrammingLanguagesController(logger.Object, repository.Object);

        var actual = await controller.Get("Java");
        
        Assert.Equal(expected, actual.Value);
    }

    [Fact]
    public async Task Create_creates_ProgrammingLanguage()
    {
        var logger = new Mock<ILogger<ProgrammingLanguagesController>>();
        var toCreate = new ProgrammingLanguageDTO("NewLanguage");
        var created = new ProgrammingLanguageDTO("NewLanguage");
        var repository = new Mock<IProgrammingLanguagesRepository>();
        repository.Setup(m => m.CreateAsync(toCreate)).ReturnsAsync(created);
        var controller = new ProgrammingLanguagesController(logger.Object, repository.Object);
        
        var result = await controller.Post(toCreate) as CreatedAtActionResult;

        Assert.Equal(created, result?.Value);
        Assert.Equal("Get", result?.ActionName);
        Assert.Equal(KeyValuePair.Create("Name", (object?)"NewLanguage"), result?.RouteValues?.Single());
    }
}
