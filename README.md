# Event Manager: Sensor Status Processing

This document provides an explanation of how the `MainService` class processes sensor status updates from the `SensorServer` API, queues them, and handles their lifecycle.
---

## Process Flow

### 1. **Receiving Sensor Status Events**
   - The `MainService` subscribes to the `OnSensorStatusEvent` from the `SensorServer`.
   - When an event is received, the corresponding `SensorStatus` is added to the queue.

#### Code:
```csharp
private async void _sensorServer_OnSensorStatusEvent(SensorStatus sensorStatus)
{
    Console.WriteLine($"got {sensorStatus.SensorId}");
    await Task.Run(() => _sensorsQueue.Enqueue(sensorStatus));
}
```

---

### 2. **Queue Processing**
   - A background task runs continuously, dequeuing and handling each `SensorStatus`.
   - If the queue is empty, the task waits briefly before retrying.

#### Code:
```csharp
private async Task ProcessQueue()
{
    while (!_cancellationTokenSource.Token.IsCancellationRequested)
    {
        try
        {
            while (_sensorsQueue.TryDequeue(out var sensorStatus))
            {
                await HandleSensorStatus(sensorStatus);
            }
            await Task.Delay(100); // Wait until there's an item in the queue
        }
        catch (OperationCanceledException)
        {
            break;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing queue: {ex.Message}");
        }
    }
}
```

---

### 3. **Handling Sensor Status**
   - Each status is checked against the dictionary:
     - If it exists, the status is updated.
     - If not, it is added as a new entry.
   - Expiry is scheduled for 15 seconds after the timestamp of the received status.

#### Code:
```csharp
private async Task HandleSensorStatus(SensorStatus sensorStatus)
{
    Sensor sensor = await _sensorServer.GetSensorById(sensorStatus.SensorId);
    var expiryTime = sensorStatus.TimeStamp.AddSeconds(15).TimeOfDay - sensorStatus.TimeStamp.TimeOfDay;
    OperationType operationType;
    if (_statuses.TryGetValue(sensor.Name, out var oldStatus))
    {
        operationType = OperationType.Update;
        _statuses[sensor.Name] = sensorStatus;
        Console.WriteLine($"Update - {sensorStatus.SensorId}, {sensor.Name}");
    }
    else
    {
        operationType = OperationType.Add;
        Console.WriteLine($"Add - {sensorStatus.SensorId}, {sensor.Name}");
        _statuses.TryAdd(sensor.Name, sensorStatus);
    }
    ScheduleExpiry(sensor.Name, expiryTime);
    OnSensorStatusUpdate.OnNext((operationType, sensorStatus, sensor));
}
```

---

### 4. **Scheduling Expiry**
   - Expiry is scheduled using `Task.Delay`. After 15 seconds, the status is removed from the dictionary.
   - The removal triggers an `OnSensorStatusUpdate` notification.

#### Code:
```csharp
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
```

---

## Visual Representation

**Diagram: Sensor Status Processing**

1. **Event Reception**:
   - `SensorServer` triggers `OnSensorStatusEvent`.
   - Status is enqueued into the `ConcurrentQueue`.

2. **Queue Processing**:
   - Background task processes the queue.
   - Status is handled (Add/Update logic).

3. **Expiry Scheduling**:
   - Expiry is set for 15 seconds post reception.
   - Status is removed upon expiry, with a notification emitted.

*(Include here a diagram or flowchart illustrating the steps, if applicable.)*

---
![image](https://github.com/user-attachments/assets/576f0da2-a925-4564-a444-4a6ab3f43f50)
---

## Testing Semaphore instead of Queue
Alternate Branch for Semaphore Lock Testing
##branch name testing/semaphore
a separate branch was created to replace the ConcurrentQueue with a semaphore lock-based mechanism. This branch tests thread synchronization and ensures accurate processing without a queue structure.
i came to the an understanding that semphore all in all is a great practice but it still a blocking code.

