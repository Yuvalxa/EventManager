using SensorServerApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManger.AlarmLogic
{
    class FenceStatusHandler: BaseStatusHandler
    {
        public override bool IsAlarming(SensorStatus sensorStatus)
        {
            return sensorStatus.StatusType == StatusType.Disconnected 
                || sensorStatus.StatusType == StatusType.Alarm
                || sensorStatus.StatusType == StatusType.Off;
        }
    }
}
