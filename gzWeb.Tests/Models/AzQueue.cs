using NUnit.Framework;
using System.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json.Linq;

namespace gzWeb.Tests.Models
{
    /// <summary>
    /// Summary description for AzQueue
    /// </summary>
    [TestFixture]
    public class AzQueue {
        private CloudQueue queue = null;

        [OneTimeSetUp]
        public void QueueInitialize()
        {
            var queueAzureConnString = ConfigurationManager.ConnectionStrings["QueueAzureConnString"].ConnectionString;
            var queueNameString = ConfigurationManager.ConnectionStrings["QueueAzureConnString"].ConnectionString;
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(queueAzureConnString);
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            queue = queueClient.GetQueueReference(queueNameString);
            queue.CreateIfNotExists();
        }

        [Test]
        public void AddMessage()
        {
            JArray invBalanceIdsArray = new JArray();
            CloudQueueMessage qmsg = new CloudQueueMessage("");
            queue.AddMessage(qmsg);
        }
    }
}
