using SensorServerApi;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManger.AlarmLogic
{
    static class HandlerLocator
    {
        private static ConcurrentDictionary<SensorType, IStatusHandler> _handlers = new ConcurrentDictionary<SensorType, IStatusHandler>(
                    new Dictionary<SensorType, IStatusHandler>()
                {
                    {SensorType.Video, new VideoStatusHandler()},
                    {SensorType.Fence, new FenceStatusHandler()},
                    {SensorType.AccessControl, new AccessControlStatusHandler()},
                    {SensorType.FireDetection, new FireDetectionStatusHandler()},
                    {SensorType.Radar, new RadarStatusHandler()},
                }
            );

        public static IStatusHandler GetHandlerBySensorType(SensorType sensorType)
        {
            IStatusHandler statusHandler;
            if (!_handlers.TryGetValue(sensorType, out statusHandler))
            {
                statusHandler = new BaseStatusHandler();
            }
            return statusHandler;
        }
    }
}
