﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

using Icarus.Controllers.Managers;
using Icarus.Models;

namespace Icarus.Controllers
{
    [Route("api/Song")]
    [ApiController]
    public class SongController : ControllerBase
    {
		#region Fields
		private SongManager _songMgr;
		#endregion


		#region Properties
		#endregion


		#region Constructor
		public SongController(IConfiguration config)
		{
			_songMgr = new SongManager(config);
		}
		#endregion


        // GET api/song
        [HttpGet]
        public ActionResult<IEnumerable<Song>> Get()
        {
			List<Song> songs = new List<Song>();


            return songs;
        }

        // GET api/song/5
        [HttpGet("{id}")]
        public ActionResult<Song> Get(int id)
        {
			Song song = _songMgr.RetrieveSong(id);

            return song;
        }

        // POST api/song
        [HttpPost]
        public void Post([FromBody] Song song)
        {
			Console.WriteLine("The song's title is: ");
			Console.WriteLine(song.Title);
			_songMgr.SaveSong(song);
        }

        // PUT api/song/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] Song song)
        {
        }

        // DELETE api/song/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
