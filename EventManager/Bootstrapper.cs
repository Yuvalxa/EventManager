using EventManger.Services;
using EventManger.ViewModels;
using SensorServerApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Unity.Lifetime;
using Unity;

namespace EventManger
{
    static class Bootstrapper
    {
        public static void Init()
        {
            Container.Instance.RegisterType<ISensorServer, SensorServer>(new ContainerControlledLifetimeManager());
            Container.Instance.RegisterType<MainService>(new ContainerControlledLifetimeManager());
            
            MainVm mainVm = Container.Instance.Resolve<MainVm>();
            mainVm.Init();
            MainWindow mainWindow = new MainWindow();
            mainWindow.DataContext = mainVm;
            mainWindow.Show();
        }
    }
}
