﻿using Microsoft.EntityFrameworkCore;
using SETraining.Server.Contexts;
using SETraining.Shared;
using SETraining.Shared.DTOs;
using SETraining.Shared.ExtensionMethods;
using SETraining.Shared.Models;

namespace SETraining.Server.Repositories;
public class ArticleRepository : IArticleRepository
{
    private readonly ISETrainingContext _context;

    public ArticleRepository(ISETrainingContext context)
    {
        _context = context;
    }

    public async Task<ArticleDTO> CreateArticleAsync(ArticleCreateDTO article)
    {
        var entity = new Article(article.Title, article.Type, DateTime.Today, article.Difficulty)
        {
            Description = article.Description,
            ProgrammingLanguages = await GetProgrammingLanguagesAsync(article.ProgrammingLanguages!).ToListAsync(),
            AvgRating = article.AvgRating,
            ImageURL = article.ImageURL,
            Created = DateTime.Now.ToUniversalTime(),
            Body = article.Body,
            VideoURL = article.VideoURL
        };
        _context.Articles.Add(entity);
        
        await _context.SaveChangesAsync();

        return new ArticleDTO(
                entity.Id,
                entity.Title,
                entity.Type,
                entity.Created.ToUniversalTime(),
                entity.Description,
                entity.ProgrammingLanguages.Select(p => p.Name).ToList(),
                entity.Difficulty,
                entity.AvgRating,
                entity.Body,
                entity.ImageURL,
                entity.VideoURL
        ); 
    }

    public async Task<Status> DeleteArticleAsync(int articleId)
    {
        var entity = await _context.Articles.FindAsync(articleId);

        if (entity == null)
        {
            return Status.NotFound;
        }

        _context.Articles.Remove(entity);
        await _context.SaveChangesAsync();
        return Status.Deleted;
    }

    public async Task<Option<ArticleDTO>> ReadArticleFromIdAsync(int articleId)
    {
        return await _context.Articles
                    .Where(c => c.Id == articleId)
                    .Select(c => new ArticleDTO(
                        c.Id,
                        c.Title,
                        c.Type,
                        c.Created.ToUniversalTime(),
                        c.Description,
                        c.ProgrammingLanguages!.Select(p => p.Name).ToList(),
                        c.Difficulty,
                        c.AvgRating,
                        c.Body,
                        c.ImageURL,
                        c.VideoURL
                    ))
                    .FirstOrDefaultAsync();
    }

    public async Task<Option<IEnumerable<ArticlePreviewDTO>>> ReadArticlesFromTitleAsync(string articleTitle)
    {
        var result =  await _context.Articles.Where(c => c.Title.ToLower().Contains(articleTitle.ToLower().Trim()))
            .Select(c => new ArticlePreviewDTO(
                c.Id,
                c.Title,
                c.Type,
                c.Created.ToUniversalTime(),
                c.Description,
                c.ProgrammingLanguages!.Select(p => p.Name).ToList(),
                c.Difficulty,
                c.AvgRating,
                c.ImageURL
                )).ToListAsync();
        
        return result.Any() ? result : null;
    }

    public async Task<Option<IEnumerable<ArticlePreviewDTO>>> ReadAllArticlesFromParametersAsync(string title, string difficulty, string[] languages)
    {
        if (!string.IsNullOrWhiteSpace(difficulty) && !(languages!.IsNullOrEmpty()))
        {
            return await ReadAllArticlesAsync(title, difficulty, languages);
        }
        else if (string.IsNullOrWhiteSpace(difficulty) && !(languages!.IsNullOrEmpty()))
        {
            return await ReadAllArticlesAsync(title, languages);
        }
        else if (!string.IsNullOrWhiteSpace(difficulty) && languages!.IsNullOrEmpty())
        {
            return await ReadAllArticlesAsync(title, difficulty);
        }
        else if (!string.IsNullOrWhiteSpace(title))
        {
            return await ReadAllArticlesAsync(title);
        }

        return await ReadAllArticlesAsync();
    }

    public async Task<Option<IEnumerable<ArticlePreviewDTO>>> ReadAllArticlesAsync()
    {
        var result =  await _context.Articles
                    .Select(article =>
                        new ArticlePreviewDTO(
                                article.Id,
                                article.Title,
                                article.Type,
                                article.Created.ToUniversalTime(),
                                article.Description,
                                article.ProgrammingLanguages!.Select(p => p.Name).ToList(),
                                article.Difficulty,
                                article.AvgRating,
                                article.ImageURL
                                )
                    ).ToListAsync();
        
        return result.Any() ? result : null;
    }

    public async Task<Option<IEnumerable<ArticlePreviewDTO>>> ReadAllArticlesAsync(string title)
    {
        var result =  await _context.Articles
                    .Where(c => c.Title.ToLower().Contains(title.ToLower().Trim()))
                    .Select(article =>
                        new ArticlePreviewDTO(
                                article.Id,
                                article.Title,
                                article.Type,
                                article.Created.ToUniversalTime(),
                                article.Description,
                                article.ProgrammingLanguages!.Select(p => p.Name).ToList(),
                                article.Difficulty,
                                article.AvgRating,
                                article.ImageURL
                                )
                    ).ToListAsync();
        
        return result.Any() ? result : null;
    }

    public async Task<Option<IEnumerable<ArticlePreviewDTO>>> ReadAllArticlesAsync(string title, string difficulty)
    {
        if (string.IsNullOrWhiteSpace(title)) title = "";
        var diffycultyToEnum = (DifficultyLevel) Enum.Parse(typeof(DifficultyLevel), difficulty);

        var result =  await _context.Articles
                    .Where(c => c.Title.ToLower().Contains(title.ToLower().Trim()))
                    .Where(article => article.Difficulty == diffycultyToEnum)
                    .Select(article =>
                        new ArticlePreviewDTO(
                                article.Id,
                                article.Title,
                                article.Type,
                                article.Created.ToUniversalTime(),
                                article.Description,
                                article.ProgrammingLanguages!.Select(p => p.Name).ToList(),
                                article.Difficulty,
                                article.AvgRating,
                                article.ImageURL
                                )
                    ).ToListAsync();
        
        return result.Any() ? result : null;
    }
    
    public async Task<Option<IEnumerable<ArticlePreviewDTO>>> ReadAllArticlesAsync(string title, string[] languages)
    {
        if (string.IsNullOrWhiteSpace(title)) title = "";

        var all = await _context.Articles
                .Where(c => c.Title.ToLower().Contains(title.ToLower().Trim()))
                .Select(article =>
                new ArticlePreviewDTO(
                        article.Id,
                        article.Title,
                        article.Type,
                        article.Created.ToUniversalTime(),
                        article.Description,
                        article.ProgrammingLanguages!.Select(p => p.Name).ToList() ?? new List<string>(),
                        article.Difficulty,
                        article.AvgRating,
                        article.ImageURL
                        )
            ).ToListAsync();

        var result =  all.Where(article => article.ProgrammingLanguages!.Intersect(languages).Any())
                  .Select(article =>
                        new ArticlePreviewDTO(
                                article.Id,
                                article.Title,
                                article.Type,
                                article.Created.ToUniversalTime(),
                                article.Description,
                                article.ProgrammingLanguages,
                                article.Difficulty,
                                article.AvgRating,
                                article.ImageURL
                                )
                    ).ToList();
        
        return result.Any() ? result : null;
    }

    public async Task<Option<IEnumerable<ArticlePreviewDTO>>> ReadAllArticlesAsync(string title, string difficulty, string[] languages)
    {
        if (string.IsNullOrWhiteSpace(title)) title = "";
        var diffycultyToEnum = (DifficultyLevel) Enum.Parse(typeof(DifficultyLevel), difficulty);

        var allWithDifficulty = await _context.Articles
                .Where(c => c.Title.ToLower().Contains(title.ToLower().Trim()))
                .Where(article => article.Difficulty == diffycultyToEnum)
                .Select(article =>
                    new ArticlePreviewDTO(
                            article.Id,
                            article.Title,
                            article.Type,
                            article.Created.ToUniversalTime(),
                            article.Description,
                            article.ProgrammingLanguages!.Select(p => p.Name).ToList() ?? new List<string>(),
                            article.Difficulty,
                            article.AvgRating,
                            article.ImageURL
                            
                            )
                ).ToListAsync();

        var result = allWithDifficulty
                    .Where(article => article.ProgrammingLanguages!.Intersect(languages).Any())
                    .Select(article =>
                        new ArticlePreviewDTO(
                                article.Id,
                                article.Title,
                                article.Type,
                                article.Created.ToUniversalTime(),
                                article.Description,
                                article.ProgrammingLanguages,
                                article.Difficulty,
                                article.AvgRating,
                                article.ImageURL
                            
                                )
                    ).ToList();

        return result.Any() ? result : null;
    }
    
    public async Task<Status> UpdateArticleAsync(int id, ArticleUpdateDTO article)
    { 
        var entity = _context.Articles.ToList().Find(c => c.Id == id);

        if (entity == null)
        {
            return Status.NotFound;
        }

        _context.Articles.Remove(entity);
        await _context.SaveChangesAsync();
        
        entity.Description = article.Description;
        entity.Difficulty = article.Difficulty;
        entity.Title = article.Title;
        entity.Body = article.Body;
        entity.AvgRating = article.AvgRating;
        entity.ImageURL = article.ImageURL;
        entity.ProgrammingLanguages = await GetProgrammingLanguagesAsync(article.ProgrammingLanguages!).ToListAsync();
        entity.VideoURL = article.VideoURL;
        
        _context.Articles.Add(entity);
        await _context.SaveChangesAsync();

        return Status.Updated;
    }

    //Heavily inspired by github.com/ondfisk/BDSA2021, credit to Author Rasmus Lystrøm
    private async IAsyncEnumerable<ProgrammingLanguage> GetProgrammingLanguagesAsync(IEnumerable<string> languages)
    {
        var existing = await _context.ProgrammingLanguages.Where(l => languages.Contains(l.Name)).ToDictionaryAsync(p => p.Name);
        
        foreach (var language in languages)
        {
            yield return existing.TryGetValue(language, out var p) ? p : new ProgrammingLanguage(language);
        }
    }
}
