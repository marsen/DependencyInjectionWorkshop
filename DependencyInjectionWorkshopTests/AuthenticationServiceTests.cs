using System;
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
        private IAuthenticationService _authenticationService;

        [SetUp]
        public void SetUp()
        {
            _logger = Substitute.For<ILogger>();
            _failedCounter = Substitute.For<IFailedCounter>();
            _otpService = Substitute.For<IOtpService>();
            _hash = Substitute.For<IHash>();
            _notify = Substitute.For<INotify>();
            _profile = Substitute.For<IProfile>();
            _authenticationService =
                new AuthenticationService(_profile, _hash, _otpService, _failedCounter);
            _authenticationService = new LoggerDecorator(_authenticationService, _logger, _failedCounter);
            _authenticationService = new NotifyDecorator(_authenticationService, _notify);
        }

        [Test]
        public void is_valid()
        {
            WhenValid();
            ShouldBeValid();
        }


        [Test]
        public void is_invalid()
        {
            WhenInvalid();
            ShouldBeInvalid();
        }

        [Test]
        public void is_invalid_should_add_failed_count()
        {
            WhenInvalid();
            ShouldAddFailedCount();
        }

        [Test]
        public void is_invalid_should_notify()
        {
            WhenInvalid();
            ShouldNotify();
        }

        [Test]
        public void is_invalid_should_log()
        {
            WhenInvalid();
            ShouldLog();
        }

        [Test]
        public void is_valid_should_reset()
        {
            WhenValid();
            ShouldReset();
        }

        [Test]
        public void when_account_locked_throw_exception()
        {
            GivenAccountIsLocked();
            ShouldThrow<FailedTooManyTimesException>();
        }

        private void ShouldThrow<TException>() where TException : Exception
        {
            void Code()
            {
                _authenticationService.Verify(DefaultTestAccount, "password", "OTP");
            }

            Assert.Throws<TException>(Code);
        }

        private void GivenAccountIsLocked()
        {
            _failedCounter.IsLocked(DefaultTestAccount).Returns(true);
        }


        private void ShouldReset()
        {
            var result = _authenticationService.Verify(DefaultTestAccount, "password", "OTP");
            _failedCounter.Received(1).Reset(DefaultTestAccount);
        }

        private void ShouldLog()
        {
            var result = _authenticationService.Verify(DefaultTestAccount, "password", "OTP");
            _failedCounter.Received(1).GetCount(DefaultTestAccount);
            _logger.Received(1).Log(Arg.Is<string>(s => s.Contains(DefaultTestAccount)));
        }


        private void ShouldNotify()
        {
            var result = _authenticationService.Verify(DefaultTestAccount, "password", "OTP");
            _notify.Received(1).Notify(Arg.Is<string>(s => s.Contains(DefaultTestAccount)));
        }

        private void WhenValid()
        {
            GivenPasswordFromDb(DefaultTestAccount, "hashed password");
            GivenOneTimePassword(DefaultTestAccount, "OTP");
            GivenHashedPassword("password", "hashed password");
        }


        protected virtual void WhenInvalid()
        {
            GivenPasswordFromDb(DefaultTestAccount, "hashed password");
            GivenOneTimePassword(DefaultTestAccount, "Error OTP");
            GivenHashedPassword("password", "hashed password");
        }


        private void ShouldAddFailedCount()
        {
            var result = _authenticationService.Verify(DefaultTestAccount, "password", "OTP");
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
            var result = _authenticationService.Verify(DefaultTestAccount, "password", "OTP");
            Assert.IsFalse(result);
        }

        private void ShouldBeValid()
        {
            var result = _authenticationService.Verify(DefaultTestAccount, "password", "OTP");
            Assert.IsTrue(result);
        }
    }
}