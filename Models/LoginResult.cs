using System;

using Newtonsoft.Json;

namespace Icarus.Models
{
	public class LoginResult
	{
		[JsonProperty("id")]
		public int UserId { get; set; }
		[JsonProperty("username")]
		public string Username { get; set; }
		[JsonProperty("token")]
		public string Token { get; set; }
		[JsonProperty("expiration")]
		public int Expiration { get; set; }
	}
}
