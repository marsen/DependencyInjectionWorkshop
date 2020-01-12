namespace DependencyInjectionWorkshop.Models
{
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

        public IFailedCounter FailedCounter
        {
            get { return _failedCounter; }
        }

        //public IFailedCounter FailedCounter
        //{
        //    get { return _failedCounter; }
        //}

        public bool Verify(string accountId, string password, string otp)
        {
            //failedCountDecorator.CheckLocked(accountId, this);

            var passwordFromDb = _profile.GetPassword(accountId);

            var hashedPassword = _hash.Hash(password);

            var currentOtp = _otpService.CurrentOtp(accountId);

            //compare
            if (passwordFromDb == hashedPassword && currentOtp == otp)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private FailedCountDecorator failedCountDecorator;
    }
}