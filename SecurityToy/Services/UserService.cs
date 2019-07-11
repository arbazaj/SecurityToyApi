﻿using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using SecurityToy.Models;
using SecurityToy.Repositories;
using SecurityToy.ViewModels;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace SecurityToy.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        //some config Audience information in the appsettings.json
        private readonly IOptions<Audience> _settings;
        private IConfiguration _configuration;
        private IEmailService _emailService;
        private IVerificationTokenRepository _tokenRepository;
        public UserService(
            IUserRepository userRepository,
            IConfiguration configuration,
            IEmailService emailService,
             IVerificationTokenRepository tokenRepository,
            IOptions<Audience> settings
        )
        {
            _userRepository = userRepository;
            _configuration = configuration;
            _emailService = emailService;
            _settings = settings;
            _tokenRepository = tokenRepository;
        }

        public IEnumerable<User> GetAllUsers() => _userRepository.GetAllUsers();

        public User GetByEmail(string email) => _userRepository.GetByEmail(email);

        public User GetByPhone(string phone) => _userRepository.GetByPhone(phone);

        public User GetByUserId(string id) => _userRepository.GetByUserId(id);

        public void SaveUser(User user)
        {
            user = SetDefaultProperties(user);
            user.Password = Util.CreateHash(user.Password, Util.CreateSalt());
            _userRepository.SaveUser(user);
        }

        private User SetDefaultProperties(User user)
        {
            user.Created = DateTime.Now;
            user.IsActive = true;
            user.IsEmailVerified = false;
            user.IsPhoneVerified = false;
            user.IsTwoStepVerificationEnabled = false;
            user.Role = Role.User;
            return user;
        }

        public string AuthUser(User user, LoginViewModel loginModel)
        {
            var cred = user.Password.Split(" ");
            if (cred.Length > 1)
            {
                if (Util.ValidateHash(loginModel.Password, cred[0], cred[1]))
                    return GetJWT(user);
            }
            return null;

        }

        public void SendVerificationEmail(User user)
        {
            string token = Guid.NewGuid().ToString();
            var verificationToken = new VerificationToken()
            {
                IsActive = true,
                UserId = user.UserId,
                Token = token,
                CreatedOn = DateTime.Now,
                ExpiresOn = DateTime.Now.AddMinutes(5)
            };
            _tokenRepository.Add(verificationToken);

            var emailTemplate = new EmailTemplate()
            {
                FromEmail = "noreply@securitytoy.com",
                ToEmail = user.Email,
                Subject = "Verify your SECURITY TOY account",
                HtmlText = $"<strong>Please follow the link to verify your email for Security Toy account <a href='http://localhost:50076/api/users/verify/email?token={token}'>Verify Email</a></strong>"
            };
            _emailService.SendEmail(emailTemplate);
        }


        //change this and add a otp send msms
        public string GetVerificationOtp(User user)
        {


            string[] saAllowedCharacters = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
            string otp = "";
            VerificationToken token = null;
            do
            {
                otp = Util.GenerateRandomOTP(6, saAllowedCharacters);
                token = _tokenRepository.GetLatestUserToken(otp);
            } while (token != null);
            var verificationToken = new VerificationToken()
            {
                IsActive = true,
                UserId = user.UserId,
                Token = otp,
                CreatedOn = DateTime.Now,
                ExpiresOn = DateTime.Now.AddMinutes(5)
            };
            _tokenRepository.Add(verificationToken);
            return otp;
        }


        private string GetJWT(User user)
        {

            //setting the claims for the user credential name and email and role
            var claims = new[]
            {
                 new Claim(JwtRegisteredClaimNames.Sub, user.UserId),
                 new Claim(ClaimTypes.Role, user.Role),
                 new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Value.Secret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                _settings.Value.Iss,
                _settings.Value.Iss,
                claims,
                expires: DateTime.Now.AddMinutes(120),
                signingCredentials: credentials
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public void VerifyEmail(User user, VerificationToken verificationToken)
        {
            user.IsEmailVerified = true;
            verificationToken.IsActive = false;
            _userRepository.UpdateUserAndToken(user, verificationToken);
        }

        public void VerifyPhone(User user, VerificationToken verificationToken)
        {
            user.IsPhoneVerified = true;
            verificationToken.IsActive = false;
            _userRepository.UpdateUserAndToken(user, verificationToken);
        }

        public void UpdateUserRole(string userId, string role) => _userRepository.UpdateUserRole(userId, role);
    }
}
