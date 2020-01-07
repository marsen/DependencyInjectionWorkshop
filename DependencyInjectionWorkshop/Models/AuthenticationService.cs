using System;
using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        public AuthenticationService(INotify slackNotify, IProfile profile, IHash hash, IOtpService otpService, IFailedCounter failedCounter)
        {
            _slackNotify = slackNotify;
            _profile = profile;
            _hash = hash;
            _otpService = otpService;
            _failedCounter = failedCounter;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationService" /> class.
        /// </summary>
        public AuthenticationService()
        {
            _slackNotify = new SlackNotify();
            _profile = new Profile();
            _hash = new Sha256Hash();
            _otpService = new OtpService();
            _failedCounter = new FailedCounter();
        }

        private readonly INotify _slackNotify;
        private readonly IProfile _profile;
        private readonly IHash _hash;
        private readonly IOtpService _otpService;
        private readonly IFailedCounter _failedCounter;

        public bool Verify(string accountId, string password, string otp)
        {
            var httpClient = new HttpClient() { BaseAddress = new Uri("http://joey.com/") };

            _failedCounter.CheckIsLocked(accountId, httpClient);

            var passwordFromDb = _profile.GetPassword(accountId);

            var hashedPassword = _hash.HashPassword(password);

            var currentOtp = _otpService.CurrentOtp(accountId, httpClient);

            //compare
            if (passwordFromDb == hashedPassword && currentOtp == otp)
            {
                _failedCounter.ResetFailedCount(accountId, httpClient);

                return true;
            }
            else
            {
                _failedCounter.AddFailedCount(accountId, httpClient);

                _failedCounter.LogFailedCount(accountId, httpClient);

                _slackNotify.Notify(accountId);

                return false;
            }
        }
    }
}