using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SecurityToy.Models;
using SecurityToy.Repositories;

namespace SecurityToy.Services
{
    public class VerificationTokenService : IVerificationTokenService
    {
        IVerificationTokenRepository _tokenRepository;
        public VerificationTokenService(IVerificationTokenRepository tokenRepository)
        {
            _tokenRepository = tokenRepository;
        }
        public VerificationToken GetLatestUserToken(string token)
        {
            return _tokenRepository.GetLatestUserToken(token);
        }

        public void Update(VerificationToken verificationToken)
        {
            _tokenRepository.Update(verificationToken);
        }
    }
}
