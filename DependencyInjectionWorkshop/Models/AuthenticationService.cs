namespace DependencyInjectionWorkshop.Models
{
    public interface IAuthenticationService
    {
        bool Verify(string accountId, string password, string otp);
    }

    public class LoggerDecorator : AuthenticationDecoratorBase
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly ILogger _logger;
        private readonly IFailedCounter _failedCounter;

        public LoggerDecorator(IAuthenticationService authenticationService, ILogger logger,
            IFailedCounter failedCounter) : base(authenticationService)
        {
            _authenticationService = authenticationService;
            _logger = logger;
            _failedCounter = failedCounter;
        }

        public void Log(string accountId)
        {
            var failedCount = _failedCounter.GetCount(accountId);
            _logger.Log($"accountId:{accountId} failed times:{failedCount}");
        }

        public override bool Verify(string accountId, string password, string otp)
        {
            var result = base.Verify(accountId, password, otp);
            if (result == false)
            {
                Log(accountId);
            }

            return result;
        }
    }

    public class AuthenticationDecoratorBase : IAuthenticationService
    {
        private readonly IAuthenticationService _authenticationService;

        public AuthenticationDecoratorBase(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        public virtual bool Verify(string accountId, string password, string otp)
        {
            var result = _authenticationService.Verify(accountId, password, otp);

            return result;
        }
    }

    public class NotifyDecorator : AuthenticationDecoratorBase
    {
        private readonly INotify _notify;

        public NotifyDecorator(IAuthenticationService authenticationService, INotify notify) : base(
            authenticationService)
        {
            _notify = notify;
        }

        private void Notify(string accountId)
        {
            _notify.Notify($"account:{accountId} try to login failed");
        }

        public override bool Verify(string accountId, string password, string otp)
        {
            var result = base.Verify(accountId, password, otp);
            if (result == false)
            {
                Notify(accountId);
            }

            return result;
        }
    }

    public class AuthenticationService : IAuthenticationService
    {
        public AuthenticationService(IProfile profile, IHash hash, IOtpService otpService,
            IFailedCounter failedCounter)
        {
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
            _profile = new Profile();
            _hash = new Sha256Hash();
            _otpService = new OtpService();
            _failedCounter = new FailedCounter();
        }

        private readonly IProfile _profile;
        private readonly IHash _hash;
        private readonly IOtpService _otpService;
        private readonly IFailedCounter _failedCounter;

        public bool Verify(string accountId, string password, string otp)
        {
            var isLocked = _failedCounter.IsLocked(accountId);
            if (isLocked)
            {
                throw new FailedTooManyTimesException() {AccountId = accountId};
            }

            var passwordFromDb = _profile.GetPassword(accountId);

            var hashedPassword = _hash.Hash(password);

            var currentOtp = _otpService.CurrentOtp(accountId);

            //compare
            if (passwordFromDb == hashedPassword && currentOtp == otp)
            {
                _failedCounter.Reset(accountId);

                return true;
            }
            else
            {
                _failedCounter.Add(accountId);

                return false;
            }
        }
    }
}