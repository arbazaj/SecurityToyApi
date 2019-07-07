using SecurityToy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SecurityToy.Repositories
{
    public interface IUserRepository
    {
        List<User> GetAllUsers();

        void SaveUser(User user);

        User GetByUserId(string id);

        User GetByEmail(string email);

        User GetByPhone(string phone);
    }
}
