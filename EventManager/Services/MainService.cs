﻿using EventManger.Enums;
using SensorServerApi;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Unity;

namespace EventManger.Services
{
    public class MainService
    {
        private ISensorServer _sensorServer;
        private ConcurrentDictionary<string, SensorStatus> _statuses; // Store statuses with expiry time
        public readonly Subject<(OperationType operationType, SensorStatus sensorStatus, Sensor sensor)> OnSensorStatusUpdate;
        private CancellationTokenSource _cancellationTokenSource;
        private SemaphoreSlim _semaphore;
        

        [InjectionMethod]
        public void Inject(ISensorServer sensorServer)
        {
            _sensorServer = sensorServer;
        }

        public MainService()
        {
            _statuses = new ConcurrentDictionary<string, SensorStatus>();
            OnSensorStatusUpdate = new Subject<(OperationType operationType, SensorStatus sensorStatus, Sensor sensor)>();
            _cancellationTokenSource = new CancellationTokenSource();
            _semaphore = new SemaphoreSlim(1, 1); // Allow one thread at a time
        }

        public async Task Start()
        {
            _sensorServer.OnSensorStatusEvent += _sensorServer_OnSensorStatusEvent;
            await _sensorServer.StartServer(Rate.Hardcore, isContinuous: true);
        }

        public async Task DeleteStatus(Guid sensorId)
        {
            Sensor sensor = await _sensorServer.GetSensorById(sensorId);
            await Task.Run(() => {
                if (_statuses.TryGetValue(sensor.Name, out SensorStatus sensorStatus))
                {
                    _statuses.TryRemove(sensor.Name, out _);
                    OnSensorStatusUpdate.OnNext((operationType: OperationType.Remove, sensorStatus: sensorStatus, sensor: null));
                }
            });
        }
        //TODO: add stop logic if needed
        public async Task Stop()
        {
            await _sensorServer.StopServer();
            _sensorServer.OnSensorStatusEvent -= _sensorServer_OnSensorStatusEvent;
        }
        /// Event handler for processing incoming sensor status events.
        private async void _sensorServer_OnSensorStatusEvent(SensorStatus sensorStatus)
        {
            Console.WriteLine($"got {sensorStatus.SensorId}");
            await _semaphore.WaitAsync(); // Wait for access
            try
            {
                await HandleSensorStatus(sensorStatus);
            }
            finally
            {
                _semaphore.Release(); // Release access
            }
        }
        /// Handles a sensor status by updating or adding it to the dictionary and notifying subscribers.
        private async Task HandleSensorStatus(SensorStatus sensorStatus)
        {
            Sensor sensor = await _sensorServer.GetSensorById(sensorStatus.SensorId);
            var expiryTime = sensorStatus.TimeStamp.AddSeconds(15).TimeOfDay - sensorStatus.TimeStamp.TimeOfDay; // Set expiry time ( 15 sec after reciveTime) 
            OperationType operationType;
            if (_statuses.TryGetValue(sensor.Name, out var oldStatus)) // Update Operation
            {
                operationType = OperationType.Update;
                _statuses[sensor.Name] = sensorStatus; // UpdateStatus
                Console.WriteLine($"Update - {sensorStatus.SensorId} , {sensor.Name}");
            }
            else // if not exits Add Opration
            {
                operationType = OperationType.Add;
                Console.WriteLine($"Add - {sensorStatus.SensorId}, {sensor.Name}");
                _statuses.TryAdd(sensor.Name, sensorStatus);
            }
            ScheduleExpiry(sensor.Name, expiryTime);
            OnSensorStatusUpdate.OnNext((operationType, sensorStatus, sensor));
        }

        /// Schedules the expiry of a sensor status after a specified delay.
        private void ScheduleExpiry(string sensorId, TimeSpan delay)
        {
            _ = Task.Run(async () =>
            {
                await Task.Delay(delay);
                if (_statuses.TryRemove(sensorId, out var removedStatus))
                {
                    OnSensorStatusUpdate.OnNext((OperationType.Remove, removedStatus, null));
                }
            });
        }
    }
}
