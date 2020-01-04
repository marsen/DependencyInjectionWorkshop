using DependencyInjectionWorkshop.Models;
using NSubstitute;
using NUnit.Framework;

namespace DependencyInjectionWorkshopTests
{
    [TestFixture]
    public class AuthenticationServiceTests
    {
        [Test]
        public void is_valid()
        {
            var profile = Substitute.For<IProfile>();
            var hash = Substitute.For<IHash>();
            var otpService = Substitute.For<IOtpService>();
            var logger = Substitute.For<ILogger>();
            var notification = Substitute.For<INotification>();
            var failedCounter = Substitute.For<IFailedCounter>();

            var authenticationService = new AuthenticationService(
                profile, hash, notification, failedCounter, otpService, logger);

            profile.Password("joey").Returns("my hashed password");
            hash.ComputeHash("1234").Returns("my hashed password");
            otpService.GetOtp("joey").Returns("123456");

            var isValid = authenticationService.Verify("joey", "1234", "123456");

            Assert.IsTrue(isValid);
        }
    }
}