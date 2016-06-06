using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace gzDAL.Models {

    /// <summary>
    /// 
    /// EF Debugging extensions
    /// 
    /// </summary>
    public static class DbSetExtensions {

        private static readonly string TempSqlLogPath = Path.GetTempPath() + "sqlLogFile.log";
        private static StreamWriter _sqlLogFile = null;

        /// <summary>
        /// 
        /// Static constructor
        /// 
        /// </summary>
        static DbSetExtensions() {

#if DEBUG
            _sqlLogFile = new StreamWriter(TempSqlLogPath);
#endif

        }

        /// <summary>
        /// 
        /// A debugging AddOrUpdate method.
        /// 
        /// http://stackoverflow.com/questions/31162576/entity-framework-add-if-not-exist-without-update
        /// 
        /// </summary>
        public static T AddOrUpdate<T>
            (this DbSet<T> dbSet, T entity, ApplicationDbContext db, Expression<Func<T, bool>> predicate = null) where T : class, new() 
        {

            var exists = predicate != null ? dbSet.Any(predicate) : dbSet.Any();

            if (exists) {
                dbSet.Attach(entity);
                db.Entry(entity).State = EntityState.Modified;
            }
            else {

                dbSet.Add(entity);
            }

            return entity;
        }

        public static void Log(string component, string message) {

            var text = "Component: " + component + " Message: " + message;

            Console.WriteLine(text);

            _sqlLogFile.WriteLine(text);

            _sqlLogFile.FlushAsync().Wait();

        }
    }

}