using ShadowsocksBG;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.Vpn;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

//“空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 上有介绍

namespace shadowsocks_uwp
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public const string _ShadowsocksBG_Name = "ShadowsocksBG";
        public const string _ShadowsocksBG_TaskEntryPoint = "ShadowsocksBG.VpnTask";

        public BackgroundTaskRegistration ShadowsocksBG;

        public MainPage()
        {
            this.InitializeComponent();

            ShadowsocksBG = CheckBackgroundTask();
            if (ShadowsocksBG == null)
            {
                var builder = new BackgroundTaskBuilder();

                builder.Name = _ShadowsocksBG_Name;
                builder.TaskEntryPoint = _ShadowsocksBG_TaskEntryPoint;
                builder.SetTrigger(new ApplicationTrigger());

                ShadowsocksBG = builder.Register();
            }
        }

        /// <summary>
        /// Check if BackgroundTask is registered
        /// </summary>
        private BackgroundTaskRegistration CheckBackgroundTask()
        {
            foreach (var task in BackgroundTaskRegistration.AllTasks)
            {
                if (task.Value.Name == _ShadowsocksBG_Name && task.Value is BackgroundTaskRegistration)
                {
                    return task.Value as BackgroundTaskRegistration;
                }
            }
            return null;
        }

        private async void button_Click(object sender, RoutedEventArgs e)
        {
        }

    }
}
