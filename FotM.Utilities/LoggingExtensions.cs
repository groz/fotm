using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace FotM.Utilities
{
    public static class LoggingExtensions
    {
        public static ILog GetLogger<T>()
        {
            return LogManager.GetLogger(typeof(T));
        }
    }
}
