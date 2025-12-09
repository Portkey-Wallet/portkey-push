namespace IM.Hub;

public class HubCacheOptions
{
    public Dictionary<string, int> MethodResponseTtl { get; set; }
    public int DefaultResponseTtl { get; set; }
}