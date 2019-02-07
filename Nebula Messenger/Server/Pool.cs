using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using RestSharp;

namespace Nebula_Messenger.Server
{
    public class Pool
    {
        public double[] coordinates;
        public string name;
        public string creator;
        public string username;
        public string poolid;
        public int connectionLimit;

        const string routeRoot = "pools";

        public static List<Pool> ListFromJson(string json)
        {
            return JsonConvert.DeserializeObject<PoolsObj>(json).pools;
        }

        class PoolsObj
        {
            public List<Pool> pools;
        }
        public static void GetAndProcessPools(Action<List<Pool>> process)
        {
            ServerStuff.Request(routeRoot + "/getpools", new { }, response);
            void response(string responseJson)
            {
                PoolsObj poolList = JsonConvert.DeserializeObject<PoolsObj>(responseJson);
                process(poolList.pools);
            }
        }
        public static void CreatePool(Pool pool, Action<IRestResponse> response)
        {
            ServerStuff.Request(routeRoot + "/createpool", pool, response);
        }
    }
}