﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Icarus.Controllers.Managers;
using Icarus.Controllers.Utilities;
using Icarus.Models;
using Icarus.Models.Context;

namespace Icarus.Controllers
{
	[Route("api/song")]
	[ApiController]
    	public class SongController : ControllerBase
    	{
		#region Fields
		private readonly ILogger<SongController> _logger;
		private IConfiguration _config;
		private SongManager _songMgr;
		#endregion


		#region Properties
		#endregion


		#region Constructor
		public SongController(IConfiguration config, ILogger<SongController> logger)
		{
			_config = config;
			_logger = logger;
			_songMgr = new SongManager(config);
		}
		#endregion


        	[HttpGet]
		[Authorize("read:song_details")]
        	public IActionResult Get()
        	{
			List<Song> songs = new List<Song>();
			Console.WriteLine("Attemtping to retrieve songs");
			_logger.LogInformation("Attempting to retrieve songs");
			
			MusicStoreContext context = HttpContext
				.RequestServices
				.GetService(typeof(MusicStoreContext)) as MusicStoreContext;

			songs = context.GetAllSongs();

			if (songs.Count > 0)
			{
				return Ok(songs);
			}
			else
			{
				return NotFound();
			}
        	}

		[HttpGet("{id}")]
		[Authorize("read:song_details")]
		public IActionResult Get(int id)
		{
			MusicStoreContext context = HttpContext
				.RequestServices
				.GetService(typeof(MusicStoreContext)) as MusicStoreContext;
			
			Song song = new Song { Id = id };
			song = context.GetSong(song);

			Console.WriteLine("Here");

			if (song.Id != 0)
			{
				return Ok(song);
			}
			else
			{
				return NotFound();
			}
		}

		[Authorize("update:songs")]
        	[HttpPut("{id}")]
		public IActionResult Put(int id, [FromBody] Song song)
        	{
			MusicStoreContext context = HttpContext
				.RequestServices
				.GetService(typeof(MusicStoreContext)) as MusicStoreContext;

			ArtistStoreContext artistStore = HttpContext
				.RequestServices
				.GetService(typeof(ArtistStoreContext)) as ArtistStoreContext;

			AlbumStoreContext albumStore = HttpContext
				.RequestServices
				.GetService(typeof(AlbumStoreContext)) as AlbumStoreContext;
			
			GenreStoreContext genreStore = HttpContext
				.RequestServices
				.GetService(typeof(GenreStoreContext)) as GenreStoreContext;

			YearStoreContext yearStore = HttpContext
				.RequestServices
				.GetService(typeof(YearStoreContext)) as YearStoreContext;

			song.Id = id;
			Console.WriteLine("Retrieving filepath of song");
			_logger.LogInformation("Retrieving filepath of song");

			if (!context.DoesSongExist(song))
			{
				return NotFound(new SongResult
					{
						Message = "Song does not exist"
					});
			}

			var songRes = _songMgr.UpdateSong(song, context, albumStore, artistStore, genreStore, 
					yearStore);

			return Ok(songRes);
        	}
    	}
}
