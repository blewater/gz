using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.SqlServer;
using System.Runtime.Remoting.Messaging;


namespace gzDAL.Conf {
    /// <summary>
    /// 
    /// Enable 
    /// Connection Resiliency / Retry Logic (EF6 onwards)
    /// https://msdn.microsoft.com/en-us/data/dn456835
    /// 
    /// </summary>
    public class ConnRetryConf : DbConfiguration {

        public static bool SuspendExecutionStrategy
        {
            get { return (bool?)CallContext.LogicalGetData("SuspendExecutionStrategy") ?? false; }
            set { CallContext.LogicalSetData("SuspendExecutionStrategy", value); }
        }

        public ConnRetryConf()
        {
            SetExecutionStrategy("System.Data.SqlClient",
                                 () => SuspendExecutionStrategy
                                               ? (IDbExecutionStrategy) new DefaultExecutionStrategy()
                                               : new SqlAzureExecutionStrategy(2, TimeSpan.FromSeconds(10)));
        }
    }
}
