using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ness.Utils
{
    public class RedisHandler
    {
        private ConnectionMultiplexer connection;

        public RedisHandler()
        {
            connection = ConnectionMultiplexer.Connect(ConfigurationManager.AppSettings["redisConnection"]);
            
        }

        public T GetObject<T>(string key)
        {
            IDatabase cache = connection.GetDatabase();
            try
            {
                var model = JsonConvert.DeserializeObject<T>(cache.StringGet(key));
                return model;
            }
            catch (Exception ex)
            {
                return default(T);
            }
        }

        public bool SetObject<T>(string key, T data)
        {
            IDatabase cache = connection.GetDatabase();
            try
            {
                cache.StringSet(key, JsonConvert.SerializeObject(data));
                return true;
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.TraceError("\"" + e.ToString() + "\"");
            }
            return false;
        }

        public bool RemoveObject(string key)
        {
            IDatabase cache = connection.GetDatabase();
            try
            {
                cache.KeyDelete(key);
            }
            catch (Exception)
            {
                //dont care if it fails if the key doesn't exists
            }
            return true;
        }
    }
}
