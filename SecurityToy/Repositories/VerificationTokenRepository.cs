using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SecurityToy.Models;

namespace SecurityToy.Repositories
{
    public class VerificationTokenRepository : IVerificationTokenRepository
    {

        private readonly ApplicationDbContext _dbContext;
        public VerificationTokenRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void Add(VerificationToken token)
        {
            _dbContext.VerificationTokens.Add(token);
            _dbContext.SaveChanges();
        }

        public VerificationToken GetLatestUserToken(string userId)
        {
            return _dbContext.VerificationTokens.OrderByDescending(vt => vt.CreatedOn).FirstOrDefault(vt => vt.UserId == userId);
        }

        public VerificationToken GetTokenByToken(string token)
        {
            return _dbContext.VerificationTokens.FirstOrDefault(vt => vt.Token == token);
        }

        public void Update(VerificationToken verificationToken)
        {
            var oldVerificationToken = _dbContext.VerificationTokens.FirstOrDefault(vt => vt.Token == verificationToken.Token);
            if(oldVerificationToken != null)
            {
                oldVerificationToken.IsActive = verificationToken.IsActive;
            }
            _dbContext.SaveChanges();
        }
    }
}
