using SecurityToy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SecurityToy.Repositories
{
    public interface IVerificationTokenRepository
    {
        void Add(VerificationToken  token);
        VerificationToken GetLatestUserToken(string userId);
        VerificationToken GetTokenByToken(string token);
        void Update(VerificationToken verificationToken);
    }
}
