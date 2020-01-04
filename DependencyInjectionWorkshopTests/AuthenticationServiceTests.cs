using DependencyInjectionWorkshop.Models;
using NSubstitute;
using NUnit.Framework;

namespace DependencyInjectionWorkshopTests
{
    [TestFixture]
    public class AuthenticationServiceTests
    {
        private IProfile _profile;
        private IHash _hash;
        private IOtpService _otpService;
        private ILogger _logger;
        private INotification _notification;
        private IFailedCounter _failedCounter;
        private AuthenticationService _authenticationService;

        [SetUp]
        public void Setup()
        {
            _profile = Substitute.For<IProfile>();
            _hash = Substitute.For<IHash>();
            _otpService = Substitute.For<IOtpService>();
            _logger = Substitute.For<ILogger>();
            _notification = Substitute.For<INotification>();
            _failedCounter = Substitute.For<IFailedCounter>();
            _authenticationService = new AuthenticationService(
                _profile, _hash, _notification, _failedCounter, _otpService, _logger);
        }

        [Test]
        public void is_valid()
        {
            GivenPasswordFromDB("joey", "my hashed password");
            GivenHashedPassword("1234", "my hashed password");
            GivenOtp("joey", "123456");

            ShouldBeValid("joey", "1234", "123456");
        }

        private void ShouldBeValid(string accountId, string password, string otp)
        {
            var isValid = _authenticationService.Verify(accountId, password, otp);

            Assert.IsTrue(isValid);
        }

        private void GivenOtp(string accountId, string otp)
        {
            _otpService.GetOtp(accountId).Returns(otp);
        }

        private void GivenHashedPassword(string password, string hashedPassword)
        {
            _hash.ComputeHash(password).Returns(hashedPassword);
        }

        private void GivenPasswordFromDB(string accountId, string hashedPasswordFromDb)
        {
            _profile.Password(accountId).Returns(hashedPasswordFromDb);
        }
    }
}