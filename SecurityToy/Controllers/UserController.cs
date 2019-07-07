using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SecurityToy.Models;
using SecurityToy.Services;
using SecurityToy.ViewModels;

namespace SecurityToy.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [Authorize(Roles = Role.User)]
        public ActionResult<IEnumerable<User>> Get()
        {
            return _userService.GetAllUsers();
        }

        [HttpPost]
        [Route("save")]
        public ActionResult Save(User user)
        {
            try
            {
                if(user == null)
                    return BadRequest(new { status = 400, title = "Invalid Payload" });

                var existingUser = _userService.GetByUserId(user.UserId);
                if(existingUser != null)
                    return BadRequest(new { status = 400, title = "User with userId - "+ user.UserId+ " already exists." });

                existingUser = _userService.GetByEmail(user.Email);
                if (existingUser != null)
                    return BadRequest(new { status = 400, title = "User with email - " + user.Email + " already exists." });

                existingUser = _userService.GetByPhone(user.Phone);
                if (existingUser != null)
                    return BadRequest(new { status = 400, title = "User with phone number - " + user.Phone + " already exists." });

                _userService.SaveUser(user);
                user.Password = null;
                return Ok(new { status = 200, title = "User saved successfully", data = user });
            }
            catch (Exception e)
            {
                return BadRequest(new { status = 400, title = "Something went wrong"});
            }
        }

        //this method will be called by the angular app with the user object for getting the JWT token
        [HttpPost]
        [Route("login")]
        public ActionResult Auth([FromBody]LoginViewModel user)
        {
            try
            {
                var existingUser = _userService.GetByEmail(user.Email);
                if (existingUser == null)
                    return Unauthorized(new  { status = 401, title = "User not found" });


                //calling the function for the JWT token for respecting user
                string token = _userService.AuthUser(existingUser, user);

                if (token == null)
                    return Unauthorized(new { status = 401, title = "Invalid email or password" });
                //returning the token
                return Ok(token);
            } 
            catch(Exception e)
            {
                return BadRequest(new { status = 401, title = "Something went wrong" });
            }
        }

    }
}