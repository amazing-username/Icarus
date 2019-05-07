using System;
using System.Collections.Generic;
using System.Globalization;

using MySql.Data.MySqlClient;
using NLog;

using Icarus.Models;

namespace Icarus.Models.Context
{
	public class AlbumStoreContext : BaseStoreContext
	{
		#region Fields
		private static Logger _logger = NLog.LogManager.GetCurrentClassLogger();
		#endregion


		#region Properties
		#endregion


		#region Constructors
		public AlbumStoreContext(string connectionString)
		{
			_connectionString = connectionString;
		}
		#endregion


		#region Methods
		public List<Album> GetAlbums()
		{
			List<Album> albums = new List<Album>();

			try
			{
				using (MySqlConnection conn = GetConnection())
				{
					conn.Open();
					var query = "SELECT * FROM Album";
					using (MySqlCommand cmd = new MySqlCommand(query, conn))
					{
						using (var reader = cmd.ExecuteReader())
						{
							while (reader.Read())
							{
								var id = Convert.ToInt32(reader["Id"]);
								var title = reader["Title"].ToString();
								var albumArtist = reader["AlbumArtist"].ToString();
								var songCount = Convert.ToInt32(reader["SongCount"]);
								albums.Add(new Album
								{
									Id = id,
									Title = title,
									AlbumArtist = albumArtist,
									SongCount = songCount
								});
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				var msg = ex.Message;
				_logger.Error(msg, "An error occurred");
			}

			return albums;
		}

		public bool DoesAlbumExist(Album album)
		{
			try
			{
			}
			catch (Exception ex)
			{
				var msg = ex.Message;
				_logger.Error(msg, "An error occurred");
			}
			return false;
		}

		public void SaveAlbum(Album album)
		{
			try
			{
				using (MySqlConnection conn = GetConnection())
				{
					conn.Open();
					var query = "INSERT INTO Album(Title, AlbumArtist, " +
						"SongCount) VALUES (@Title, @AlbumArtist, " +
						"SongCount)";
					using (MySqlCommand cmd = new MySqlCommand(query, conn))
					{
						cmd.Parameters.AddWithValue("@Title", album.Title);
						cmd.Parameters.AddWithValue("@AlbumArtist", album.AlbumArtist);
						cmd.Parameters.AddWithValue("@SongCount", album.SongCount);

						cmd.ExecuteNonQuery();
					}
				}
			}
			catch (Exception ex)
			{
				var msg = ex.Message;
				_logger.Error(msg, "An error occurred");
			}
		}

		private List<Album> ParseData(MySqlDataReader reader)
		{
			List<Album> albums = new List<Album>();
			while (reader.Read())
			{
				var id = Convert.ToInt32(reader["Id"]);
				var title = reader["Title"].ToString();
				var albumArtist = reader["AlbumArtist"].ToString();
				var songCount = Convert.ToInt32(reader["SongCount"]);
				albums.Add(new Album
				{
					Id = id,
					Title = title,
					AlbumArtist = albumArtist,
					SongCount = songCount
				});
			}

			return albums;
		}
		private Album ParseSingleData(MySqlDataReader reader)
		{
			Album album = new Album();
			while (reader.Read())
			{
				var id = Convert.ToInt32(reader["Id"]);
				var title = reader["Title"].ToString();
				var albumArtist = reader["AlbumArtist"].ToString();
				var songCount = Convert.ToInt32(reader["SongCount"]);
				album = new Album
				{
					Id = id,
					Title = title,
					AlbumArtist = albumArtist,
					SongCount = songCount
				};
			}

			return album;
		}
		#endregion
	}
}
