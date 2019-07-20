using SecurityToy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SecurityToy.Repositories
{
    public interface IUserRepository
    {
        IEnumerable<User> GetAllUsers();

        void SaveUser(User user);

        User GetByUserId(string id);

        User GetByEmail(string email);

        User GetByPhone(string phone);

        void UpdateUserRole(string userId, string role);

        void UpdateUserAndToken(User user, VerificationToken verificationToken);

        void UpdateTwoFactorLogin(string userId, bool twoFactorLogin);

        void UpdatePassword(User user, VerificationToken verificationToken);
    }
}
