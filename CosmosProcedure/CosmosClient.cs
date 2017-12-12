using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CosmosProcedure
{

    public class CosmosClient
    {
        string endPoint;
        string key;
        string database;
        string collection;

        DocumentClient client;

        public CosmosClient(string endporint, string key, string database, string collection)
        {
            this.endPoint = endporint;
            this.key = key;
            this.database = database;
            this.collection = collection;
            this.client = new DocumentClient(new Uri(endporint), key);
        }

        // local emulator
        public CosmosClient() : this("https://localhost:8081",
            "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==", "sample", "col")
        { }

        public Task AddDocument(object doc)
        {
            return client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(database, collection), doc);
        }


        // DB, コレクションを作成する。
        public async Task CreateDBandCollection()
        {
            var col = new DocumentCollection { Id = collection };


            var db = await client.CreateDatabaseIfNotExistsAsync(new Microsoft.Azure.Documents.Database { Id = database });
            await client.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri(database),
                col, new RequestOptions { OfferThroughput = 5000 });
        }


        public async Task DeleteDBandCollection()
        {
            await client.DeleteDatabaseAsync(UriFactory.CreateDatabaseUri(database));
        }

        public async Task CreateProcedure(string name, string procedure)
        {
            await client.CreateStoredProcedureAsync(
                UriFactory.CreateDocumentCollectionUri(database, collection),
                new StoredProcedure { Id = name, Body = procedure });
        }

        public async Task<(double, O)> CallProcedure<O>(string name, object input)
        {
            var res = await client.ExecuteStoredProcedureAsync<O>(
                UriFactory.CreateStoredProcedureUri(database, collection, name),
                input);

            return (res.RequestCharge, res.Response);
        }
    }
}
