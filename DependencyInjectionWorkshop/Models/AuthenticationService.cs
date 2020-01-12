namespace DependencyInjectionWorkshop.Models
{
    public class FailedCountDecorator : AuthenticationDecoratorBase
    {
        private readonly IFailedCounter _failedCounter;

        public FailedCountDecorator(IAuthenticationService authenticationService, IFailedCounter failedCounter) : base(
            authenticationService)
        {
            _failedCounter = failedCounter;
        }

        public override bool Verify(string accountId, string password, string otp)
        {
            var result = base.Verify(accountId, password, otp);
            if (result == false)
            {
                Add(accountId);
            }

            return result;
        }

        public void Add(string accountId)
        {
            _failedCounter.Add(accountId);
        }
    }

    public class AuthenticationService : IAuthenticationService
    {
        public AuthenticationService(IProfile profile, IHash hash, IOtpService otpService,
            IFailedCounter failedCounter)
        {
            //_failedCountDecorator = new FailedCountDecorator(this);
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
            //_failedCountDecorator = new FailedCountDecorator(this);
            _profile = new Profile();
            _hash = new Sha256Hash();
            _otpService = new OtpService();
            _failedCounter = new FailedCounter();
        }

        private readonly IProfile _profile;
        private readonly IHash _hash;
        private readonly IOtpService _otpService;
        private readonly IFailedCounter _failedCounter;
        private readonly FailedCountDecorator _failedCountDecorator;

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
                //_failedCountDecorator.Add(accountId);

                return false;
            }
        }
    }
}