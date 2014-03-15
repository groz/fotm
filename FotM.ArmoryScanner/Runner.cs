using System;
using System.Threading;
using FotM.Utilities;
using log4net;

namespace FotM.ArmoryScanner
{
    class Runner
    {
        private static readonly ILog Logger = LoggingExtensions.GetLogger<Program>();

        private readonly TimeSpan _timeout;
        private readonly Action _action;
        private bool _stopped = false;

        public Runner(Action action, TimeSpan timeout)
        {
            Logger.InfoFormat("Sleep timeout set to {0}", timeout);

            _timeout = timeout;
            _action = action;
        }

        public static Runner TimesPerDay(Action action, int nTimes)
        {
            var timeout = TimeSpan.FromDays(1.0/nTimes);
            return new Runner(action, timeout);
        }

        public void Run()
        {
            while (!_stopped)
            {
                _action();

                Logger.InfoFormat("Sleeping for {0}...", _timeout);
                Thread.Sleep(_timeout);
            }
        }

        public void Stop()
        {
            _stopped = true;
        }
    }
}