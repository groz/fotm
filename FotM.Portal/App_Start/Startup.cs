using Microsoft.Owin;
using Owin;

namespace FotM.Portal
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
        }
    }
}
