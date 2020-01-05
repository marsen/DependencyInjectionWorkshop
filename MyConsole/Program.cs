using System;
using Autofac;
using DependencyInjectionWorkshop.Models;
using DependencyInjectionWorkshop.Models.Decorator;
using DependencyInjectionWorkshop.Models.Interface;

namespace MyConsole
{
    internal class Program
    {
        private static IContainer _container;

        private static void Main(string[] args)
        {
            RegisterContainer();
            var authentication = _container.Resolve<IAuthentication>();
            var isValid = authentication.Verify("joey", "abc", "wrong otp");
            Console.WriteLine($"result:{isValid}");
            Console.ReadLine();
        }

        private static void RegisterContainer()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<FakeOtp>().As<IOtpService>();
            builder.RegisterType<FakeHash>().As<IHash>();
            builder.RegisterType<FakeProfile>().As<IProfile>();
            builder.RegisterType<FakeLogger>().As<ILogger>();
            builder.RegisterType<FakeSlack>().As<INotification>();
            builder.RegisterType<FakeFailedCounter>().As<IFailedCounter>();
            builder.RegisterType<AuthenticationService>().As<IAuthentication>();

            builder.RegisterDecorator<FailedCounterDecorator, IAuthentication>();
            builder.RegisterDecorator<LoggerDecorator, IAuthentication>();
            builder.RegisterDecorator<NotificationDecorator, IAuthentication>();

            _container = builder.Build();
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