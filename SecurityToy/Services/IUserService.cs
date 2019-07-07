using SecurityToy.Models;
using SecurityToy.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SecurityToy.Services
{
    public interface IUserService
    {
        List<User> GetAllUsers();

        void SaveUser(User user);

        User GetByUserId(string id);

        User GetByEmail(string email);

        User GetByPhone(string phone);

        string AuthUser(User user, LoginViewModel login);
    }
}
