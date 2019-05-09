using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

using Id3;
using Id3.Frames;
using MySql.Data;
using MySql.Data.MySqlClient;
using NLog;
using TagLib;

using Icarus.Controllers.Utilities;
using Icarus.Models;
using Icarus.Models.Context;

namespace Icarus.Controllers.Managers
{
	public class SongManager
    	{
        	#region Fields
		private static Logger _logger = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
        	private MySqlConnection _conn;
        	private MySqlCommand _cmd;
        	private MySqlDataAdapter _dataDump;
        	private DataTable _results;
		private List<Song> _songs;
		private Song _song;
		private IConfiguration _config;
		private string _connectionString;
		private string _tempDirectoryRoot;
		private string _archiveDirectoryRoot;
		private string _compressedSongFilename;
		#endregion


		#region Properties
		public Song SongDetails
		{
	    		get => _song;
	    		set => _song = value;
		}

		public string ArchiveDirectoryRoot
		{
			get => _archiveDirectoryRoot;
		    	set => _archiveDirectoryRoot = value;
		}
		public string CompressedSongFilename
		{
	 	   	get => _compressedSongFilename;
	    		set => _compressedSongFilename = value;
		}
		#endregion


		#region Constructors
		public SongManager()
		{
	    		Initialize();
            		InitializeConnection();
		}

		public SongManager(Song song)
		{
	    		Initialize();
            		InitializeConnection();
	    		_song = song;
		}
		public SongManager(IConfiguration config)
		{
	    		_config = config;
	    		Initialize();
            		InitializeConnection();
		}
		public SongManager(IConfiguration config, string tempDirectoryRoot)
		{
	    		_config = config;
	    		_tempDirectoryRoot = tempDirectoryRoot;
	    		Initialize();
            		InitializeConnection();
		}
		#endregion


		#region Methods
		public bool DeleteSongFromFileSystem(Song songMetaData)
		{
		    	bool successful = false;
	    		try
	    		{
	        		var songPath = songMetaData.SongPath;
				System.IO.File.Delete(songPath);
				successful = true;
				DirectoryManager dirMgr = new DirectoryManager(_config, songMetaData);
				dirMgr.DeleteEmptyDirectories();
				Console.WriteLine("Song successfully deleted");
	    		}
	    		catch (Exception ex)
	    		{
	        		var exMsg = ex.Message;
	    		}

	    		return successful;
		}

		public void SaveSongDetails()
		{
			try
	    		{
	        		using (MySqlConnection conn = new MySqlConnection(_connectionString))
				{
		    			conn.Open();
		    			string query = "INSERT INTO Songs(Title, AlbumTitle, Artist, Year, Genre, Duration, " +
					   "Filename, SongPath) VALUES(@Title, @AlbumTitle, @Artist, @Year, @Genre, " +
					   "@Duration, @Filename, @SongPath)";
					using (MySqlCommand cmd = new MySqlCommand(query, conn))
		    			{
		        			cmd.Parameters.AddWithValue("@Title", _song.Title);
						cmd.Parameters.AddWithValue("@AlbumTitle", _song.AlbumTitle);
						cmd.Parameters.AddWithValue("@Artist", _song.Artist);
						cmd.Parameters.AddWithValue("@Year", _song.Year);
						cmd.Parameters.AddWithValue("@Genre", _song.Genre);
						cmd.Parameters.AddWithValue("@Duration", _song.Duration);
						cmd.Parameters.AddWithValue("@Filename", _song.Filename);
						cmd.Parameters.AddWithValue("@SongPath", _song.SongPath);

						cmd.ExecuteNonQuery();
		    			}
				}
	    		}
	    		catch (Exception ex)
	    		{
	        		var exMsg = ex.Message;
				Console.WriteLine($"An Error Occurred: {exMsg}");
	    		}
		}
		public void SaveSongDetails(Song song)
		{
	    		try
	    		{
	        		using (MySqlConnection conn = new MySqlConnection(_connectionString))
				{
		    			conn.Open();
		    			string query = "INSERT INTO Songs(Title, AlbumTitle, Artist, Year, Genre, Duration, " +
					   ", Filename, SongPath) VALUES(@Title, @AlbumTitle, @Artist, @Year, @Genre, " +
					   "@Duration, @Filename, @SongPath)";
		    			using (MySqlCommand cmd = new MySqlCommand(query, conn))
		    			{
		        			cmd.Parameters.AddWithValue("@Title", song.Title);
						cmd.Parameters.AddWithValue("@AlbumTitle", song.AlbumTitle);
						cmd.Parameters.AddWithValue("@Artist", song.Artist);
						cmd.Parameters.AddWithValue("@Year", song.Year);
						cmd.Parameters.AddWithValue("@Genre", song.Genre);
						cmd.Parameters.AddWithValue("@Duration", song.Duration);
						cmd.Parameters.AddWithValue("@Filename", song.Filename);
						cmd.Parameters.AddWithValue("@SongPath", song.SongPath);

						cmd.ExecuteNonQuery();
		   	 		}
				}
	    		}
	    		catch (Exception ex)
	    		{
	        		var exMsg = ex.Message;
				Console.WriteLine($"An Error Occurred: {exMsg}");
	    		}
		}

		public async Task SaveSong(SongData songData)
		{
	    		try
	    		{
				using (MySqlConnection conn = new MySqlConnection(_connectionString))
				{
		    			conn.Open();
		    			string query = "INSERT INTO SongData(Data) VALUES(@Data)";
		    			using (MySqlCommand cmd = new MySqlCommand(query, conn))
		    			{
		        			cmd.Parameters.AddWithValue("@Data", songData.Data);

						cmd.ExecuteNonQuery();
		    			}
				}
	    		}
	    		catch(Exception ex)
	    		{
	        		var exMsg = ex.Message;
	 			Console.WriteLine($"An error occurred: {exMsg}");
	    		}
		}
		public async Task SaveSongToFileSystem(IFormFile song)
		{
	    		try
	    		{
				Console.WriteLine("Saving song to the filesystem");
	        		var filePath = Path.Combine(_tempDirectoryRoot, song.FileName);
				Console.WriteLine("Saving song to the filePath");
				await SaveSongToFileSystemTemp(song, filePath);
				System.IO.File.Delete(filePath);


				DirectoryManager dirMgr = new DirectoryManager(_config, _song);
				dirMgr.CreateDirectory();
				filePath = dirMgr.SongDirectory;
				if (!song.FileName.EndsWith(".mp3"))
		    			filePath += $"{song.FileName}.mp3";
				else
		    			filePath += $"{song.FileName}";

				Console.WriteLine($"Full path {filePath}");

				using (var fileStream = new FileStream(filePath, FileMode.Create))
				{
		    			await song.CopyToAsync(fileStream);
		    			_song.SongPath = filePath;

		    			Console.WriteLine($"Writing song to the directory: {filePath}");
				}
	    		}
	    		catch (Exception ex)
	    		{
	        		var exMsg = ex.Message;
				Console.WriteLine($"An error occurred: {exMsg}");
	    		}
		}
		public async Task SaveSongToFileSystem(IFormFile songFile, MusicStoreContext sStoreContext,
				AlbumStoreContext alStoreContext,
				ArtistStoreContext arStoreContext) 
		{
			try
			{
				_logger.Info("Starting process to save song to the filesystem");
				var fileTempPath = Path.Combine(_tempDirectoryRoot, songFile.FileName);
				var song = await SaveSongTemp(songFile, fileTempPath);
				System.IO.File.Delete(fileTempPath);

				DirectoryManager dirMgr = new DirectoryManager(_config, song);
				dirMgr.CreateDirectory();
				var filePath = dirMgr.SongDirectory;
				if (!songFile.FileName.EndsWith(".mp3"))
					filePath += $"{songFile.FileName}.mp3";
				else
					filePath += $"{songFile.FileName}";

				_logger.Info($"Absolute song path: {filePath}");

				using (var fileStream = new FileStream(filePath, FileMode.Create))
				{
					await (songFile.CopyToAsync(fileStream));
					song.SongPath = filePath;

					_logger.Info("Song Successfully saved");
				}
				SaveSongToDatabase(song, sStoreContext, alStoreContext, arStoreContext);
			}
			catch (Exception ex)
			{
				var msg = ex.Message;
				_logger.Error(msg, "An error occurred");
			}
		}

        	public async Task<List<Song>> RetrieveAllSongDetails()
        	{
            		try
            		{
                		InitializeResults();
                		_songs = new List<Song>();
                		_conn.Open();
                		string query = "SELECT * FROM Songs";

                		_cmd = new MySqlCommand(query, _conn);
                		_cmd.ExecuteNonQuery();

                		_dataDump = new MySqlDataAdapter(_cmd);
                		_dataDump.Fill(_results);
                		_dataDump.Dispose();

                		await PopulateSongDetails();


                		_conn.Close();

            		}
            		catch (Exception ex)
            		{
                		var exMsg = ex.Message;
                		Console.WriteLine($"An error ocurred: {exMsg}");
            		}

            		return _songs;
        	}
		public async Task<Song> RetrieveSongDetails(int id)
		{
	    		DataTable results = new DataTable();

	    		try
	    		{
	        		using (MySqlConnection conn = new MySqlConnection(_connectionString))
				{
		    			conn.Open();
		    			string query = "SELECT * FROM Songs WHERE Id=@Id";
		    			using (MySqlCommand cmd = new MySqlCommand(query, conn))
		    			{
		        			cmd.Parameters.AddWithValue("@Id", id);
		        	
						cmd.ExecuteNonQuery();

	                			using (MySqlDataAdapter dataDump = new MySqlDataAdapter(cmd))
						{
			    				dataDump.Fill(results);
						}
		    			}
				}
	    		}
	    		catch (Exception ex)
	    		{
	        		var exMsg = ex.Message;
				Console.WriteLine($"An error occurred");
	    		}	
	    
       	    		DataRow row = results.Rows[0];

	    		return new Song
	    		{
	        		Id = Int32.Parse(row["Id"].ToString()),
				Filename = row["Filename"].ToString(),
				SongPath = row["SongPath"].ToString()
	    		};
		}
		public async Task<SongData> RetrieveSong(int id)
		{
	    		SongData song = new SongData();
	    		try
	    		{
	        		_song = await RetrieveSongDetails(id);
				song = await RetrieveSongFromFileSystem(_song);
	    		}
	    		catch (Exception ex)
	    		{
	        		var exMsg = ex.Message;
				Console.WriteLine($"An error occurred: {exMsg}");
	    		}

	    		return song;
		}
		public async Task<SongData> RetrieveSong(Song songMetaData)
		{
	    		SongData song = new SongData();
	    		try
	    		{
				Console.WriteLine("Fetching song from filesystem");
				song = await RetrieveSongFromFileSystem(songMetaData);
	    		}
	    		catch (Exception ex)
	    		{
	        		var exMsg = ex.Message;
				Console.WriteLine($"An error occurred: {exMsg}");
	    		}

	    		return song;
		}


		private Song RetrieveMetaData(string filePath)
		{
			Song newSong = new Song
			{
	        		Title = "Untitled",
				Artist = "Untitled",
				AlbumTitle = "Untitled",
				Year = 0,
				Genre = "Untitled",
				Duration = 0,
				SongPath = ""
			};
	    		string title, artist, album, genre;
	    		int year, duration;
			
			Console.WriteLine("Stripping song metadata");
			try
			{
	    			TagLib.File tfile = TagLib.File.Create(filePath);


	    			using (var mp3 = new Mp3(filePath))
    	    			{
                			Id3Tag tag = mp3.GetTag(Id3TagFamily.Version2X);
					title = tag.Title;
        				Console.WriteLine("Title: {0}", title);
					newSong.Title = title;
					artist = tag.Artists;
        				Console.WriteLine("Artist: {0}", artist);
					newSong.Artist = artist;
	        			album = tag.Album;
        				Console.WriteLine("Album: {0}", album);
					newSong.AlbumTitle = album;
					genre = "Not Implemented";
					Console.WriteLine("Genre: {0}", genre);
					newSong.Genre = genre;
					year = (int)tag.Year;
					Console.WriteLine("Year: {0}", year);
					newSong.Year = year;
	    				duration = (int)tfile.Properties.Duration.TotalSeconds;
					Console.WriteLine("Duration: {0}", duration);
					newSong.Duration = duration;
    	    			}

	    			_song = newSong;
			}
			catch (Exception ex)
			{
				var msg = ex.Message;
				Console.WriteLine($"An error occurred when stripping metadata\n{msg}");
				_song = newSong;
			}

	    		return newSong;
		}

		private async Task<SongData> RetrieveSongFromFileSystem(Song details)
		{
            		byte[] uncompressedSong = System.IO.File.ReadAllBytes(details.SongPath);
			
	    		return new SongData
	    		{
	        		Data = uncompressedSong
	    		};
		}
		private async Task<Song> SaveSongTemp(IFormFile songFile, string filePath)
		{
			var song = new Song();


			using (var filestream = new FileStream(filePath, FileMode.Create))
			{
				_logger.Info("Saving song to temporary directory");
				await songFile.CopyToAsync(filestream);
				MetadataRetriever meta = new MetadataRetriever();
				song =  meta.RetrieveMetaData(filePath);

				_logger.Info("Assigning song filename");
				song.Filename = songFile.FileName;
			}

			return song;
		}

		private async Task SaveSongToFileSystemTemp(IFormFile song, string filePath)
		{
	    		using (var fileStream = new FileStream(filePath, FileMode.Create))
	    		{
				Console.WriteLine("Retrieving song and storing it in memory");
				_logger.Info("Retrieving song and storing it in memory");
	        		await song.CopyToAsync(fileStream);
				Console.WriteLine($"Retrieving metadata of song from filepath {filePath}");
				_logger.Info($"Retrieving metadata of song from filepath {filePath}");
				MetadataRetriever meta = new MetadataRetriever();
				_song = meta.RetrieveMetaData(filePath);
	
				Console.WriteLine("Assigning song filename");
				_song.Filename = song.FileName;
				Console.WriteLine($"Song filename retrieved: {song.FileName}");
	    		}
		}

		private void Initialize()
		{
	    		try
	    		{
	        		_connectionString = _config.GetConnectionString("IcarusDev");
	    		}
	    		catch (Exception ex)
	    		{
	        		Console.WriteLine($"Error Occurred: {ex.Message}");
	    		}
			
		}
        	private void InitializeConnection()
        	{
            		_conn = new MySqlConnection(_connectionString);
        	}
       	 	private void InitializeResults()
        	{
            		_results = new DataTable();
        	}
		private void SaveSongToDatabase(Song song, MusicStoreContext songStore, AlbumStoreContext albumStore,
				ArtistStoreContext artistStore)
		{
			song.AlbumId = 1;
			var album = new Album();
			album.Title = song.AlbumTitle;
			album.AlbumArtist = song.Artist;
			if (!albumStore.DoesAlbumExist(song))
			{
				album.SongCount = 1;
				_logger.Info("Saving album to database");
				_logger.Info($"Ablum song count before retrieving from store: {album.SongCount}");
				_logger.Info($"Ablum Id before retrieving from store: {album.AlbumId}");
				albumStore.SaveAlbum(album);
				_logger.Info("Album suuccessfully saved to database");
				album = albumStore.GetAlbum(song);
				_logger.Info($"Ablum song count after retrieving from store: {album.SongCount}");
				_logger.Info($"Ablum Id after retrieving from store: {album.AlbumId}");
			}
			else
			{
				var albumRetrieved = albumStore.GetAlbum(song);
				album.AlbumId = albumRetrieved.AlbumId;
				album.SongCount = albumRetrieved.SongCount + 1;

				albumStore.UpdateAlbum(album);
			}
			
			song.AlbumId = album.AlbumId;
			_logger.Info($"Song;\nTitle {song.Title}\nAlbum {song.AlbumTitle}\nAlbum Id {song.AlbumId}");

			songStore.SaveSong(song);
			_logger.Info("Successfully saved song to database");
		}

        	private async Task PopulateSongDetails()
        	{
            		foreach (DataRow row in _results.Rows)
            		{
                		Song song = new Song();
                		foreach (DataColumn col in _results.Columns)
                		{
                    			string colStr = col.ToString().ToUpper();
                    			switch (colStr)
                    			{
                        			case "ID":
                            				song.Id = Int32.Parse(row[col].ToString());
                            				break;
                        			case "TITLE":
                            				song.Title = row[col].ToString();
                            				break;
                        			case "ALBUM":
                            				song.AlbumTitle = row[col].ToString();
                            				break;
                        			case "ARTIST":
                            				song.Artist = row[col].ToString();
                            				break;
                        			case "YEAR":
                            				song.Year = Int32.Parse(row[col].ToString());
                            				break;
                        			case "GENRE":
                            				song.Genre = row[col].ToString();
                            				break;
                        			case "DURATION":
                            				song.Duration = Int32.Parse(row[col].ToString());
                            				break;
                        			case "FILENAME":
                            				song.Filename = row[col].ToString();
                            				break;
                        			case "SONGPATH":
                            				song.SongPath = row[col].ToString();
                            			break;
                    			}
                		}
                		_songs.Add(song);
            		}
        	}
        	#endregion	
    	}
}
