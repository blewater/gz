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

        private static bool SuspendRetryExecutionStrategy
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

        /// <summary>
        /// 
        /// Suspend Retry Execution Strategy handling method:
        /// 
        /// 1. That does not leak the SuspendRetryExecutionStrategy setting change.
        /// 
        /// 2. Encapsulate the same opening and closing db user transaction and final context.SaveChanges() code in this method.
        /// 
        /// </summary>
        /// <param name="db">The db context that will be for all enclosed within a transaction database update operations</param>
        /// <param name="dbOperationsAction">The delegate with any entity updates operations performed using the db context</param>
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
