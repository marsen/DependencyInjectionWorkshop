using DependencyInjectionWorkshop.Models;
using NSubstitute;
using NSubstitute.Core;
using NUnit.Framework;

namespace DependencyInjectionWorkshopTests
{
    [TestFixture]
    public class AuthenticationServiceTests
    {
        private ILogger _logger;
        private IFailedCounter _failedCounter;
        private IOtpService _otpService;
        private IHash _hash;
        private INotify _notify;
        private IProfile _profile;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationServiceTests" /> class.
        /// </summary>
        public AuthenticationServiceTests()
        {
            _logger = Substitute.For<ILogger>();
            _failedCounter = Substitute.For<IFailedCounter>();
            _otpService = Substitute.For<IOtpService>();
            _hash = Substitute.For<IHash>();
            _notify = Substitute.For<INotify>();
            _profile = Substitute.For<IProfile>();
        }

        [Test]
        public void is_valid()
        {
            _profile.GetPassword("marsen").Returns("hashed password");
            _otpService.CurrentOtp("marsen").Returns("OTP");
            _hash.Hash("password").Returns("hashed password");
            var authenticationService =
                new AuthenticationService(_notify, _profile, _hash, _otpService, _failedCounter, _logger);
            var result = authenticationService.Verify("marsen", "password", "OTP");
            Assert.IsTrue(result);
        }

        [Test]
        public void is_invalid()
        {
            _profile.GetPassword("marsen").Returns("hashed password");
            _otpService.CurrentOtp("marsen").Returns("Error OTP");
            _hash.Hash("password").Returns("hashed password");
            var authenticationService =
                new AuthenticationService(_notify, _profile, _hash, _otpService, _failedCounter, _logger);
            var result = authenticationService.Verify("marsen", "password", "OTP");
            Assert.IsFalse(result);
        }
    }
}