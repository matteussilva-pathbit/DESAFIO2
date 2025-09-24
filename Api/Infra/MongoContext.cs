using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace DESAFIO2.Api.Infra;

public class MongoSettings { public string ConnectionString { get; set; } = ""; public string Database { get; set; } = ""; public string Collection { get; set; } = ""; }

public class MongoContext
{
  public IMongoCollection<T> GetCollection<T>(IOptions<MongoSettings> opt) =>
    new MongoClient(opt.Value.ConnectionString)
      .GetDatabase(opt.Value.Database)
      .GetCollection<T>(opt.Value.Collection);
}
