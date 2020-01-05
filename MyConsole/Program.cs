using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DependencyInjectionWorkshop.Models;
using DependencyInjectionWorkshop.Models.Decorator;
using DependencyInjectionWorkshop.Models.Interface;

namespace MyConsole
{
    class Program
    {

        private static IAuthentication _authentication;
        private static IFailedCounter _failedCounter;
        private static IHash _hash;
        private static ILogger _logger;
        private static INotification _notification;
        private static IOtpService _otpService;
        private static IProfile _profile;
        static void Main(string[] args)
        {

            _otpService = new FakeOtp();
            _hash = new FakeHash();
            _profile = new FakeProfile();
            _logger = new FakeLogger();
            _notification = new FakeSlack();
            _failedCounter = new FakeFailedCounter();
            _authentication =
                new AuthenticationService(_profile, _hash,_otpService);

            _authentication = new FailedCounterDecorator(_authentication, _failedCounter);
            _authentication = new LoggerDecorator(_authentication, _logger, _failedCounter);
            _authentication = new NotificationDecorator(_authentication, _notification);


            var isValid = _authentication.Verify("joey", "abc", "wrong otp");
            Console.WriteLine($"result:{isValid}");
            Console.ReadLine();

        }


        internal class FakeLogger : ILogger
        {
            public void Info(string message)
            {
                Console.WriteLine($"Logger: {message}");
            }
        }

        internal class FakeSlack : INotification
        {
            public void PushMessage(string message)
            {
                Console.WriteLine(message);
            }

            public void Notify(string accountId, string message)
            {
                PushMessage($"{nameof(Notify)}, accountId:{accountId}, message:{message}");
            }
        }

        internal class FakeFailedCounter : IFailedCounter
        {
            public void Reset(string accountId)
            {
                Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(Reset)}({accountId})");
            }

            public void Add(string accountId)

            { 
                Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(Add)}({accountId})");
            }


            public int Get(string accountId)
            {
                Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(Get)}({accountId})");
                return 91;
            }

            public bool IsLocked(string accountId)
            {
                Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(IsLocked)}({accountId})");
                return false;
            }
        }

        internal class FakeOtp : IOtpService
        {
            public string GetOtp(string accountId)
            {
                Console.WriteLine($"{nameof(FakeOtp)}.{nameof(GetOtp)}({accountId})");
                return "123456";
            }

        }

        internal class FakeHash : IHash
        {
            public string ComputeHash(string plainText)
            {
                Console.WriteLine($"{nameof(FakeHash)}.{nameof(ComputeHash)}({plainText})");
                return "my hashed password";
            }

        }

        internal class FakeProfile : IProfile
        {
            public string Password(string accountId)
            {
                Console.WriteLine($"{nameof(FakeProfile)}.{nameof(Password)}({accountId})");
                return "my hashed password";
            }

        }
    }
}
