using System;
using NUnit.Framework;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using gzDAL.DTO;
using gzDAL.ModelUtil;
using Microsoft.Data.OData.Query.SemanticAst;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace gzWeb.Tests.Models
{
    /// <summary>
    /// Summary description for AzQueue
    /// </summary>
    [TestFixture]
    public class AzQueue
    {
        private CloudQueue queue = null;

        [OneTimeSetUp]
        public async Task QueueInitialize() {
            var queueAzureConnString = ConfigurationManager.AppSettings["QueueAzureConnString"];
            var queueName = ConfigurationManager.AppSettings["QueueName"];
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(queueAzureConnString);
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            queue = queueClient.GetQueueReference(queueName);
            queue.CreateIfNotExists();
            await queue.ClearAsync();
        }

        [Test]
        public async Task SetGetMessagingCounters()
        {
            // Save initial counter
            var initCnt = await GetQueueCountAsync();

            // Create object, serialize, add to queue
            var newBonusReq = CreateBonusReq();
            var bonusReqJson = GetaBonusReqJson(newBonusReq);
            await AddQueueMsgAsync(bonusReqJson);

            // Check counter + 1
            var qmsg = await queue.GetMessageAsync();
            int qcnt = await GetQueueCountAsync();
            Assert.AreEqual(qcnt, initCnt + 1);

            // Back to initial counter
            queue.DeleteMessage(qmsg);
            qcnt = await GetQueueCountAsync();
            Assert.AreEqual(qcnt, initCnt);

            // Retrieved msg json is same as sent
            var retrievedJson = qmsg.AsString;

            // Retrieved deserialized object remains the same
            var retrievedBonusReq = new BonusReq();
            JsonConvert.PopulateObject(retrievedJson, retrievedBonusReq);
            Console.WriteLine("retrievedJson:");
            Console.WriteLine(retrievedJson);

            Assert.AreEqual(bonusReqJson, retrievedJson);
        }

        private async Task AddQueueMsgAsync(string bonusJson) {

            CloudQueueMessage qmsg = new CloudQueueMessage(bonusJson);
            await queue.AddMessageAsync(qmsg);
        }

        private static string GetaBonusReqJson(BonusReq newBonusReq) {
            string bonusReqJson = JsonConvert.SerializeObject(newBonusReq);
            Console.WriteLine($"bonusReqJson:{bonusReqJson}");

            return bonusReqJson;
        }

        private static BonusReq CreateBonusReq() {

            var newBonusReq = new BonusReq() {
                AdminEmailRecipients = new[] {"mario@greenzorro.com", "salem8@gmail.com"},
                Amount = 0.10M,
                GmUserId = 626499,
                InvBalIds = new int[] {
                    880,
                    886,
                    892
                },
                UserEmail = "salem8@gmail.com",
                UserFirstName = "George",
            };
            return newBonusReq;
        }

        private async Task<int> GetQueueCountAsync() {

            // Fetch the queue attributes.
            await queue.FetchAttributesAsync();

            // Retrieve the cached approximate message count.
            int cachedMessageCount = queue.ApproximateMessageCount.GetValueOrDefault();

            return cachedMessageCount;
        }
    }
}
