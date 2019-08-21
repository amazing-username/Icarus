#include <exception>
#include <iostream>
#include <filesystem>
#include <memory>
#include <string>

#include <mysql/mysql.h>
#include "oatpp/network/server/Server.hpp"
#include "oatpp/network/server/SimpleTCPConnectionProvider.hpp"
#include "oatpp/web/server/HttpConnectionHandler.hpp"

#include "appComponent.hpp"
#include "controller/loginController.hpp"
#include "controller/songController.hpp"
#include "database/base_repository.h"
#include "models/models.h"

namespace fs = std::filesystem;

void run(const std::string& working_path)
{
    appComponent component;

    OATPP_COMPONENT(std::shared_ptr<oatpp::web::server::HttpRouter>, router);

    auto logController = std::make_shared<loginController>(working_path);
    auto sngController = std::make_shared<songController>(working_path);
    logController->addEndpointsToRouter(router);
    sngController->addEndpointsToRouter(router);

    OATPP_COMPONENT(std::shared_ptr<oatpp::network::server::ConnectionHandler>, connectionHandler);

    OATPP_COMPONENT(std::shared_ptr<oatpp::network::ServerConnectionProvider>, connectionProvider);

    oatpp::network::server::Server server(connectionProvider, connectionHandler);

    OATPP_LOGI("icarus", "Server running on port %s", connectionProvider->getProperty("port").getData());

    server.run();
}

void test_database()
{
    database_connection mysqlD;
    mysqlD.server = "";  // where the mysql database is
    mysqlD.username = "";		// the root user of mysql
    mysqlD.password = ""; // the password of the root user in mysql
    mysqlD.database = "";	// the databse to pick

    base_repository base;
    auto conn = base.setup_mysql_connection(mysqlD);

    // assign the results return to the MYSQL_RES pointer
    const std::string query = "SELECT *, NULL FROM Song";
    auto res = base.perform_mysql_query(conn, query);
    auto num_of_fields = mysql_num_fields(res);

    printf("MySQL Tables in mysql database:\n");
    auto row_count = 1;
    
    for (MYSQL_ROW row = NULL; (row = mysql_fetch_row(res)) != NULL;) {
        std::cout << " row " << row_count << std::endl;
        for (auto i = 0; i != num_of_fields; ++i) {
            auto val = row[i];

            if ( val == NULL) {
                std::cout << "found null value " << std::endl;
                continue;
            }
            std::cout << val << " ";
        }
        std::cout << std::endl;
    }

    /* clean up the database result set */
    mysql_free_result(res);
    /* clean up the database link */
    mysql_close(conn);
}


int main(int argc, char **argv)
{
    oatpp::base::Environment::init();
    std::string working_path = argv[0];

    //test_database();
    run(working_path);

    oatpp::base::Environment::destroy();

    return 0;
}
