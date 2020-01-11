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
            ShouldBeValid();
        }


        [Test]
        public void is_invalid()
        {
            GivenPasswordFromDb("marsen", "hashed password");
            GivenOneTimePassword("marsen", "Error OTP");
            GivenHashedPassword("password", "hashed password");
            ShouldBeInvalid();
        }

        private void GivenHashedPassword(string password, string hashedPassword)
        {
            _hash.Hash(password).Returns(hashedPassword);
        }

        private void GivenOneTimePassword(string accountId, string OTP)
        {
            _otpService.CurrentOtp(accountId).Returns(OTP);
        }

        private void GivenPasswordFromDb(string accountId, string password)
        {
            _profile.GetPassword(accountId).Returns(password);
        }

        private void ShouldBeInvalid()
        {
            var authenticationService =
                new AuthenticationService(_notify, _profile, _hash, _otpService, _failedCounter, _logger);
            var result = authenticationService.Verify("marsen", "password", "OTP");
            Assert.IsFalse(result);
        }

        private void ShouldBeValid()
        {
            var authenticationService =
                new AuthenticationService(_notify, _profile, _hash, _otpService, _failedCounter, _logger);
            var result = authenticationService.Verify("marsen", "password", "OTP");
            Assert.IsTrue(result);
        }
    }
}