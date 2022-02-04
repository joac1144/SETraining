using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SETraining.Server.Contexts;
using SETraining.Server.Repositories;
using SETraining.Shared.DTOs;
using SETraining.Shared.Models;
using Xunit;

namespace Server.Repositories.Tests;

public class ProgrammingLanguagesRepositoryTests : IDisposable
{
    private readonly SETrainingContext _context;
    private readonly ProgrammingLanguagesRepository _repository;
    
    public ProgrammingLanguagesRepositoryTests()
    {
        var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();
        var builder = new DbContextOptionsBuilder<SETrainingContext>();
        builder.UseSqlite(connection);
        var context = new SETrainingContext(builder.Options);
        context.Database.EnsureCreated();
        
        context.AddRange(
            new ProgrammingLanguage("C#"),
            new ProgrammingLanguage("Java"),
            new ProgrammingLanguage("F#"),
            new ProgrammingLanguage("JavaScript"),
            new ProgrammingLanguage("Go")
        );

        context.SaveChanges();

        _context = context;
        _repository = new ProgrammingLanguagesRepository(_context);
    }

    [Fact]
    public async Task Create_new_ProgrammingLanguage_returns_Created_ProgrammingLanguage()
    {
        var toCreate = new ProgrammingLanguageDTO("SourcePawn");

        var created = await _repository.CreateAsync(toCreate);
        
        Assert.Equal("SourcePawn", created.Name);
    }
    
    [Fact]
    public async Task Create_new_ProgrammingLanguage_With_special_Letters()
    {
        var toCreate = new ProgrammingLanguageDTO("Java2");

        var created = await _repository.CreateAsync(toCreate);
        
        Assert.Equal("Java2", created.Name);
    }
    
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task Create_new_ProgrammingLanguage_Where_Name_isEmpty_or_null_returns_null(string name)
    {
        var toCreate = new ProgrammingLanguageDTO(name);

        var created = await _repository.CreateAsync(toCreate);
        
        Assert.Null(created);
    }

    [Fact]
    public async Task Read_returns_all_ProgrammingLanguages()
    {
        var actual = await _repository.ReadAsync();
        var expected = new[] { 
            new ProgrammingLanguageDTO("C#"),
            new ProgrammingLanguageDTO("F#"),
            new ProgrammingLanguageDTO("Go"),
            new ProgrammingLanguageDTO("Java"),
            new ProgrammingLanguageDTO("JavaScript")
        };

        var enumerator = expected.GetEnumerator();

        foreach (var item in actual.Value)
        {
            enumerator.MoveNext();
            Assert.Equal(enumerator.Current, item);
        }
    }
    
    [Theory]
    [InlineData(" ")]
    [InlineData("")]
    [InlineData(null)]
    public async Task Read_given_nullOrEmpty_returns_OptionIsNone(string searchName)
    {
        var actual = await _repository.ReadAsync(searchName);
        Assert.True(actual.IsNone);
    }

    [Fact]
    public async Task Read_given_non_existing_name_returns_OptionIsNone()
    {
        var actual = await _repository.ReadAsync("NotALanguage");
        Assert.True(actual.IsNone);
    }
    
    [Theory]
    [InlineData("F#")]
    [InlineData("Go")]
    [InlineData("Java")]
    [InlineData("JavaScript")]
    [InlineData("C#")]
    public async Task Read_given_existing_name_returns_ProgrammingLanguage(string searchName)
    {
        var expected = new ProgrammingLanguageDTO(searchName);
        
        var actual = await _repository.ReadAsync(searchName);

        Assert.Equal(expected, actual.Value);
    }
    
    [Theory]
    [InlineData("c#")]
    [InlineData("JAVASCRIPT")]
    [InlineData("jAvA")]
    public async Task Read_Upper_and_LowerCase_returns_ProgrammingLanguage_with_same_Lowercase_letters(string searchName)
    {
        var expected = new ProgrammingLanguageDTO(searchName);
        
        var actual = await _repository.ReadAsync(searchName);

        Assert.Equal(expected.Name.ToLower(), actual.Value.Name.ToLower());
    }
    
    [Theory]
    [InlineData("   JavaScript")]
    [InlineData("   Go    ")]
    [InlineData("Java    ")]
    public async Task Read_String_with_Withspaces_in_End_and_Beginning_Returns_ProgrammingLanguage(string searchString)
    {
        var expected = new ProgrammingLanguage(searchString);

        var actual = await _repository.ReadAsync(searchString);
        
        Assert.Equal(expected.Name.Trim(), actual.Value.Name);
    }

    public void Dispose() => _context.Dispose();
}
