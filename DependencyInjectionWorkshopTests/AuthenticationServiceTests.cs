using DependencyInjectionWorkshop.Models;
using NSubstitute;
using NSubstitute.Core;
using NUnit.Framework;

namespace DependencyInjectionWorkshopTests
{
    [TestFixture]
    public class AuthenticationServiceTests
    {
        [Test]
        public void is_valid()
        {
            ILogger logger = Substitute.For<ILogger>();
            IFailedCounter failedCounter = Substitute.For<IFailedCounter>();
            IOtpService otpService = Substitute.For<IOtpService>();
            IHash hash = Substitute.For<IHash>();
            INotify notify = Substitute.For<INotify>();
            IProfile profile = Substitute.For<IProfile>();
            var authenticationService =
                new AuthenticationService(notify, profile, hash, otpService, failedCounter, logger);
            var result = authenticationService.Verify("marsen", "password", "OTP");
            Assert.IsTrue(result);
        }
    }
}