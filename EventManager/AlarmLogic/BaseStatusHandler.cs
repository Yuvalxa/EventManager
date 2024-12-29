using SensorServerApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManger.AlarmLogic
{
    class BaseStatusHandler : IStatusHandler
    {
        public virtual bool IsAlarming(SensorStatus sensorStatus)
        {
            return sensorStatus.IsAlarmStatus;
        }
    }
}
