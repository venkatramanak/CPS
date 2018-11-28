using Microsoft.Xrm.Sdk;

namespace Microsoft.Dynamics365.BPF.Stage.Audit.Logs
{
    public class Logger
    {
        private ITracingService _tracingservice = null;
        private static readonly object syncroot = new object();
        private static Logger _instance;

        public static Logger LogInstance
        {
            get
            {
                if (_instance == null)
                {
                    lock (syncroot)
                    {
                        if (_instance == null)
                        {
                            _instance = new Logger();
                        }
                    }
                }
                return _instance;
            }
        }

        public void AssignTrace(ITracingService tracingservice)
        {
            if(_tracingservice == null)
                _tracingservice = tracingservice;
        }
        public void LogWarning(string methodName, string eventDetails, string message)
        {
            TraceLog($"MethodName : { methodName} \n Eventdetails : {eventDetails} \n {message}");

        }
        public void LogError(string methodName, string eventDetails, string stacktrace)
        {
            TraceLog($"MethodName : { methodName} \n Eventdetails : {eventDetails} \n {stacktrace}");
        }

        private void TraceLog(string message)
        {
            _tracingservice.Trace(message);
        }
    }
}
