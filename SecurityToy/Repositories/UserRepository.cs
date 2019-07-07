using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SecurityToy.Models;

namespace SecurityToy.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _dbContext;
        public UserRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public List<User> GetAllUsers()
        {
            return _dbContext.Users.ToList();
        }

        public void SaveUser(User user)
        {
            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();
        }

        public User GetByUserId(string id)
        {
            return _dbContext.Users.FirstOrDefault(u => u.UserId == id);
        }

        public User GetByEmail(string email)
        {
            return _dbContext.Users.FirstOrDefault(u => u.Email == email);
        }

        public User GetByPhone(string phone)
        {
            return _dbContext.Users.FirstOrDefault(u => u.Phone == phone);
        }
    }
}
