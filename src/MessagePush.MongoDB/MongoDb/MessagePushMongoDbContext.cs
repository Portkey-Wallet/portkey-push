using Volo.Abp.Data;
using Volo.Abp.MongoDB;

namespace MessagePush.MongoDb;

[ConnectionStringName("Default")]
public class MessagePushMongoDbContext : AbpMongoDbContext
{
    /* Add mongo collections here. Example:
     * public IMongoCollection<Question> Questions => Collection<Question>();
     */
    //public IMongoCollection<Token> Tokens => Collection<Token>();
    //public IMongoCollection<TokenPriceData> TokenPriceData => Collection<TokenPriceData>();


    protected override void CreateModel(IMongoModelBuilder modelBuilder)
    {
        base.CreateModel(modelBuilder);

        // modelBuilder.Entity<Token>(t =>
        // {
        //     t.CollectionName = IMConsts.DbTablePrefix + "Token" + IMConsts.DbSchema;
        // });
        // modelBuilder.Entity<TokenPriceData>(t =>
        // {
        //     t.CollectionName = IMConsts.DbTablePrefix + "TokenPrice" + IMConsts.DbSchema;
        // });
        
    }
}