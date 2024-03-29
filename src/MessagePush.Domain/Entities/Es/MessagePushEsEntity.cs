using Volo.Abp.Domain.Entities;

namespace MessagePush.Entities.Es;

public abstract class MessagePushEsEntity<TKey> : Entity, IEntity<TKey>
{
    public virtual TKey Id { get; set; }

    public override object[] GetKeys()
    {
        return new object[] { Id };
    }
}