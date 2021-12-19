﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using SETraining.Server.Repositories;
using SETraining.Shared.DTOs;
using SETraining.Shared.ExtensionMethods;

namespace SETraining.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class ArticleController : ControllerBase
    {
        private readonly ILogger<ArticleController> _logger;
        private readonly IArticleRepository _repository;

        public ArticleController(ILogger<ArticleController> logger, IArticleRepository repository)
        {
            _logger = logger;
            _repository = repository;
        }

        [ProducesResponseType(404)]
        [ProducesResponseType(typeof(ArticlePreviewDTO), 200)]
        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<ArticlePreviewDTO>>> Get()
        {
            var res = await _repository.ReadAllArticlesAsync();
            return res.ToActionResult();
        }

        [ProducesResponseType(404)]
        [ProducesResponseType(typeof(ArticlePreviewDTO), 200)]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ArticlePreviewDTO>>> GetFromParameters([FromQuery]string? title, [FromQuery]string? difficulty, [FromQuery]string[]? languages)
        {
            var res = await _repository.ReadAllArticlesFromParametersAsync(title!, difficulty!, languages!);
            return res.ToActionResult();
        }
        
        [ProducesResponseType(404)]
        [ProducesResponseType(typeof(ArticleDTO), 200)]
        [HttpGet("id={id}")]
        public async Task<ActionResult<ArticleDTO>> GetFromId(int id)
            => (await _repository.ReadArticleFromIdAsync(id)).ToActionResult();



        [HttpPost]
        [ProducesResponseType(typeof(ArticleDTO), 201)]
        public async Task<IActionResult> Post(ArticleCreateDTO article)
        {
            var created = await _repository.CreateArticleAsync(article);
            Console.WriteLine(created);
            return CreatedAtAction(nameof(Get), new { created.Id }, created);
        }

        //put = update
        [Authorize]
        [HttpPut ("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task <IActionResult> Put(int id, [FromBody] ArticleUpdateDTO article)
            => (await _repository.UpdateArticleAsync(id, article)).ToActionResult();
        

        //delete
        [Authorize]
        [HttpDelete ("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task <IActionResult> Delete(int id)
            => (await _repository.DeleteArticleAsync(id)).ToActionResult();
    }
}

