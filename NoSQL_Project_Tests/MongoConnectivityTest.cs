using MongoDB.Bson;
using MongoDB.Driver;

namespace NoSQL_Project_Tests
{
    [TestFixture]
    public class MongoConnectivityTest
    {
        [SetUp]
        public void LoadEnv()
        {
            try
            {
                // Traverse upwards until .env found (solution root)
                DotNetEnv.Env.TraversePath().Load();
            }
            catch
            {
                TestContext.WriteLine("No .env file found, relying on system environment vars.");
            }
        }

        [Test]
        public async Task Should_Connect_To_Atlas_And_Ping()
        {
            // Arrange
            var conn = Environment.GetEnvironmentVariable("Mongo__ConnectionString");
            if (string.IsNullOrWhiteSpace(conn))
            {
                Assert.Ignore("Mongo__ConnectionString not set. Did you create .env?");
            }

            var settings = MongoClientSettings.FromConnectionString(conn);
            settings.ServerSelectionTimeout = TimeSpan.FromSeconds(5);
            settings.ConnectTimeout = TimeSpan.FromSeconds(5);

            var client = new MongoClient(settings);
            var db = client.GetDatabase("Test");

            // Act
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var result = await db.RunCommandAsync<BsonDocument>(
                new BsonDocument("ping", 1),
                cancellationToken: cts.Token);

            // Assert
            Assert.That(result.Contains("ok"), "Ping result did not contain 'ok'.");
            Assert.That(result["ok"].ToInt32(), Is.EqualTo(1), "Ping did not return ok:1");
        }
    }
}
