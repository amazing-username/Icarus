using System;

using Newtonsoft.Json;

namespace Icarus.Models
{
	public class Song
	{
		[JsonIgnore]
		private MusicStoreContext _context;

		[JsonProperty("id")]
		public int Id { get; set; }
		[JsonProperty("title")]
		public string Title { get; set; }
		[JsonProperty("album")]
		public string Album { get; set; }
		[JsonProperty("artist")]
		public string Artist { get; set; }
		[JsonProperty("year")]
		public int Year { get; set; }
		[JsonProperty("genre")]
		public string Genre { get; set; }
		[JsonProperty("duration")]
		public int Duration { get; set; }
		[JsonProperty("filename")]
		public string Filename { get; set; }
		[JsonProperty("song_path")]
		public string SongPath { get; set; }
	}
}
