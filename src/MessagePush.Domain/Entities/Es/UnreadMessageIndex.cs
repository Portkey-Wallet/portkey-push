using System.Collections.Generic;
using AElf.Indexing.Elasticsearch;
using Nest;

namespace MessagePush.Entities.Es;

public class UnreadMessageIndex : MessagePushEsEntity<string>, IIndexBuild
{
    [Keyword] public override string Id { get; set; }
    [Keyword] public string UserId { get; set; }
    [Keyword] public string AppId { get; set; }
    public List<UnreadMessageInfo> UnreadMessageInfos { get; set; }
}

public class UnreadMessageInfo
{
    [Keyword] public string MessageType { get; set; }
    public int UnreadCount { get; set; }
}