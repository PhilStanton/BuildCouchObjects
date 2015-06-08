using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Couchbase;
using Couchbase.Configuration;
using Enyim.Caching.Memcached;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SaleCycle.Data.Scheduling;
using SaleCycle.Framework.Serialization;
using SaleCycle.Model.Enums;
using SaleCycle.Model.ShoppingProcesses;
using System.Configuration;
using Couchbase.Management;
using SaleCycle.Contract.Enums;
using SaleCycle.Model.Affiliates;
using SaleCycle.Model.Campaigns;
using SaleCycle.Model.Security;

namespace BuildCouchObjects
{
    class Program
    {

        public static CouchbaseClient _client;
        public static CouchbaseClientConfiguration _config;

        public static CouchbaseClient _client1;
        public static CouchbaseClient _client2;

        static void Main(string[] args)
        {

            InitCouchbase();
            ClientStatusCheck(_client);

            //var docFromDefault =  GetDoc<Affiliate>(_client1, "affiliate_1010");
            //var docFromIntegration =  GetDoc<Campaign>(_client2, "campaign_1101");
           
      
            Console.WriteLine("");
            Console.WriteLine("Done!");
            Console.ReadLine();
        }

        private static void InitCouchbase()
        {
            //_config = new CouchbaseClientConfiguration();
            //_config.Bucket = "requestlogging";
            //_config.Urls.Add(new Uri("http://172.16.209.26/pools"));
            //_config.Urls.Add(new Uri("http://172.16.209.44/pools"));
            //_config.Urls.Add(new Uri("http://172.16.209.72/pools"));
       

            // Test Multiple Buckets
            //var bucketASection = (CouchbaseClientSection)ConfigurationManager.GetSection("couchbase/bucket-a");
            //_client1 = new CouchbaseClient(bucketASection);     // default

            //var bucketBSection = (CouchbaseClientSection)ConfigurationManager.GetSection("couchbase/bucket-b");
            //_client2 = new CouchbaseClient(bucketBSection);   // requestlogging

            var bucketASection = (CouchbaseClientSection)ConfigurationManager.GetSection("couchbase/bucket-a");
            _client = new CouchbaseClient(bucketASection);      // default

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                ContractResolver = new ContractResolver(),
                Converters = new List<JsonConverter> { new StringEnumConverter() },
                Formatting = Formatting.None,
                NullValueHandling = NullValueHandling.Include,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Auto,
            };
           
        }

        private static void ClientStatusCheck(CouchbaseClient couchClient)
        {
            var clientStates = new Dictionary<string, Tuple<ClientStatus, bool>>();
            var totalCount = 0;

            var view = couchClient.GetView("client", "by_id").Reduce(false);
            foreach (var row in view)
            {
                totalCount++;

                var client = GetDoc<LegacyClient>(_client, row.ItemId);

                var statusVals = new Tuple<ClientStatus, bool>(client.Status, client.Enabled);
                clientStates.Add(row.ItemId, statusVals);
            }

            Console.WriteLine("==================");
            Console.WriteLine("Overall Comparison");
            Console.WriteLine("==================");

            var active_enabled= clientStates.Count(x => x.Value.Item1 == ClientStatus.Active && x.Value.Item2 == true);
            var active_disabled = clientStates.Count(x => x.Value.Item1 == ClientStatus.Active && x.Value.Item2 == false);
            Console.WriteLine(string.Format("ClientStatus.Active & Enabled: {0}", active_enabled));
            Console.WriteLine(string.Format("ClientStatus.Active & Disabled: {0}", active_disabled));
            
            var disabled_enabled = clientStates.Count(x => x.Value.Item1 == ClientStatus.Disabled && x.Value.Item2 == true);
            var disabled_disabled = clientStates.Count(x => x.Value.Item1 == ClientStatus.Disabled && x.Value.Item2 == false);
            Console.WriteLine(string.Format("ClientStatus.Disabled & Enabled: {0}", disabled_enabled));
            Console.WriteLine(string.Format("ClientStatus.Disabled & Disabled: {0}", disabled_disabled));

            var pending_enabled = clientStates.Count(x => x.Value.Item1 == ClientStatus.Pending && x.Value.Item2 == true);
            var pending_disabled = clientStates.Count(x => x.Value.Item1 == ClientStatus.Pending && x.Value.Item2 == false);
            Console.WriteLine(string.Format("ClientStatus.Pending & Enabled: {0}", pending_enabled));
            Console.WriteLine(string.Format("ClientStatus.Pending & Disabled: {0}", pending_disabled));

            var testing_enabled = clientStates.Count(x => x.Value.Item1 == ClientStatus.Testing && x.Value.Item2 == true);
            var testing_disabled = clientStates.Count(x => x.Value.Item1 == ClientStatus.Testing && x.Value.Item2 == false);
            Console.WriteLine(string.Format("ClientStatus.Testing & Enabled: {0}", testing_enabled));
            Console.WriteLine(string.Format("ClientStatus.Testing & Disabled: {0}", testing_disabled));

            Console.WriteLine(string.Format("Total: {0}", totalCount));
            Console.WriteLine();

            Console.WriteLine("========================");
            Console.WriteLine("Status Active : Disabled");
            Console.WriteLine("========================");
            var items = clientStates.Where(x => x.Value.Item1 == ClientStatus.Active && x.Value.Item2 == false);
            foreach (var item in items)
            {
                Console.WriteLine(string.Format("{0} : {1} : {2}", item.Key, item.Value.Item1, item.Value.Item2));                
            }
            Console.WriteLine();

            Console.WriteLine("=========================");
            Console.WriteLine("Status Disabled : Enabled");
            Console.WriteLine("=========================");
            items = clientStates.Where(x => x.Value.Item1 == ClientStatus.Disabled && x.Value.Item2 == true);
            foreach (var item in items)
            {
                Console.WriteLine(string.Format("{0} : {1} : {2}", item.Key, item.Value.Item1, item.Value.Item2));                
            }
            Console.WriteLine();

            Console.WriteLine("========================");
            Console.WriteLine("Status Pending : Enabled");
            Console.WriteLine("========================");
            items = clientStates.Where(x => x.Value.Item1 == ClientStatus.Pending && x.Value.Item2 == true);
            foreach (var item in items)
            {
                Console.WriteLine(string.Format("{0} : {1} : {2}", item.Key, item.Value.Item1, item.Value.Item2));
            }
            Console.WriteLine();
            
            Console.WriteLine("========================");
            Console.WriteLine("Status Pending : Enabled");
            Console.WriteLine("========================");
            items = clientStates.Where(x => x.Value.Item1 == ClientStatus.Pending && x.Value.Item2 == true);
            foreach (var item in items)
            {
                Console.WriteLine(string.Format("{0} : {1} : {2}", item.Key, item.Value.Item1, item.Value.Item2));
            }
            Console.WriteLine();

            Console.WriteLine("=========================");
            Console.WriteLine("Status Pending : Disabled");
            Console.WriteLine("=========================");
            items = clientStates.Where(x => x.Value.Item1 == ClientStatus.Pending && x.Value.Item2 == false);
            foreach (var item in items)
            {
                Console.WriteLine(string.Format("{0} : {1} : {2}", item.Key, item.Value.Item1, item.Value.Item2));
            }
            Console.WriteLine();

            Console.WriteLine("========================");
            Console.WriteLine("Status Testing : Enabled");
            Console.WriteLine("========================");
            items = clientStates.Where(x => x.Value.Item1 == ClientStatus.Testing && x.Value.Item2 == true);
            foreach (var item in items)
            {
                Console.WriteLine(string.Format("{0} : {1} : {2}", item.Key, item.Value.Item1, item.Value.Item2));
            }
            Console.WriteLine();

            Console.WriteLine("=========================");
            Console.WriteLine("Status Testing : Disabled");
            Console.WriteLine("=========================");
            items = clientStates.Where(x => x.Value.Item1 == ClientStatus.Testing && x.Value.Item2 == false);
            foreach (var item in items)
            {
                Console.WriteLine(string.Format("{0} : {1} : {2}", item.Key, item.Value.Item1, item.Value.Item2));
            }
            Console.WriteLine();

            Console.WriteLine();
        }


        private static void DeleteRequestLogs(CouchbaseClient client)
        {
            var view = client.GetView("requestlogging", "by_id").Reduce(false);
            foreach (var row in view)
            {
                var success = client.Remove(row.ItemId);
                if (!success)
                {
                    Console.WriteLine(@"Log  {0} - Error Deleting", row.ItemId);
                }
                else
                {
                    Console.WriteLine(@"Log {0} - Deleted", row.ItemId);
                }
            }

            Console.WriteLine(@"Logs Deleted");
        }

        private static void SeedSession(string source, int clientId, string sessionId, int campaignId, string language)
        {
            var session = JsonConvert.DeserializeObject<Session>(source);
            session.ClientId = clientId;
            session.Language = language;
            session.Id = sessionId;
            session.Status = SessionStatus.Active;
            session.Abandonment.CampaignId = campaignId;
            session.Abandonment.Status = AbandonmentStatus.Pending;

            var key = string.Format("session_1000_{0}", sessionId);
            var doc = JsonConvert.SerializeObject(session);
            
            AddDoc(key, doc); 
        }

        private static void DeleteJobs()
        {
            var view = _client.GetView("scheduling", "jobs_by_status_scheduledFor").Reduce(false);
            foreach (var row in view)
            {
                var success = _client.Remove(row.ItemId);
                if (!success)
                {
                    Console.WriteLine(@"Job {0} - Error Deleting", row.ItemId);
                }
                else
                {
                    Console.WriteLine(@"Job {0} - Deleted", row.ItemId);
                }
            }

            Console.WriteLine(@"Jobs Deleted");
        }

        public static void ReQueueJobs()
        {
            var view = _client.GetView("scheduling", "jobs_by_status_scheduledFor").Reduce(false);
            foreach (var row in view)
            {
                var job = GetDoc<Job>(_client, row.ItemId);
                job.Status = JobStatus.Queued;
                var doc = JsonConvert.SerializeObject(job);
                AddDoc(row.ItemId, doc);
            }
        }

        private static void AddDoc(string key, string doc)
        {
            var result = _client.Store(StoreMode.Set, key, doc);
            if (result)
            {
                Console.WriteLine(@"Saved Doc {0}", key);
            }
        }

        private static void GetDoc(CouchbaseClient client, string key)
        {            
            var result = client.ExecuteGet(key);         
        }

        private static T GetDoc<T>(CouchbaseClient client, string key)
        {
            var doc = client.Get<String>(key);
            var model = JsonConvert.DeserializeObject<T>(doc);
            return model;
        }

        private static void DeleteApiRequests()
        {
            int i;
            for (i = 1000; i < 2000; i++)
            {
                var key = string.Format("apirequest_{0}", i.ToString());
                var success = _client.Remove(key);
                if (!success)
                {
                    Console.WriteLine(@"{0} - Error Deleting", key);
                }
                else
                {
                    Console.WriteLine(@"{0} - Deleted", key);
                }
            }
            Console.WriteLine(@"{0} Jobs Deleted", i.ToString());
        }
    }
}
