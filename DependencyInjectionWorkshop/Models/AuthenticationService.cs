using System;
using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        public AuthenticationService(SlackNotify slackNotify, Profile profile, Sha256Hash sha256Hash, OtpService otpService, FailedCounter failedCounter)
        {
            _slackNotify = slackNotify;
            _profile = profile;
            _sha256Hash = sha256Hash;
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
        _sha256Hash = new Sha256Hash();
        _otpService = new OtpService();
        _failedCounter = new FailedCounter();

        }

        private readonly SlackNotify _slackNotify;
        private readonly Profile _profile;
        private readonly Sha256Hash _sha256Hash;
        private readonly OtpService _otpService;
        private readonly FailedCounter _failedCounter;

        public bool Verify(string accountId, string password, string otp)
        {
            var httpClient = new HttpClient() { BaseAddress = new Uri("http://joey.com/") };

            _failedCounter.CheckIsLocked(accountId, httpClient);

            var passwordFromDb = _profile.GetPassword(accountId);

            var hashedPassword = _sha256Hash.HashPassword(password);

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