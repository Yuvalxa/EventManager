using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;

namespace EventManger.Services
{
    static class Container
    {
        public static IUnityContainer Instance = new UnityContainer(); 
    }
}
