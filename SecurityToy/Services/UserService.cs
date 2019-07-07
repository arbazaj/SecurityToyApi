using System;
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

namespace SecurityToy.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        //some config Audience information in the appsettings.json
        private readonly IOptions<Audience> _settings;
        public UserService(
            IUserRepository userRepository,
            IOptions<Audience> settings)
        {
            _userRepository = userRepository;
            _settings = settings;
        }
        [Authorize]
        public List<User> GetAllUsers()
        {

            var users = _userRepository.GetAllUsers();
            return users;
        }

        public User GetByEmail(string email)
        {
            return _userRepository.GetByEmail(email);
        }

        public User GetByPhone(string phone)
        {
            return _userRepository.GetByPhone(phone);
        }

        public User GetByUserId(string id)
        {
            return _userRepository.GetByUserId(id);
        }

        public void SaveUser(User user)
        {
            if (user != null)
            {
                user = SetDefaultProperties(user);
                user.Password = Util.CreateHash(user.Password, Util.CreateSalt());
                _userRepository.SaveUser(user);
            }
            else
            {
                throw new Exception("Invalid payload");
            }

        }

        private User SetDefaultProperties(User user)
        {
            user.Created = DateTime.Now;
            user.IsActive = false;
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

        private string GetJWT(User user)
        {

            // //setting the claims for the user credential name and email
            var claims = new[]
            {
                 new Claim(JwtRegisteredClaimNames.Sub, user.UserId),
                 new Claim(JwtRegisteredClaimNames.Email, user.Email),
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
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
