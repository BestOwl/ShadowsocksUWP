using System.Diagnostics;
using Windows.ApplicationModel.Background;
using Windows.Networking.Vpn;

namespace ShadowsocksBG
{
    public sealed class VpnTask : IBackgroundTask
    {
        private static IVpnPlugIn _pluginInstance = null;
        private static object _pluginLocker = new object();

        public static IVpnPlugIn GetPlugin()
        {
            if (_pluginInstance == null)
            {
                lock (_pluginLocker)
                {
                    if (_pluginInstance != null) return _pluginInstance;
                    _pluginInstance = new VpnPlugin();
                }
            }
            return _pluginInstance;
        }

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            var plugin = GetPlugin() as VpnPlugin;
            plugin.def = taskInstance.GetDeferral();
            VpnChannel.ProcessEventAsync(GetPlugin(), taskInstance.TriggerDetails);
        }
    }
}
