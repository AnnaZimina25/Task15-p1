using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static RedisMongo.Redis;

namespace RedisMongo
{
    class Mongo
    {
        const string MongoDBConnectionString = "mongodb://localhost:27017";
        const string databaseName = "formData";
        const string collectionName = "personData";

        public static async Task TableDataInput(List<Person> list)
        {
            var client = new MongoClient(MongoDBConnectionString);
            var database = client.GetDatabase(databaseName);
            var personData = database.GetCollection<Person>(collectionName);

            if (list.Count > 0)
            {
                 foreach (var person in list)
                 {
                     //BsonDocument personBs = person.ToBsonDocument();
                   await  personData.InsertOneAsync(person);

                 }
            }

               
        }

                
    }

        
    
}
