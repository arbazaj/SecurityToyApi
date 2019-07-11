using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
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
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IVerificationTokenService _verificationTokenService;
        private readonly ISmsService _smsService;
        public UsersController(
            IUserService userService,
            IVerificationTokenService verificationTokenService,
            ISmsService smsService
            )
        {
            _userService = userService;
            _verificationTokenService = verificationTokenService;
            _smsService = smsService;
        }

        [HttpGet]
        //[Authorize(Roles = Role.User+","+Role.Admin)]
        [Authorize(Roles = Role.Manager)]
        public ActionResult<IEnumerable<User>> Get()
        {
            try
            {
                return Ok(new { status = 200, title = "Request successful.", data = new { users = _userService.GetAllUsers() } });
            }
            catch(Exception)
            {
                return BadRequest(new { status = 400, title = "Something went wrong" });
            }
        }

        [HttpPost]
        [Route("save")]
        public ActionResult Save(User user)
        {
            try
            {
                var existingUser = _userService.GetByUserId(user.UserId);
                if (existingUser != null)
                    return BadRequest(new { status = 400, title = "User with userId - " + user.UserId + " already exists." });

                if (user.Email != null)
                {
                    existingUser = _userService.GetByEmail(user.Email);
                    if (existingUser != null)
                        return BadRequest(new { status = 400, title = "User with email - " + user.Email + " already exists." });
                }
                if (user.Phone != null)
                {
                    existingUser = _userService.GetByPhone(user.Phone);
                    if (existingUser != null)
                        return BadRequest(new { status = 400, title = "User with phone number - " + user.Phone + " already exists." });
                }


                _userService.SaveUser(user);
                var title = "";
                if (user.Email != null)
                {
                    _userService.SendVerificationEmail(user);
                    title = "Registration successful. A verification email has been sent to you. Please follow the link in the email.";
                    return Ok(new { status = 200, title, data = new { user } });
                }
                user.Password = null;
                title = "Registration successful";
                return Ok(new { status = 200, title, data = new { user } });
            }
            catch (Exception e)
            {
                return BadRequest(new { status = 400, title = "Something went wrong" });
            }
        }

        //this method will be called by the angular app with the user object for getting the JWT token
        [HttpPost]
        [Route("login")]
        public ActionResult Auth([FromBody]LoginViewModel user)
        {
            try
            {
                var existingUser = _userService.GetByUserId(user.UserId);
                if (existingUser == null || !existingUser.IsActive)
                    return Unauthorized(new { status = 401, title = "User not found" });

                //if (!existingUser.IsEmailVerified)
                //    return Unauthorized(new { status = 401, title = "Email is not verified" });

                //calling the function for the JWT token for respecting user
                string token = _userService.AuthUser(existingUser, user);

                if (token == null)
                    return Unauthorized(new { status = 401, title = "Invalid email or password" });
                //returning the token
                return Ok(new { status = 200, title = "Login success!", data = new { token } });
            }
            catch (Exception e)
            {
                return BadRequest(new { status = 401, title = "Something went wrong" });
            }
        }

        [HttpPost]
        [Route("verify/email")]
        public ActionResult SendVerificationEmail([FromBody]EmailVerificationViewModel emailViewModel)
        {
            try
            {
                var user = _userService.GetByEmail(emailViewModel.Email);
                if (user == null || !user.IsActive)
                    return BadRequest(new { status = 400, title = $"User with email - {emailViewModel.Email} does not exists" });

                if (user.IsEmailVerified)
                    return BadRequest(new { status = 400, title = $"Email address - {emailViewModel.Email} already verified" });

                _userService.SendVerificationEmail(user);
                return Ok(new { status = 200, title = "Verification email sent successfully. Please follow the instructions in the email." });
            }
            catch (Exception e)
            {
                return BadRequest(new { status = 400, title = "Something went wrong" });
            }
        }

        [HttpPut]
        [Route("{userId}/verify/email")]
        public ActionResult VerifyEmail([FromRoute] string userId, [FromBody]OtpViewModel otpViewModel)
        {
            try
            {
                var verificationToken = _verificationTokenService.GetLatestUserToken(userId);

                if (verificationToken == null)
                    return BadRequest(new { status = 400, title = "Invalid Otp" });

                if (verificationToken.IsActive != true || verificationToken.ExpiresOn < DateTime.Now)
                    return BadRequest(new { status = 400, title = $"Otp expired. Please create a new one" });

                var user = _userService.GetByUserId(verificationToken.UserId);
                if (user == null || !user.IsActive)
                    return BadRequest(new { status = 400, title = $"User does not exists" });

                if (user.UserId != verificationToken.UserId)
                    return BadRequest(new { status = 400, title = "Invalid Otp" });

                if (user.IsEmailVerified)
                    return BadRequest(new { status = 400, title = $"Email address - {user.Email} already verified" });

                _userService.VerifyEmail(user, verificationToken);
                return Ok(new { status = 200, title = "Email verified successfully." });
            }
            catch (Exception e)
            {
                return BadRequest(new { status = 400, title = "Something went wrong" });
            }
        }

        [HttpPost]
        [Route("verify/phone")]
        public async Task<ActionResult> SendPhoneOtp([FromBody]PhoneVerificationViewModel phoneViewModel)
        {
            try
            {
                var user = _userService.GetByPhone(phoneViewModel.Phone);
                if (user == null || !user.IsActive)
                    return BadRequest(new { status = 400, title = $"User with phone number - {phoneViewModel.Phone} does not exists" });

                if (user.IsPhoneVerified)
                    return BadRequest(new { status = 400, title = $"Phone number - {phoneViewModel.Phone} already verified" });

                var otp = _userService.GetVerificationOtp(user);
                var smsResponse = await _smsService.SendSmsAsync("+" + phoneViewModel.Phone, otp);
                return Ok(new { data = smsResponse });
            }
            catch (Exception e)
            {
                return BadRequest(new { status = 400, title = "Something went wrong" });
            }
        }

        [HttpPut]
        [Route("{userId}/verify/phone")]
        public ActionResult VerifyPhoneVerificationOtp([FromRoute] string userId, [FromBody]OtpViewModel otpViewModel)
        {
            try
            {
                var verificationToken = _verificationTokenService.GetLatestUserToken(userId);

                if (verificationToken == null)
                    return BadRequest(new { status = 400, title = "Invalid Otp" });

                if (verificationToken.IsActive != true || verificationToken.ExpiresOn < DateTime.Now)
                    return BadRequest(new { status = 400, title = $"Otp expired. Please create a new one" });

                var user = _userService.GetByUserId(verificationToken.UserId);
                if (user == null || !user.IsActive)
                    return BadRequest(new { status = 400, title = $"User does not exists" });

                if (user.UserId != verificationToken.UserId)
                    return BadRequest(new { status = 400, title = "Invalid Otp" });

                if (user.IsPhoneVerified)
                    return BadRequest(new { status = 400, title = $"Phone number - {user.Phone} already verified" });

                _userService.VerifyPhone(user, verificationToken);
                return Ok(new { status = 200, title = "Phone number verified successfully." });
            }
            catch (Exception e)
            {
                return BadRequest(new { status = 400, title = "Something went wrong" });
            }
        }

        [HttpPut]
        [Authorize(Roles = Role.Admin)]
        [Route("{userId}/role/{role}")]
        public ActionResult ChangeUserRole([FromRoute] string userId, [FromRoute] string role)
        {
            try
            {
                var selectedRole = Role.GetRoleByRole(role);
                if (selectedRole == null)
                    return BadRequest(new { status = 400, title = "Invalid Role" });

                _userService.UpdateUserRole(userId, selectedRole);
                return Ok(new { status = 200, message = "Role changed successfully." });
            }
            catch (Exception e)
            {
                return BadRequest(new { status = 400, title = "Something went wrong." });
            }
        }
    }

}