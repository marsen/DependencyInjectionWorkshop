using System;
using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        public AuthenticationService(INotify notify, IProfile profile, IHash hash, IOtpService otpService, FailedCounter failedCounter)
        {
            _nLogLogger = new NLogLogger();
            _notify = notify;
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
            _nLogLogger = new NLogLogger();
            _notify = new SlackNotify();
            _profile = new Profile();
            _hash = new Sha256Hash();
            _otpService = new OtpService();
            _failedCounter = new FailedCounter();
        }

        private readonly INotify _notify;
        private readonly IProfile _profile;
        private readonly IHash _hash;
        private readonly IOtpService _otpService;
        private readonly FailedCounter _failedCounter;
        private readonly NLogLogger _nLogLogger;

        public bool Verify(string accountId, string password, string otp)
        {
            var httpClient = new HttpClient() { BaseAddress = new Uri("http://joey.com/") };

            _failedCounter.IsLocked(accountId, httpClient);

            var passwordFromDb = _profile.GetPassword(accountId);

            var hashedPassword = _hash.Hash(password);

            var currentOtp = _otpService.CurrentOtp(accountId, httpClient);

            //compare
            if (passwordFromDb == hashedPassword && currentOtp == otp)
            {
                _failedCounter.Reset(accountId, httpClient);

                return true;
            }
            else
            {
                _failedCounter.Add(accountId, httpClient);

                var failedCount = _failedCounter.GetCount(accountId, httpClient);
                _nLogLogger.Log($"accountId:{accountId} failed times:{failedCount}");

                _notify.Notify(accountId);

                return false;
            }
        }
    }
}