using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
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
        //[Authorize(Roles = Role.Manager+","+Role.Admin)]
        [Authorize(Roles = Role.Admin)]
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
                user.Password = null;
                title = "Registration successful. You can now login.";
                return Ok(new { status = 201, title, data = new { user } });
            }
            catch (Exception e)
            {
                return BadRequest(new { status = 400, title = "Something went wrong" });
            }
        }

        
        [HttpPost]
        [Route("login")]
        public ActionResult Login([FromBody]LoginViewModel loginViewModel)
        {
            try
            {
                var existingUser = _userService.GetByUserId(loginViewModel.UserId);
                if (existingUser == null || !existingUser.IsActive)
                    return Unauthorized(new { status = 401, title = "User not found" });

                //calling the function for the JWT token for respecting user
                string token = _userService.AuthUser(existingUser, loginViewModel);

                if (token == null)
                    return Unauthorized(new { status = 401, title = "Invalid email or password" });

                existingUser.Password = null;

                if(existingUser.IsTwoStepVerificationEnabled == true)
                {
                    if(string.IsNullOrEmpty(loginViewModel.Otp))
                    {
                        return Ok(new {
                            status = 200,
                            title = "Two-factor login is enabled for you",
                            data = new { user = existingUser }
                        });
                    }

                    var verificationToken = _verificationTokenService.GetLatestUserToken(loginViewModel.UserId);
                    if (verificationToken == null || verificationToken.Token != loginViewModel.Otp || existingUser.UserId != verificationToken.UserId || verificationToken.TokenPurpose != TokenPurpose.TwoFactorLogin)
                        return BadRequest(new { status = 401, title = "Invalid Otp" });

                    if (verificationToken.IsActive != true || verificationToken.ExpiresOn < DateTime.Now)
                        return BadRequest(new { status = 401, title = $"Otp expired. Please create a new one" });
                }
                //returning the token && user
                return Ok(new { status = 200, title = "Login success!", data = new { token, user = existingUser } });
            }
            catch (Exception e)
            {
                return BadRequest(new { status = 401, title = "Something went wrong" });
            }
        }

        [HttpPost]
        [Route("{userId}/login/otp")]
        public ActionResult SendLoginOtp([FromRoute] string userId, [FromBody]TwoFactorOtpViewModel otpViewodel)
        {
            try
            {
                var user = _userService.GetByUserId(userId);
                if(user == null)
                    return BadRequest(new { status = 400, title = "User not found" });
                if(user.IsTwoStepVerificationEnabled != true)
                    return BadRequest(new { status = 400, title = "Two-factor is not enabled for you" });
                if(!string.IsNullOrEmpty(otpViewodel.Email) && user.IsEmailVerified)
                {
                    _userService.SendLoginOtpEmail(user);
                    return Ok(new { status = 200, title = "Otp sent successfully"});
                }
                if(!string.IsNullOrEmpty(otpViewodel.Phone) && user.IsPhoneVerified)
                {
                    _userService.SendLoginOtpPhone(user);
                    return Ok(new { status = 200, title = "Otp sent successfully" });
                }
                
                return BadRequest(new { status = 400, title = "Verify your email or phone number." });

            }
            catch (Exception e)
            {
                return BadRequest(new { status = 400, title = "Something went wrong" });
            }
        }

        [HttpPost]
        [Route("{userId}/verify/email")]
        public ActionResult SendVerificationEmail([FromRoute] string userId, [FromBody]EmailVerificationViewModel emailViewModel)
        {
            try
            {
                var user = _userService.GetByEmail(emailViewModel.Email);
                if (user == null || !user.IsActive)
                    return BadRequest(new { status = 400, title = $"User with email - {emailViewModel.Email} does not exists" });
                if (user.UserId != userId)
                    return BadRequest(new { status = 400, title = "Email does not belongs to you" });

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

                if (verificationToken == null || verificationToken.Token != otpViewModel.Otp)
                    return BadRequest(new { status = 400, title = "Invalid Otp" });

                if (verificationToken.IsActive != true || verificationToken.ExpiresOn < DateTime.Now)
                    return BadRequest(new { status = 400, title = $"Otp expired. Please create a new one" });

                var user = _userService.GetByUserId(verificationToken.UserId);
                if (user == null || !user.IsActive)
                    return BadRequest(new { status = 400, title = $"User does not exists" });

                if (user.UserId != verificationToken.UserId || verificationToken.TokenPurpose != TokenPurpose.EmailVerification)
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
        [Route("{userId}/verify/phone")]
        public ActionResult SendPhoneVerificationOtp([FromRoute] string userId, [FromBody]PhoneVerificationViewModel phoneViewModel)
        {
            try
            {
                var user = _userService.GetByPhone(phoneViewModel.Phone);
                if (user == null || !user.IsActive)
                    return BadRequest(new { status = 400, title = $"User with phone number - {phoneViewModel.Phone} does not exists" });

                if (user.UserId != userId)
                    return BadRequest(new { status = 400, title = "Phone number does not belongs to you" });

                if (user.IsPhoneVerified)
                    return BadRequest(new { status = 400, title = $"Phone number - {phoneViewModel.Phone} already verified" });

                _userService.SendPhoneVerificationSms(user);
                return Ok(new { status = 200, title = "Sms sent successfully." });
            }
            catch (Exception e)
            {
                return BadRequest(new { status = 400, title = "Something went wrong" });
            }
        }

        [HttpPut]
        [Route("{userId}/verify/phone")]
        public ActionResult VerifyPhone([FromRoute] string userId, [FromBody]OtpViewModel otpViewModel)
        {
            try
            {
                var verificationToken = _verificationTokenService.GetLatestUserToken(userId);

                if (verificationToken == null || verificationToken.Token != otpViewModel.Otp)
                    return BadRequest(new { status = 400, title = "Invalid Otp" });

                if (verificationToken.IsActive != true || verificationToken.ExpiresOn < DateTime.Now)
                    return BadRequest(new { status = 400, title = $"Otp expired. Please create a new one" });

                var user = _userService.GetByUserId(verificationToken.UserId);
                if (user == null || !user.IsActive)
                    return BadRequest(new { status = 400, title = $"User does not exists" });

                if (user.UserId != verificationToken.UserId || verificationToken.TokenPurpose != TokenPurpose.PhoneVerification)
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
        [Route("{userId}/roles/{role}")]
        public ActionResult ChangeUserRole([FromRoute] string userId, [FromRoute] string role)
        {
            try
            {
                var selectedRole = Role.GetRoleByRole(role);
                if (selectedRole == null)
                    return BadRequest(new { status = 400, title = "Invalid Role" });

                _userService.UpdateUserRole(userId, selectedRole);
                return Ok(new { status = 200, title = "Role changed successfully." });
            }
            catch (Exception e)
            {
                return BadRequest(new { status = 400, title = "Something went wrong." });
            }
        }

        [HttpPut]
        [Authorize]
        [Route("{userId}/enabletwofactorlogin")]
        public ActionResult EnableTwoFactorLogin([FromRoute] string userId)
        {
            try
            {
                string token = Request.Headers["Authorization"];
                
                //Remove "Bearer " fron Authorization header
                token = token.Substring(7);
                //decode jwt
                var decodedJwt = new JwtSecurityToken(jwtEncodedString: token);

                //get userId from claims. we set sub claim with userId while creating jwt
                var userIdInToken = decodedJwt.Claims.First(c => c.Type == "sub").Value;

                var user = _userService.GetByUserId(userId);
                if (user == null)
                    return BadRequest(new { status = 400, title = "User does not exists." });
                if(user.UserId != userIdInToken)
                    return BadRequest(new { status = 403, title = "You have insufficient permissions." });
                if(user.IsEmailVerified != true && user.IsPhoneVerified != true)
                    return BadRequest(new { status = 400, title = "Email address or phone number must be verified for enabling two factor authentication" });

                _userService.UpdateTwoFactorLogin(userIdInToken, true);
                return Ok(new { status = 200, title = "Two-factor Login is enabled now." });
            }
            catch (Exception e)
            {
                return BadRequest(new { status = 400, title = "Something went wrong." });
            }
        }

        [HttpGet]
        [Authorize]
        [Route("{userId}")]
        public ActionResult<User> GetByUserId([FromRoute] string userId)
        {
            try
            {   var user = _userService.GetByUserId(userId);
                user.Password = null;
                return Ok(new { status = 200, title = "Request successful.", data = new { user } });
            }
            catch (Exception)
            {
                return BadRequest(new { status = 400, title = "Something went wrong" });
            }
        }

        [HttpPost]
        [Route("forgotpassword/otp")]
        public ActionResult SendForgotPasswordOtp([FromBody]ForgotPasswordViewModel forgotPasswordViewModel)
        {
            try
            {
               
                if (!string.IsNullOrEmpty(forgotPasswordViewModel.Email))
                {
                    var user = _userService.GetByEmail(forgotPasswordViewModel.Email);
                    if (user == null || user.IsActive != true)
                        return BadRequest(new { status = 400, title = "No user account exists with this email"});

                    _userService.SendForgotPasswordOtpEmail(user);
                    return Ok(new { status = 200, title = "Otp sent successfully", data = new { userId = user.UserId } });
                }
                if (!string.IsNullOrEmpty(forgotPasswordViewModel.Phone))
                {
                    var user = _userService.GetByPhone(forgotPasswordViewModel.Phone);
                    if (user == null || user.IsActive != true)
                        return BadRequest(new { status = 400, title = "No user account exists with this phone number" });

                    _userService.SendForgotPasswordOtpPhone(user);
                    return Ok(new { status = 200, title = "Otp sent successfully", data = new { userId = user.UserId } });
                }

                return BadRequest(new { status = 400, title = "Your contact is not verified" });

            }
            catch (Exception)
            {
                return BadRequest(new { status = 400, title = "Something went wrong" });
            }
        }

        [HttpPut]
        [Route("{userId}/resetpassword")]
        public ActionResult ResetPassword([FromRoute] string userId, [FromBody]ResetPasswordViewModel resetPasswordViewModel)
        {
            try
            {
                var user = _userService.GetByUserId(userId);
                if (user == null || user.IsActive != true)
                    return BadRequest(new { status = 400, title = "User not found" });

                var verificationToken = _verificationTokenService.GetLatestUserToken(userId);

                if (verificationToken == null || verificationToken.Token != resetPasswordViewModel.Otp)
                    return BadRequest(new { status = 400, title = "Invalid Otp" });

                if (verificationToken.IsActive != true || verificationToken.ExpiresOn < DateTime.Now)
                    return BadRequest(new { status = 400, title = $"Otp expired. Please create a new one" });


                if (user.UserId != verificationToken.UserId || verificationToken.TokenPurpose != TokenPurpose.ForgotPassword)
                    return BadRequest(new { status = 400, title = "Invalid Otp" });

                _userService.ResetPassword(user, resetPasswordViewModel.Password, verificationToken);

                return Ok(new { status = 200, title = "Password reset successful." });
            }
            catch
            {
                return BadRequest(new { status = 400, title = "Something went wrong" });
            }

        }

    }

}