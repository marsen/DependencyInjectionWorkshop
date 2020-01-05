using System;
using System.Net.Http;
using DependencyInjectionWorkshop.Models.Decorator;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService : IAuthentication
    {
        private readonly IProfile _profile;
        private readonly IHash _hash;
        private readonly IOtpService _otpService;

        public AuthenticationService(IProfile profile, IHash hash, IOtpService otpService)
        {
            _profile = profile;
            _hash = hash;
            _otpService = otpService;
        }

        public AuthenticationService()
        {
            _profile = new ProfileDao();
            _hash = new Sha256Adapter();
            _otpService = new OtpService();
        }


        /// <summary>
        /// Verifies the specified account identifier.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="password">The password.</param>
        /// <param name="otp">The otp.</param>
        /// <returns></returns>
        /// <exception cref="FailedTooManyTimesException"></exception>
        public bool Verify(string accountId, string password, string otp)
        {
            if (CheckOtp(otp, accountId) && CheckPassword(accountId, password))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool CheckPassword(string accountId, string password)
        {
            var passwordFromDb = _profile.Password(accountId);
            var hashedPassword = _hash.ComputeHash(password);
            return hashedPassword == passwordFromDb;
        }

        private bool CheckOtp(string otp, string accountId)
        {
            var currentOtp = _otpService.GetOtp(accountId);
            return currentOtp == otp;
        }
    }
}