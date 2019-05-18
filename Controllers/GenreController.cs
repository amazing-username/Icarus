using System;
using System.Collections.Generic;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Icarus.Models;
using Icarus.Models.Context;

namespace Icarus.Controllers
{
	[Route("api/genre")]
	[ApiController]
	public class GenreController : ControllerBase
	{
		#region Fields
		private readonly ILogger<GenreController> _logger;
		#endregion


		#region Properties
		#endregion


		#region Constructors
		public GenreController(ILogger<GenreController> logger)
		{
			_logger = logger;
		}
		#endregion


		#region HTTP Routes
		[HttpGet]
		public IActionResult Get()
		{
			var genres = new List<Genre>();

			var genreStore = HttpContext
				.RequestServices
				.GetService(typeof(GenreStoreContext)) as GenreStoreContext;

			genres = genreStore.GetGenres();

			if (genres.Count > 0)
			{
				return Ok(genres);
			}
			else
			{
				return NotFound(new List<Genre>());
			}
		}

		[HttpGet("{id}")]
		public IActionResult Get(int id)
		{
			var genre = new Genre
			{
				GenreId = id
			};

			var genreStore = HttpContext
				.RequestServices
				.GetService(typeof(GenreStoreContext)) as GenreStoreContext;

			if (genreStore.DoesGenreExist(genre))
			{
				genre =  genreStore.GetGenre(genre);

				return Ok(genre);
			}
			else
			{
				return NotFound(new Genre());
			}
		}
		#endregion
	}
}
