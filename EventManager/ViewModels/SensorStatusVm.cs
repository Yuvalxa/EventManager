using EventManger.Services;
using Prism.Commands;
using Prism.Mvvm;
using SensorServerApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;

namespace EventManger.ViewModels
{
    public class SensorStatusVm : BindableBase, IEquatable<SensorStatusVm>
    {
        private Guid _id;
        private SensorStatus _sensorStatus;
        private MainService _mainService;

        private bool _IsAlarmingStatus;
        public bool IsAlarmingStatus { get { return _IsAlarmingStatus; } set { SetProperty(ref _IsAlarmingStatus, value); } }

        private DateTime _Time;
        public DateTime Time { get { return _Time; } set { SetProperty(ref _Time, value); } }

        private StatusType _StatusType;
        public StatusType StatusType { get { return _StatusType; } set { SetProperty(ref _StatusType, value); } }

        private string _SensorName;
        public string SensorName { get { return _SensorName; } set { SetProperty(ref _SensorName, value); } }

        public DelegateCommand DeleteStatusCommand { get; }

        public SensorStatusVm()
        {
            DeleteStatusCommand = new DelegateCommand(async () => { await _mainService.DeleteStatus(_id); });     
        }

        public bool Equals(SensorStatusVm other)
        {
            return this._id == other._id;
        }

        [InjectionMethod]
        public void Inject(MainService mainService)
        {
            _mainService = mainService;
        }

        internal void ReadModel(SensorStatus sensorStatus, Sensor sensor, bool isAlarming)
        {
            _id = sensorStatus.SensorId;
            _sensorStatus = sensorStatus;
            IsAlarmingStatus = isAlarming;
            Time = sensorStatus.TimeStamp;
            StatusType = sensorStatus.StatusType;
            SensorName = sensor?.Name;
        }
    }
}
