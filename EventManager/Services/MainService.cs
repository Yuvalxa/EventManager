using EventManger.AlarmLogic;
using EventManger.Enums;
using SensorServerApi;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unity;

namespace EventManger.Services
{
    public class MainService
    {
        private ISensorServer _sensorServer;
        //List<SensorStatus> _statuses;
        private ConcurrentDictionary<Guid, SensorStatus> _statuses; // Thread-safe diconary to hold sensor statuses

        public Subject<(OperationType operationType, SensorStatus sensorStatus, Sensor sensor)> OnSensorStatusUpdate;

        [InjectionMethod]
        public void Inject(ISensorServer sensorServer)
        {
            _sensorServer = sensorServer;
        }

        public MainService()
        {
            _statuses = new ConcurrentDictionary<Guid, SensorStatus>();
            OnSensorStatusUpdate = new Subject<(OperationType operationType, SensorStatus sensorStatus, Sensor sensor)>();
        }

        public async Task Start()
        {
            _sensorServer.OnSensorStatusEvent += _sensorServer_OnSensorStatusEvent;
            await _sensorServer.StartServer(Rate.Easy, isContinuous: true);
        }

        public Task DeleteStatus(Guid sensorId)
        {
            return Task.Run(() => {
                if(_statuses.TryGetValue(sensorId, out SensorStatus sensorStatus))
                    _statuses.TryRemove(sensorStatus.Id,out _);
                    OnSensorStatusUpdate.OnNext((operationType: OperationType.Remove, sensorStatus: sensorStatus, sensor: null));
            });
        }

        public async Task Stop()
        {
            // Add code to stop the service .. 
            await _sensorServer.StopServer();
        }

        private async void _sensorServer_OnSensorStatusEvent(SensorStatus sensorStatus)
        {
            // Handle status from server ...
            Console.WriteLine($"got {sensorStatus.SensorId}");
            Sensor sensor = await _sensorServer.GetSensorById(sensorStatus.SensorId);

            OperationType operationType;
           if(!_statuses.TryGetValue(sensorStatus.Id,out SensorStatus oldstatus))
            {
                operationType = OperationType.Add;
            }
            else
            {
                operationType = OperationType.Update;
                _statuses.TryRemove(oldstatus.Id, out _);
            }

            _statuses.TryAdd(sensorStatus.Id, sensorStatus);


            Console.WriteLine($"added {sensorStatus.SensorId}, {sensor.Name} to dictionary");

            // update ui
            OnSensorStatusUpdate.OnNext((operationType: operationType, sensorStatus: sensorStatus, sensor: sensor));
        }
    }
}
