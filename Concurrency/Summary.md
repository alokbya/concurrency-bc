# Task
In C#, a `Task` is a unit of work that can generally be created in two ways: 
```
var t = new Task(Action action);
t.Start();
```
or
```
Task.Factory.StartNew(Action action);
```
Tasks can be passed an object, but this requires boxing and unboking, which is expensive.  Instead, use a lambda expression:
```
var t = new Task(() => DoWork());
t.Start();
```
Tasks can also return values using generics:
```
var t = new Task<int>(() => DoWork());
t.Start();
int result = t.Result;
```
It should be noted that getting the result of a task will block the calling thread until the task is complete. This is known as a blocking operation. 

Tasks can be chained together using `ContinueWith`:
```
var t = new Task(() => DoWork());
t.ContinueWith(() => DoMoreWork());
t.Start();
```
Tasks can report their state:
```
var t = new Task(() => DoWork());
t.Start();
Console.WriteLine(t.IsCompleted);
Console.WriteLine(t.IsFaulted);
```
## Task Cancellation
Tasks can be cancelled using a `CancellationTokenSource`:
```
var cts = new CancellationTokenSource();
var token = cts.Token;
var t = new Task(() => DoWork(token));
t.Start();
cts.Cancel();
```
The cancellation source can generate multiple tokens which can be passed to multiple tasks.  When the source is cancelled, all tasks using the token will be cancelled.

**Cancellation is cooperative** - the task must check the token to see if it has been cancelled.  If it has, it should throw an `OperationCancelledException`.  This will cause the task to be marked as Faulted.  If the task is not cancelled, it should call `ThrowIfCancellationRequested()` on the token to check if it has been cancelled.  This will cause the task to be marked as RanToCompletion.
```
void DoWork(CancellationToken token)
{
    for (int i = 0; i < 100000; i++)
    {
        if (token.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }
        else
        {
            token.ThrowIfCancellationRequested();
        }
    }
}
```
## Waiting for time to pass
The most common way to wait for time to pass is to use `Thread.Sleep()`.  However, this is not recommended for tasks.  Instead, use `Task.Delay()`:
```
var t = new Task(() => DoWork());
t.Start();
Task.Delay(1000).Wait();
```
It is also possible to use a cancellation token's wait handle to wait for time to pass:
```
var cts = new CancellationTokenSource();
var token = cts.Token;
var t = new Task(() => DoWork(token));
t.Start();
var result = token.WaitHandle.WaitOne(1000); // wait for 1000 milliseconds
```
This will return a bool indicating whether cancellation was requested in the time period specified (1000 ms).
You can also use spinning to wait for time to pass:
```
Thread.SpinWait();
SpinWait.SpinUntil(() => false);
```
Spin waiting doees not give up the thread's turn. This is not a recommended way to wait for time to pass, but it is useful for waiting for a lock to be released.

Waiting for a single task is done using `Task.Wait()`:
```
var t = new Task(() => DoWork());
t.Start();
t.Wait();
```
Waiting for multiple tasks is done using `Task.WaitAll()` or `Task.WaitAny()` (which will return when any of the tasks have completed):
```
var t1 = new Task(() => DoWork());
var t2 = new Task(() => DoMoreWork());
t1.Start();
t2.Start();
Task.WaitAll(t1, t2);
// or Task.WaitAny(t1, t2);
```
`WaitAny()` and `WaitAll()` can also take a timeout parameter will throw on cancellation, so it is recommended to use a timeout parameter:
```
var t1 = new Task(() => DoWork());
var t2 = new Task(() => DoMoreWork());
t1.Start();
t2.Start();
Task.WaitAll(new Task[] { t1, t2 }, 1000);
```

## Exception Handling
**An unobserved task exception will not get handled**

Exceptions thrown by tasks are stored in the `AggregateException` class.  This class contains a list of exceptions that were thrown by the task.  It is possible to iterate over the exceptions:
```
var t = new Task(() => DoWork());
t.Start();
try
{
    t.Wait();
}
catch (AggregateException ex)
{
    foreach (var inner in ex.InnerExceptions) // use InnerExceptions to get a list of exceptions that were thrown
    {
        Console.WriteLine(inner.Message);
    }
    // or use ex.Handle(e => {...}) to selectively handle exceptions (return true if handled, false otherwise)
}
```
It is also possible to use the `Flatten()` method to get a single list of exceptions:
```
var t = new Task(() => DoWork());
t.Start();
try
{
    t.Wait();
}
catch (AggregateException ex)
{
    foreach (var inner in ex.Flatten().InnerExceptions) // use Flatten() to get a single list of exceptions
    {
        Console.WriteLine(inner.Message);
    }
}
```

**Note: There are ways of handling unobserved exceptions (exceptions that are thrown by a task but not handled by the calling thread), but they are not recommended.  Instead, it is recommended to handle exceptions in the task itself.**

# Data Sharing & Synchronization
### Controlling concurrent access to data.

## Critical Sections
Areas of code whose access is controlled by a lock object. Only one thread can be in a critical section at a time.  This is done using the `lock` keyword:
```csharp
lock (lockObject)
{
    // critical section
}
```

## Interlocked Operations


## Spin Locking and Lock Recursion
Spin locking is a way of waiting for a lock to be released without giving up the thread's turn.  This is done using the `SpinWait` class:
```
var sw = new SpinWait();
sw.SpinOnce();
```
This will spin once and then return.  It is also possible to spin until a condition is met:
```
SpinWait.SpinUntil(() => false);
```
This will spin until the condition is met.  This is useful for waiting for a lock to be released.

## Mutex

## Reader-Writer Locks

## Atomicity
* An operation is atomic if it cannot be interrupted.
  * It cannot be separated into several different parts such that between the execution of one part and another, a separate thread can execute and change the state of the object.
  * `x = 1` is atomic, but `x++` is not, because it is made up of two operations
    * separated into `temp <- x + 1; x <- temp;`
    * vulnurable to a race condition because there are several things happening and something can jump in there and change the state of the object 
  * In .NET, atomic operations include:
    * reference assignment
    * reads and writes to value types <= 32 bits on a 32 bit system
    * 64 bit reads and writes on a 64 bit system