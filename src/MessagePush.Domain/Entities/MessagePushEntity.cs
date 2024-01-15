using System;
using Volo.Abp.Domain.Entities;

namespace MessagePush.Entities;

[Serializable]
public abstract class MessagePushEntity <TKey> : Entity, IEntity<TKey>
{
    /// <inheritdoc/>
    public virtual TKey Id { get; set; }

    protected MessagePushEntity()
    {

    }

    protected MessagePushEntity(TKey id)
    {
        Id = id;
    }

    public override object[] GetKeys()
    {
        return new object[] {Id};
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"[ENTITY: {GetType().Name}] Id = {Id}";
    }
}