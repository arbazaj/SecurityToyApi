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
        public IEnumerable<User> GetAllUsers() => _dbContext.Users.ToList();

        public void SaveUser(User user)
        {
            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();
        }

        public User GetByUserId(string id) => _dbContext.Users.FirstOrDefault(u => u.UserId == id);

        public User GetByEmail(string email) => _dbContext.Users.FirstOrDefault(u => u.Email == email);

        public User GetByPhone(string phone) => _dbContext.Users.FirstOrDefault(u => u.Phone == phone);

        public void UpdateUserRole(string userId, string role)
        {
            var oldUser = _dbContext.Users.FirstOrDefault(u => u.UserId == userId);
            if (oldUser != null)
            {
                oldUser.Role = role;
                _dbContext.SaveChanges();
            }
        }

        public void UpdateUserAndToken(User user, VerificationToken verificationToken)
        {
            using (var transaction = _dbContext.Database.BeginTransaction())
            {
                try
                {
                    var oldUser = _dbContext.Users.FirstOrDefault(u => u.UserId == user.UserId);
                    if (oldUser != null)
                    {
                        oldUser.IsEmailVerified = user.IsEmailVerified;
                        oldUser.IsPhoneVerified = user.IsPhoneVerified;
                    }
                    _dbContext.SaveChanges();

                    var oldVerificationToken = _dbContext.VerificationTokens.FirstOrDefault(vt => vt.Token == verificationToken.Token);
                    if (oldVerificationToken != null)
                    {
                        oldVerificationToken.IsActive = false;
                    }
                    _dbContext.SaveChanges();
                    // Commit transaction if all commands succeed, transaction will auto-rollback
                    // when disposed if either commands fails
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw e;

                }
            }
        }
    }
}
