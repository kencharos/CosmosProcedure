using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using Xunit;
using System.Threading.Tasks;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using Xunit.Abstractions;

namespace CosmosProcedure
{

    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class Sample
    {
        public String Id { get; set; }
        public String Category { get; set; }
        public long Price { get; set; }
    }

    public class Fixture : IAsyncLifetime
    {
        public CosmosClient Client { get; set; } = new CosmosClient();

        public Fixture()
        {
        }

        /// <summary>
        /// 1000件データ登録
        /// </summary>
        /// <returns></returns>
        public async Task InitializeAsync()
        {
            await Client.CreateDBandCollection();

            await Task.WhenAll(Enumerable.Range(0, 3000).Select(i =>
            {
                return Client.AddDocument(new Sample { Category = (i / 300) + "", Price = i });
            }));
        }

        public async Task DisposeAsync()
        {
            await Client.DeleteDBandCollection();
        }
    }

    public class ProcedureTest : IClassFixture<Fixture>
    {
        Fixture fixture;
        ITestOutputHelper output;

        public ProcedureTest(Fixture fixture, ITestOutputHelper output)
        {
            this.fixture = fixture;
            this.output = output;
        }


        [Fact]
        public async Task TestCallProcedure()
        {
            await fixture.Client.CreateProcedure("groupBy",
                new System.IO.StreamReader("groupBy.js").ReadToEnd());

            var result = await fixture.Client.CallProcedure<IDictionary<string, int>>("groupBy", new List<string> { "1", "3", "4", "5", "8" });

            output.WriteLine("RU/s:" + result.Item1);

            Assert.Equal(5, result.Item2.Count);
            Assert.Equal(300, result.Item2["1"]);
            Assert.Equal(300, result.Item2["3"]);
            Assert.Equal(300, result.Item2["4"]);
            Assert.Equal(300, result.Item2["5"]);
            Assert.Equal(300, result.Item2["8"]);
        }
    }
}
