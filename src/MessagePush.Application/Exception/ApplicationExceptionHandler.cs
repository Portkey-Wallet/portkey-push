using System;
using System.Threading.Tasks;
using AElf.ExceptionHandler;
using Serilog;

namespace MessagePush.Exception;

public class ApplicationExceptionHandler
{
    
    public static async Task<FlowBehavior> BulkPushHandleException(System.Exception e)
    {
        Log.Error(e, "multicast send firebase exception");
        return new FlowBehavior
        {
            ExceptionHandlingStrategy = ExceptionHandlingStrategy.Return,
            ReturnValue = true
        };
    }

    public static async Task<FlowBehavior> PushHandleException(System.Exception e)
    {
        Log.Error(e, "send firebase exception");
        return new FlowBehavior
        {
            ExceptionHandlingStrategy = ExceptionHandlingStrategy.Return,
            ReturnValue = true
        };
    }
}