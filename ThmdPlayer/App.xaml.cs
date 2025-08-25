using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ThmdPlayer
{
    /// <summary>
    /// Logika interakcji dla klasy App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static Thmd.Configuration.Config Config { get; } = Thmd.Configuration.Config.Instance;

        public App()
        {
            
            //AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            //Dispatcher.UnhandledException += Dispatcher_UnhandledException;
            //TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }
    }
}
