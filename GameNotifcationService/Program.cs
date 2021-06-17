using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topshelf;

namespace GameNotificationService
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceMethods.APIInitiator();
            var exitCode = HostFactory.Run(x =>
            {
                x.Service<ServiceMethods>(s =>
                {
                    s.ConstructUsing(games => new ServiceMethods());
                    s.WhenStarted(games => games.Start());
                    s.WhenStopped(games => games.Stop());
                });

                x.RunAsLocalSystem();

                x.SetServiceName("GameNotificationService");
                x.SetDisplayName("Game Notification Service");
                x.SetDescription("This is the Game Notification Service.");
            });

            int exitCodeValue = (int)Convert.ChangeType(exitCode, exitCode.GetTypeCode());
            Environment.ExitCode = exitCodeValue;
        }
    }
}
