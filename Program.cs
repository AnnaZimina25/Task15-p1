
using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace RedisMongo
{
    class Program
    {
        
        private static readonly Channel<List<string>> channel1 = Channel.CreateUnbounded<List<string>>();

        static async Task Main()
        {
            //await DbMigration();


            Thread th = new Thread(new ThreadStart(Go));
            th.Start();

            //string firstString = await channel.Reader.ReadAsync();
            //Console.WriteLine($"Line after block method: {firstString}");

            //List<string> list = await Redis.RedisDataOutput();
            List<string> list = await channel1.Reader.ReadAsync();
            //list.Insert(0, firstString);

            await Mongo.TableDataInput(Redis.GetPersonObj(list));

        }

        /*
        private static async Task DbMigration()
        {
            AppConfig configObj = DatabaseServer.GetConfigObj();
            string user = configObj.User.Login;
            string pw = configObj.User.Password;
            string dbn = configObj.User.DatabaseName;

            // подключаемся через супер юзера

            string connString1 = DatabaseServer.GetConnectString(configObj.DbHost, configObj.SuperUser.Login, configObj.SuperUser.Password, configObj.SuperUser.DatabaseName);

            await using (var conn1 = new NpgsqlConnection(connString1))
            {
                await conn1.OpenAsync();

                // Define a query
                var isUserExist = await DatabaseServer.IsUserExist(conn1, user);
                var isDatabaseExists = await DatabaseServer.IsDatabaseExist(conn1, dbn);

                if (isUserExist)
                {
                    Console.WriteLine($"{isUserExist}, User exists.");

                }
                else
                {
                    Console.WriteLine($"{isUserExist}, User doesn't exists. Creating new user...");

                    await DatabaseServer.UserCreation(conn1, user, pw);
                }

                if (isDatabaseExists)
                {
                    Console.WriteLine($"{isDatabaseExists}, Database exists.");
                }
                else
                {
                    Console.WriteLine($"{isDatabaseExists}, Database doesn't exist. Creating new database...");
                    await DatabaseServer.DatabaseCreation(conn1, user, dbn);
                }

                // подключаемся через юзера
                string connString2 = DatabaseServer.GetConnectString(configObj.DbHost, configObj.User.Login, configObj.User.Password, configObj.User.DatabaseName);

                await using (var conn2 = new NpgsqlConnection(connString2))
                {
                    await conn2.OpenAsync();
                    string tableName = "form";
                    var isTableExists = await DatabaseServer.IsCurrentTableExist(conn2, tableName);

                    if (isTableExists)
                    {
                        Console.WriteLine($"Table {tableName} exists");
                    }
                    else
                    {
                        await DatabaseServer.TableCreation(conn2, tableName);
                        Console.WriteLine($"Table {tableName} is created");
                    }
                }

            }
        }
        */

        public static void Go()
        {
            while (true)
            {
                string str = Redis.GetFirstString();

                Console.WriteLine($"Line in block method: {str}");

                if (str != null)
                {
                    //channel.Writer.WriteAsync(str);

                    List<string> list = Redis.GetList();
                    list.Insert(0, str);

                    channel1.Writer.WriteAsync(list);

                    Task.Run(() => Mongo.TableDataInput(Redis.GetPersonObj(list)));
                }
            }
        }

    }
}
