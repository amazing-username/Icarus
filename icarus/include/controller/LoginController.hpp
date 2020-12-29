#ifndef LOGINCONTROLLER_H_
#define LOGINCONTROLLER_H_

#include <iostream>
#include <string>
#include <memory>

#include "icarus_lib/icarus.h"
#include "oatpp/core/macro/codegen.hpp"
#include "oatpp/core/macro/component.hpp"
#include "oatpp/web/server/api/ApiController.hpp"

#include "controller/BaseController.hpp"
#include "dto/conversion/DtoConversions.h"
#include "dto/LoginResultDto.hpp"
#include "dto/conversion/DtoConversions.h"
#include "manager/TokenManager.h"
#include "manager/UserManager.h"

namespace controller
{
    class LoginController : public BaseController
    {
    public:
		LoginController(const icarus_lib::binary_path &bConf, 
                        OATPP_COMPONENT(std::shared_ptr<ObjectMapper>, object_mapper)) : 
							BaseController(bConf, object_mapper)
        {
        }


		#include OATPP_CODEGEN_BEGIN(ApiController)
    
		ENDPOINT("POST", "/api/v1/login", data, BODY_DTO(oatpp::Object<UserDto>, usr))
        {
		    OATPP_LOGI("icarus", "logging in");

		    manager::UserManager usrMgr(m_bConf);
		    auto user = dto::conversion::DtoConversions::toUser(usr);

		    if (!usrMgr.doesUserExist(user) || !usrMgr.validatePassword(user)) {
		    	auto logRes = dto::LoginResultDto::createShared();

		        logRes->message = "invalid credentials";

		        std::cout << "user does not exist\n";

		        return createDtoResponse(Status::CODE_401, logRes);
		    }

		    manager::TokenManager tok;
		    auto token = tok.retrieveToken(m_bConf);

		    auto logRes = dto::conversion::DtoConversions::toLoginResultDto(user, token);
		    logRes->message = "Successful";

		    return createDtoResponse(Status::CODE_200, logRes);
		}

		#include OATPP_CODEGEN_END(ApiController)
    private:
    };
}
#endif
