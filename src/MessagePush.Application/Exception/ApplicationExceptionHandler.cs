using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AElf.ExceptionHandler;
using AElf.Indexing.Elasticsearch;
using MessagePush.Entities.Es;
using MessagePush.MessagePush.Provider;
using Serilog;

namespace MessagePush.Exception;

public class ApplicationExceptionHandler
{
    private readonly INESTRepository<UserDeviceIndex, string> _userDeviceRepository;
    public ApplicationExceptionHandler(INESTRepository<UserDeviceIndex, string> userDeviceRepository)
    {
        _userDeviceRepository = userDeviceRepository;
    }
    
    public async Task<FlowBehavior> BulkPushHandleException(System.Exception e)
    {
        Log.Error(e, "multicast send firebase exception");
        return new FlowBehavior
        {
            ExceptionHandlingStrategy = ExceptionHandlingStrategy.Return,
            ReturnValue = true
        };
    }

    public async Task<FlowBehavior> PushHandleException(System.Exception e, string indexId, string token, string icon, string title, string content,
        Dictionary<string, string> data, int badge = 1)
    {
        Log.Error(e, "send firebase exception");
        await HandleExceptionAsync(e.Message, indexId, token);
        return new FlowBehavior
        {
            ExceptionHandlingStrategy = ExceptionHandlingStrategy.Return,
            ReturnValue = true
        };
    }
    
    public async Task HandleExceptionAsync(string exMessage, string indexId, string token)
    {
        if (exMessage.Contains(ResponseErrorMessageConstants.EntityNotFoundErrorMessage) 
            || exMessage.Contains(ResponseErrorMessageConstants.InvalidFcmTokenErrorMessage))
        {
            Log.Error("Exception occurred during Firebase push. Token has expired. Attempting to delete token. IndexId: {indexId}, Token: {token}", indexId, token);
            await _userDeviceRepository.DeleteAsync(indexId);
        }
    }
}