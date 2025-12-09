using MessagePush.Commons;

namespace MessagePush.Grains.Grain;

public class GrainResultDto<T> : GrainResultDto
{
    public T Data { get; set; }
}

public class GrainResultDto
{
    public string Code { get; set; } = CommonResult.SuccessCode;

    public bool Success() => Code == CommonResult.SuccessCode;
}