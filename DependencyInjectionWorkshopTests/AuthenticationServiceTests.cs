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
        private string DefaultTestAccount = "marsen";

        [SetUp]
        public void SetUp()
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
            GivenPasswordFromDb(DefaultTestAccount, "hashed password");
            GivenOneTimePassword(DefaultTestAccount, "OTP");
            GivenHashedPassword("password", "hashed password");
            ShouldBeValid();
        }


        [Test]
        public void is_invalid()
        {
            GivenPasswordFromDb(DefaultTestAccount, "hashed password");
            GivenOneTimePassword(DefaultTestAccount, "Error OTP");
            GivenHashedPassword("password", "hashed password");
            ShouldBeInvalid();
        }

        [Test]
        public void is_invalid_should_add_failed_count()
        {
            GivenPasswordFromDb(DefaultTestAccount, "hashed password");
            GivenOneTimePassword(DefaultTestAccount, "Error OTP");
            GivenHashedPassword("password", "hashed password");
            ShouldAddFailedCount();
        }

        [Test]
        public void is_invalid_should_notify()
        {
            GivenPasswordFromDb(DefaultTestAccount, "hashed password");
            GivenOneTimePassword(DefaultTestAccount, "Error OTP");
            GivenHashedPassword("password", "hashed password");
            var authenticationService =
                new AuthenticationService(_notify, _profile, _hash, _otpService, _failedCounter, _logger);
            var result = authenticationService.Verify(DefaultTestAccount, "password", "OTP");
            _notify.Received(1).Notify(Arg.Is<string>(s=>s.Contains(DefaultTestAccount)));
        }


        private void ShouldAddFailedCount()
        {
            var authenticationService =
                new AuthenticationService(_notify, _profile, _hash, _otpService, _failedCounter, _logger);
            var result = authenticationService.Verify(DefaultTestAccount, "password", "OTP");
            _failedCounter.Received(1).Add(DefaultTestAccount);
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
            var result = authenticationService.Verify(DefaultTestAccount, "password", "OTP");
            Assert.IsFalse(result);
        }

        private void ShouldBeValid()
        {
            var authenticationService =
                new AuthenticationService(_notify, _profile, _hash, _otpService, _failedCounter, _logger);
            var result = authenticationService.Verify(DefaultTestAccount, "password", "OTP");
            Assert.IsTrue(result);
        }
    }
}