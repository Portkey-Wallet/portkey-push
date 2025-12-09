namespace MessagePush.Entities;

public class MultiChainEntity<TKey> : MessagePushEntity<TKey>, IMultiChain
{
    public virtual int ChainId { get; set; }


    protected MultiChainEntity()
    {
    }

    protected MultiChainEntity(TKey id)
        : base(id)
    {
    }
}