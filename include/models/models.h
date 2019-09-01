#ifndef MODELS_H_
#define MODELS_H_

#include <string>
#include <vector>

namespace Model
{
    struct Song
    {
        int id;
        std::string title;
        std::string artist;
        std::string album;
        std::string genre;
        int year;
        int duration;
        int track;
        int disc;
        std::string songPath;
        std::vector<unsigned char> data;
        int coverArtId;
        int artistId;
        int albumId;
        int genreId;
        int yearId;
    };

    struct Artist
    {
        int id;
        std::string artist;
    };

    struct Album
    {
        int id;
        std::string title;
        int year;
        std::vector<Song> songs;
    };

    struct Genre
    {
        int id;
        std::string category;

    };

    struct Year
    {
        int id;
        int year;
    };

    struct Cover
    {
        int id;
        std::string songTitle;
        std::string imagePath;
        // Not being used but it should be
        std::vector<unsigned char> data;
    };

    struct loginResult
    {
        int user_id;
        std::string username;
        std::string access_token;
        std::string token_type;
        std::string message;
        int expiration;
    };

    struct auth_credentials
    {
        std::string domain;
        std::string api_identifier;
        std::string client_id;
        std::string client_secret;
        std::string uri;
        std::string endpoint;
    };

    struct database_connection
    {
        std::string server;
        std::string username;
        std::string password;
        std::string database;
    };

    struct BinaryPath
    {
        BinaryPath() = default;
        BinaryPath(const char *p) : path(std::move(p)) { }
        std::string path;
    };
}

#endif
