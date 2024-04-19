namespace MessagePush.Options;

/// <summary>
/// Options for message push functionality.
/// </summary>
public class MessagePushOptions
{
    // Constants
    private const int DefaultExpiredDeviceInfoFromDays = 60;
    // FCM actually supports a batch size of 500 messages, but we use 400 to avoid the limit.
    private const int DefaultSendAllBatchSize = 400;

    // Fields
    private int _expiredDeviceInfoFromDays;
    private int _sendAllBatchSize;
    
    
    /// <summary>
    /// Gets or sets the number of days before a device info is considered expired.
    /// If not set, defaults to DefaultExpiredDeviceInfoFromDays.
    /// </summary>
    public int ExpiredDeviceInfoFromDays {
        get => _expiredDeviceInfoFromDays > 0 ? _expiredDeviceInfoFromDays : DefaultExpiredDeviceInfoFromDays;
        set => _expiredDeviceInfoFromDays = value;
    }
    
    /// <summary>
    /// Gets or sets the batch size for sending all messages.
    /// If not set, defaults to DefaultSendAllBatchSize.
    /// </summary>
    public int SendAllBatchSize  {
        get => _sendAllBatchSize > 0 ? _sendAllBatchSize : DefaultSendAllBatchSize;
        set => _sendAllBatchSize = value;
    }
}