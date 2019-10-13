#include "manager/UserManager.h"

#include "database/UserRepository.h"
#include "type/UserFilter.h"
#include "type/SaltFilter.h"
#include "utility/MetadataRetriever.h"
#include "utility/PasswordEncryption.h"

namespace manager {
UserManager::UserManager(const model::BinaryPath& bConf) : m_bConf(bConf) { }


model::RegisterResult UserManager::registerUser(model::User& user) {
    model::RegisterResult result;
    result.username = user.username;
    printUser(user);

    utility::PasswordEncryption passEnc;
    auto hashed = passEnc.hashPassword(user);
    user.password = hashed.hashPassword;

    database::UserRepository usrRepo(m_bConf);
    usrRepo.saveUserRecord(user);

    model::User usr;
    usr.username = user.username;

    std::cout << usr.username << std::endl;
    usr = usrRepo.retrieveUserRecord(usr, type::UserFilter::username);
    hashed.hashPassword = usr.password;
    hashed.userId = usr.id;

    usrRepo.saveUserSalt(hashed);
    result.registered = true;
    result.message = "successfully registered";
    printUser(usr);

    return result;
}


bool UserManager::doesUserExist(const model::User& user) {
    database::UserRepository userRepo(m_bConf);

    return userRepo.doesUserRecordExist(user, type::UserFilter::username);
}

bool UserManager::validatePassword(const model::User& user) {
    database::UserRepository userRepo(m_bConf);
    model::User usr;
    usr.username = user.username;
    usr = userRepo.retrieveUserRecord(usr, type::UserFilter::username);

    model::PassSec userSec;
    userSec.userId = usr.id;
    userSec = userRepo.retrieverUserSaltRecord(userSec, type::SaltFilter::userId);
    userSec.hashPassword = usr.password;

    utility::PasswordEncryption passEnc;

    return passEnc.isPasswordValid(user, userSec);
}


void UserManager::printUser(const model::User& user) {
    std::cout << "\nuser info" << std::endl;
    std::cout << "id: " << user.id << std::endl;
    std::cout << "firstname: " << user.firstname << std::endl;
    std::cout << "lastname: " << user.lastname << std::endl;
    std::cout << "phone: " << user.phone << std::endl;
    std::cout << "email: " << user.email << std::endl;
    std::cout << "username: " << user.username << std::endl;
    std::cout << "password: " << user.password<< std::endl;
}

}
