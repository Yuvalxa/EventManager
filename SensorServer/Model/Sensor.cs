using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensorServerApi
{
    public class Sensor : IIdEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public SensorType SensorType { get; set; }
    }
}
