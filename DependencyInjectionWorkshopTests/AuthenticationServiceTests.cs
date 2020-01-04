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
            _hash.ComputeHash("1234").Returns("my hashed password");
            _otpService.GetOtp("joey").Returns("123456");

            var isValid = _authenticationService.Verify("joey", "1234", "123456");

            Assert.IsTrue(isValid);
        }

        private void GivenPasswordFromDB(string accountId, string hashedPasswordFromDb)
        {
            _profile.Password(accountId).Returns(hashedPasswordFromDb);
        }
    }
}