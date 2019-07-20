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
        IEnumerable<User> GetAllUsers();

        void SaveUser(User user);

        User GetByUserId(string id);

        User GetByEmail(string email);

        User GetByPhone(string phone);

        string AuthUser(User user, LoginViewModel login);

        void UpdateUserRole(string userId, string role);

        void SendVerificationEmail(User user);

        void SendLoginOtpEmail(User user);

        void SendForgotPasswordOtpEmail(User user);

        void SendForgotPasswordOtpPhone(User user);

        void SendPhoneVerificationSms(User user);

        void SendLoginOtpPhone(User user);

        void VerifyEmail(User user, VerificationToken verificationToken);

        void VerifyPhone(User user, VerificationToken verificationToken);

        void UpdateTwoFactorLogin(string userId, bool twoFactorLogin);

        void ResetPassword(User user, string newPassword, VerificationToken verificationToken);
    }
}
