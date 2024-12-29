using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensorServerApi
{
    public class SensorStatus
    {
        public Guid Id { get; set; }
        public bool IsAlarmStatus { get; set; }
        public StatusType StatusType { get; set; }
        public DateTime TimeStamp { get; set; }
        public Guid SensorId { get; set; }

        public SensorStatus()
        {
            Id = Guid.NewGuid();
            TimeStamp = DateTime.Now;
        }
    }
}
