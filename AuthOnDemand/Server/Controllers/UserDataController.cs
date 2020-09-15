using AuthOnDemand.Shared;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AuthOnDemand.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserDataController : ControllerBase
    {
        [HttpGet]
        public UserData Get()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return new UserData
                {
                     Name = "Unknown",
                     Email = ""
                };
            }

            var nameClaim = User.Claims.First(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name");
            var mailClaim = User.Claims.First(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress");
            
            return new UserData
            {
                Name = nameClaim.Value,
                Email = mailClaim.Value
            };
        }
    }
}
