using gzDAL.Models;
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

        public static bool SuspendRetryExecutionStrategy
        {
            get { return (bool?)CallContext.LogicalGetData("SuspendExecutionStrategy") ?? false; }
            set { CallContext.LogicalSetData("SuspendExecutionStrategy", value); }
        }

        public ConnRetryConf()
        {
            SetExecutionStrategy("System.Data.SqlClient",
                                 () => SuspendRetryExecutionStrategy
                                               ? (IDbExecutionStrategy) new DefaultExecutionStrategy()
                                               : new SqlAzureExecutionStrategy(2, TimeSpan.FromSeconds(10)));
        }

        public static void TransactWithRetryStrategy(ApplicationDbContext db, Action dbOperationsAction) {

            try {
                ConnRetryConf.SuspendRetryExecutionStrategy = true;

                var executionStrategy = new SqlAzureExecutionStrategy(2, TimeSpan.FromSeconds(10));
                executionStrategy
                    .Execute(() => {
                        using (var dbContextTransaction = db.Database.BeginTransaction()) {

                            dbOperationsAction();

                            db.SaveChanges();
                            dbContextTransaction.Commit();
                        }
                    });

                // Don't leak suspend setting
            } finally {

                ConnRetryConf.SuspendRetryExecutionStrategy = false;
            }
        }
    }
}
