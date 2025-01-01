using EventManger.Enums;
using EventManger.Services;
using Prism.Mvvm;
using SensorServerApi;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Unity;

namespace EventManger.ViewModels
{
    public class MainVm : BindableBase
    {
        private MainService _mainService;

        private ObservableCollection<SensorStatusVm> _SensorStatuses;
        public ObservableCollection<SensorStatusVm> SensorStatuses { get { return _SensorStatuses; } set { SetProperty(ref _SensorStatuses, value); } }

        private CollectionViewSource _CollectionSource;
        public CollectionViewSource CollectionSource { get { return _CollectionSource; } set { SetProperty(ref _CollectionSource, value); } }

        [InjectionMethod]
        public void Inject(MainService mainService)
        {
            _mainService = mainService;
        }

        public MainVm()
        {            
            SensorStatuses = new ObservableCollection<SensorStatusVm>();
            CollectionSource = new CollectionViewSource();
            CollectionSource.Source = SensorStatuses;
        }

        public void Init()
        {
            _mainService.OnSensorStatusUpdate
                .ObserveOnDispatcher()
                .Subscribe(HandleSensorStatusFromMainService);

            _mainService.Start().Wait();

        }

        private void HandleSensorStatusFromMainService((OperationType operationType, SensorStatus sensorStatus, Sensor sensor) update)
        {
            var sensorStatusVm = Container.Instance.Resolve<SensorStatusVm>();
            sensorStatusVm.ReadModel(update.sensorStatus, update.sensor, update.sensorStatus.IsAlarmStatus);

            switch (update.operationType)
            {
                case OperationType.Add:
                    _insertStatus(sensorStatusVm);
                    break;
                case OperationType.Update:
                    _updateStatus(sensorStatusVm);
                    break;
                case OperationType.Remove:
                    _removeStatus(sensorStatusVm);
                    break;
                default:
                    break;
            }
        }

        private void _insertStatus(SensorStatusVm sensorStatusVm)
        {
            if (!SensorStatuses.Contains(sensorStatusVm))
            {
                SensorStatuses.Add(sensorStatusVm);
                SortSensorStatuses();
            }
            else
            {
                _updateStatus(sensorStatusVm);
            }
        }

        private void _updateStatus(SensorStatusVm sensorStatusVm)
        {
            var existingItem = SensorStatuses.FirstOrDefault(s => s.Equals(sensorStatusVm));
            if (existingItem != null)
            {
                var index = SensorStatuses.IndexOf(existingItem);
                SensorStatuses[index] = sensorStatusVm;
            }
            else
            {
                _insertStatus(sensorStatusVm);
            }
        }

        private void _removeStatus(SensorStatusVm sensorStatusVm)
        {
            if (SensorStatuses.Contains(sensorStatusVm))
            {
                SensorStatuses.Remove(sensorStatusVm);
            }
        }
        private void SortSensorStatuses()
        {
            var sortedList = SensorStatuses.OrderBy(s => ExtractSensorNumber(s.SensorName)).ToList();
            SensorStatuses.Clear();
            foreach (var item in sortedList)
            {
                SensorStatuses.Add(item);
            }
        }

        private int ExtractSensorNumber(string sensorName)
        {
            if (string.IsNullOrWhiteSpace(sensorName)) return int.MaxValue;

            // Extract the numeric part of "Sensor {num}"
            var match = System.Text.RegularExpressions.Regex.Match(sensorName, @"\d+");
            if (match.Success && int.TryParse(match.Value, out int number))
            {
                return number;
            }

            return int.MaxValue; // Fallback for invalid or non-numeric names
        }
    }
}
