# ServiceFabric.WatchDog.Connector

This is a client library that can be added to your Stateless/Stateful service so that you can register it with a watchdog with ease.

1. Install the package (https://www.nuget.org/packages/ServiceFabric.Watchdog.Connector/1.0.0)
2. Change your static ServiceEventSource to derive from IServiceEventSource
3. Register ISeviceEventSource in your DI container
4. Decorate your controllers with [ServiceRequestActionFilter] to emit events to ETW
5. Call RegisterHealthCheckAsync from your RunAsync to register an endpoint for the Watchdog to monitor
6. Call RegisterMetricsAsync from your RunAsync to register an endpoint for the Watchdog to monitor

The full watchdog sample can be found here: - https://github.com/Azure-Samples/service-fabric-watchdog-service