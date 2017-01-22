using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(VRFace.Startup))]

namespace VRFace
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureMobileApp(app);
        }
    }
}