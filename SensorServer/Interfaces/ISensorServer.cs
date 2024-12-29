using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensorServerApi
{
    public delegate void OnSensorStatus(SensorStatus sensorStatus);

    public interface ISensorServer
    {
        event OnSensorStatus OnSensorStatusEvent;
        Task StartServer(Rate rate = Rate.Easy, bool isContinuous = false);
        Task StopServer();
        Task<Sensor> GetSensorById(Guid sensorId);
    }
}
