namespace ServiceFabric.Helpers
{
    using System.Fabric;

    /// <summary>
    /// Provides the contract that most Event Sources will implement as part of Service Fabric.
    /// </summary>
    public interface IServiceEventSource
    {
        void Trace(string name, string args = "");
        void Error(string name, string args = "");
        void Message(string message);
        void Message(string message, params object[] args);
        void ServiceHostInitializationFailed(string exception);
        void ServiceMessage(ServiceContext serviceContext, string message, params object[] args);
        void ServiceRequestStart(string requestTypeName);
        void ServiceRequestStop(string requestTypeName, string exception = "");
        void ServiceTypeRegistered(int hostProcessId, string serviceType);
    }
}
