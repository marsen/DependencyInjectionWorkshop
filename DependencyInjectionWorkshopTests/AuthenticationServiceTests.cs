﻿using System;
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
        private const string DefaultAccountId = "joey";
        private const int DefaultFailedCount = 91;

        [SetUp]
        public void Setup()
        {
            _profile = Substitute.For<IProfile>();
            _hash = Substitute.For<IHash>();
            _otpService = Substitute.For<IOtpService>();
            _logger = Substitute.For<ILogger>();
            _notification = Substitute.For<INotification>();
            _failedCounter = Substitute.For<IFailedCounter>();
            _authenticationService =
                new AuthenticationService(_profile, _hash, _notification, _failedCounter, _otpService, _logger);
        }

        [Test]
        public void is_valid()
        {
            GivenPasswordFromDB(DefaultAccountId, "my hashed password");
            GivenHashedPassword("1234", "my hashed password");
            GivenOtp(DefaultAccountId, "123456");

            ShouldBeValid(DefaultAccountId, "1234", "123456");
        }

        [Test]
        public void reset_failed_counter_when_is_valid()
        {
            WhenValid();
            FailedCounterShouldReset();
        }

        private void FailedCounterShouldReset()
        {
            _failedCounter.Received(1).Reset(DefaultAccountId);
        }

        protected virtual void WhenValid()
        {
            GivenPasswordFromDB(DefaultAccountId, "my hashed password");
            GivenHashedPassword("1234", "my hashed password");
            GivenOtp(DefaultAccountId, "123456");

            var isValid = _authenticationService.Verify(DefaultAccountId, "1234", "123456");
        }

        [Test]
        public void is_invalid()
        {
            GivenPasswordFromDB(DefaultAccountId, "my hashed password");
            GivenHashedPassword("1234", "my hashed password");
            GivenOtp(DefaultAccountId, "123456");

            ShouldBeInvalid(DefaultAccountId, "1234", "wrong otp");
        }

        [Test]
        public void log_when_invalid()
        {
            GivenFailedCount(DefaultFailedCount);
            WhenInvalid();

            this._logger.Received(1).Info(Arg.Is<string>(x => x.Contains(DefaultFailedCount.ToString())));
        }


        [Test]
        public void notify_user_when_invalid()
        {
            WhenInvalid();
            ShouldNotify(DefaultAccountId);
        }

        [Test]
        public void account_is_locked()
        {
            GivenAccountIsLocked(true);
            ShouldThrow<FailedTooManyTimesException>();
        }


        private void ShouldThrow<TException>() where TException : Exception
        {
            TestDelegate action = () => _authenticationService.Verify(DefaultAccountId, "1234", "123456");
            Assert.Throws<TException>(action);
        }


        private void GivenAccountIsLocked(bool isLocked)
        {
            _failedCounter.IsLocked(DefaultAccountId).Returns(isLocked);
        }

        private void ShouldNotify(string accountId)
        {
            _notification.Received(1).Notify(accountId, Arg.Is<string>(s => s.Contains(accountId)));
        }

        private bool WhenInvalid()
        {
            GivenPasswordFromDB(DefaultAccountId, "my hashed password");
            GivenHashedPassword("1234", "my hashed password");
            GivenOtp(DefaultAccountId, "123456");

            var isValid = _authenticationService.Verify(DefaultAccountId, "1234", "wrong otp");
            return isValid;
        }

        private void GivenFailedCount(int failedCount)
        {
            this._failedCounter.Get(DefaultAccountId).Returns(failedCount);
        }

        private void ShouldBeInvalid(string accountId, string password, string otp)
        {
            var isValid = _authenticationService.Verify(accountId, password, otp);

            Assert.IsFalse(isValid);
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