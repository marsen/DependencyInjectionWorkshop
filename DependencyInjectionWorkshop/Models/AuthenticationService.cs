using System;
using System.Net.Http;
using DependencyInjectionWorkshop.Models.Decorator;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService : IAuthentication
    {
        private readonly IProfile _profile;
        private readonly IHash _hash;
        private readonly IFailedCounter _failedCounter;
        private readonly IOtpService _otpService;
        private readonly ILogger _logger;

        public AuthenticationService(IProfile profile, IHash hash,
            IFailedCounter failedCounter, IOtpService otpService, ILogger logger)
        {
            //_loggerDecorator = new LoggerDecorator(this);
            _profile = profile;
            _hash = hash;
            _failedCounter = failedCounter;
            _otpService = otpService;
            _logger = logger;
        }

        public AuthenticationService()
        {
            //_loggerDecorator = new LoggerDecorator(this);
            _profile = new ProfileDao();
            _hash = new Sha256Adapter();
            _failedCounter = new FailedCounter();
            _otpService = new OtpService();
            _logger = new NLoggerAdapter();
        }

        public IFailedCounter FailedCounter
        {
            get { return _failedCounter; }
        }

        /// <summary>
        /// Verifies the specified account identifier.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="password">The password.</param>
        /// <param name="otp">The otp.</param>
        /// <returns></returns>
        /// <exception cref="DependencyInjectionWorkshop.Models.FailedTooManyTimesException"></exception>
        public bool Verify(string accountId, string password, string otp)
        {
            var passwordFromDb = _profile.Password(accountId);

            var hashedPassword = _hash.ComputeHash(password);

            var currentOtp = _otpService.GetOtp(accountId);

            if (currentOtp == otp && hashedPassword == passwordFromDb)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private FailedCounterDecorator a;
        private readonly LoggerDecorator _loggerDecorator;
    }
}