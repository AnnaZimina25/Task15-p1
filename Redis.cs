using Newtonsoft.Json;
using ServiceStack.Redis;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RedisMongo
{
    class Redis
    {
        public static string rKey = "dataList";

        public class Person
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }

        public static async Task<List<string>> RedisDataOutput()
        {
            var redisList = new List<string>();
            RedisValue[] array;


            using (var redis = ConnectionMultiplexer.Connect("localhost"))
            {
                IDatabase db = redis.GetDatabase();
                var trans = db.CreateTransaction();

                //var condition = trans.AddCondition(Condition.ListLengthGreaterThan(rKey, 3));
                Task<RedisValue[]> result = trans.ListRangeAsync(rKey, 0, 2);

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                trans.ListTrimAsync(rKey, 3, -1);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                if (await trans.ExecuteAsync())
                {
                    array = await result;

                    foreach (var elem in array)
                    {
                        redisList.Add(elem.ToString());
                        Console.WriteLine(elem);
                    }

                }

            }

            return redisList;
        }

        public static List<Person> GetPersonObj(List<string> redisList)
        {
            List<Person> personList = new List<Person>();

            foreach (var elem in redisList)
            {
                personList.Add(JsonConvert.DeserializeObject<Person>(elem));
            }
            return personList;
        }

        public static string GetFirstString()
        {

            string firstData;
            TimeSpan infinite = TimeSpan.FromMilliseconds(-1);
            var manager = new RedisManagerPool("localhost");

            using (var client = manager.GetClient())
            {

                firstData = client.BlockingRemoveStartFromList(rKey, infinite);
                Console.WriteLine(firstData);

            }

            return firstData;
        }

        public static List<string> GetList()
        {
            List<string> list = new List<string>();
            var manager = new RedisManagerPool("localhost");

            using (var client = manager.GetClient())
            {
                using var trans = client.CreateTransaction();
                trans.QueueCommand(r => r.GetRangeFromList(rKey, 0, 1), s => list = s);
                trans.QueueCommand(r => r.RemoveStartFromList(rKey));
                trans.QueueCommand(r => r.RemoveStartFromList(rKey));
                trans.QueueCommand(r => r.RemoveStartFromList(rKey));

                trans.Commit();

            }
            return list;
        }

        public static void Print(List<Person> list)
        {
            foreach (var elem in list)
            {
                Console.WriteLine($"{elem.FirstName}, {elem.LastName}");
            }
        }
    }
}
