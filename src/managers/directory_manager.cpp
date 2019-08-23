#include <iostream>
#include <filesystem>
#include <fstream>
#include <sstream>

#include "managers/directory_manager.h"

namespace fs = std::filesystem;

std::string directory_manager::create_directory_process(Song song, const std::string& root_path)
{
    auto curr_path = fs::path(root_path);

    if (fs::exists(curr_path)) {
        std::cout<<"path exists"<<std::endl;
    } else {
        std::cout<<"creating path"<<std::endl;
        fs::create_directory(curr_path);
    }

    auto art_path = fs::path(curr_path.string() + song.artist);
    if (fs::exists(art_path)) {
        std::cout<<"artist path exists"<<std::endl;
    } else {
        std::cout<<"creating artist path"<<std::endl;
        fs::create_directory(art_path);
    }

    auto alb_path = fs::path(art_path.string() + "/" + song.album);
    if (fs::exists(alb_path)) {
        std::cout<<"album path exists"<<std::endl;
    } else {
        std::cout<<"creating album path"<<std::endl;
        fs::create_directory(alb_path);
    }

    return alb_path.string() + "/";
}

std::string directory_manager::configPath(std::string_view path)
{
    return fs::canonical(path).parent_path().string();
}
std::string directory_manager::contentOfPath(std::string_view path)
{
    std::string configPath(path);
    std::fstream a(configPath, std::ios::in);
    std::stringstream s;
    s << a.rdbuf();
    a.close();

    return s.str();
}

nlohmann::json directory_manager::credentialConfigContent(const std::string& exe_path)
{
    auto path = configPath(exe_path);
    path.append("/authcredentials.json");

    return nlohmann::json::parse(contentOfPath(path));
}
nlohmann::json directory_manager::databaseConfigContent(const std::string& exe_path)
{
    auto path = configPath(exe_path);
    path.append("/database.json");

    return nlohmann::json::parse(contentOfPath(path));
}
nlohmann::json directory_manager::pathConfigContent(const std::string& exe_path)
{
    auto path = configPath(exe_path);
    path.append("/paths.json");

    return nlohmann::json::parse(contentOfPath(path));
}

/**
std::string directory_manager::read_cover_art(const std::string& source)
{
    auto source_path = fs::path(source);
    
    std::fstream cov(source, std::ios::in | std::ios::binary);

    cov.seekg(0);

    std::stringstream buf;
    std::copy(std::istreambuf_iterator<char>(cov),
        std::istreambuf_iterator<char>(),
        std::ostreambuf_iterator<char>(buf));

    return buf.str();
}

void directory_manager::copy_stock_to_root(const std::string& target, const std::string& buff)
{
    std::cout<<"starting process"<<std::endl;
    auto target_path = fs::path(target);
    if (fs::exists(target_path)) {
        std::cout<<target_path.string()<<" exists"<<std::endl;
        return;
    }

    std::cout<<target_path.string()<<" does not exist, copying over"<<std::endl;
    std::fstream cov(target, std::ios::out | std::ios::binary);
    cov.write(buff.c_str(), buff.size());
    cov.close();

    std::cout<<"copy finished"<<std::endl;
}
void directory_manager::copy_song_to_path(const std::string& target, const std::string& source)
{
    std::cout<<"starting process to copy song"<<std::endl;
    auto target_path = fs::path(target);
    auto src_path = fs::path(source);

    std::cout<<"copting over to "<<target_path.string()<<std::endl;
    fs::copy(src_path, target_path);

    fs::remove(source);
    std::cout<<"copy finished"<<std::endl;
}
*/
void directory_manager::delete_cover_art_file(const std::string& cov_path, const std::string& stock_cover_path)
{
    if (cov_path.compare(stock_cover_path) == 0) {
        std::cout << "cover has stock cover art, will not deleted" << std::endl;
    } else {
        std::cout << "deleting song path" << std::endl;
        auto cov = fs::path(cov_path);
        fs::remove(cov);
    }
}
void directory_manager::delete_directories(Song song, const std::string& root_path)
{
    std::cout<<"checking to for empty directories to delete"<<std::endl;
    const std::string art{root_path + std::string{"/"} + song.artist};
    const std::string alb{art + "/" + song.album};

    auto alb_path = fs::path(alb);
    
    if (!fs::exists(alb_path)) {
        std::cout<<"directory does not exists"<<std::endl;
    } else if (fs::is_empty(alb_path)) {
        fs::remove(alb_path);
    }

    auto art_path = fs::path(art);
    
    if (!fs::exists(art_path)) {
        std::cout<<"directory does not exists"<<std::endl;
        return;
    } else if (fs::is_empty(art_path)) {
        fs::remove(art_path);
    }

    std::cout<<"deleted empty directory or directories"<<std::endl;
}
void directory_manager::delete_song(const Song song)
{
    std::cout<<"deleting song"<<std::endl;
    auto song_path = fs::path(song.songPath);

    if (!fs::exists(song_path)) {
        std::cout<<"song does not exists"<<std::endl;
        return;
    }

    fs::remove(song_path);
    std::cout<<"deleted song"<<std::endl;
}

