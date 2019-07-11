using SecurityToy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SecurityToy.Services
{
    public interface IVerificationTokenService
    {
        VerificationToken GetLatestUserToken(string userId);
        void Update(VerificationToken verificationToken);
    }
}
