using System;
using System.Data.Entity;
using System.Data.Entity.SqlServer;


namespace gzDAL.Conf {
    /// <summary>
    /// 
    /// Enable 
    /// Connection Resiliency / Retry Logic (EF6 onwards)
    /// https://msdn.microsoft.com/en-us/data/dn456835
    /// 
    /// </summary>
    public class ConnRetryConf : DbConfiguration {

        public ConnRetryConf()
        {
            SetExecutionStrategy("System.Data.SqlClient"
                , () => new SqlAzureExecutionStrategy(2, TimeSpan.FromSeconds(10)));
        }
    }
}
