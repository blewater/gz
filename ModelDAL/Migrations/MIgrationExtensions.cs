using System.Data.Entity.Migrations.Infrastructure;
using System.Data.Entity.Migrations.Model;

namespace gzDAL.Migrations
{
    public static class MigrationExtensions
    {
        public static void DeleteDefaultConstraint(this IDbMigration migration, string tableName, string colName, bool suppressTransaction = false)
        {
            var sql = new SqlOperation(
                $"DECLARE @SQL varchar(1000) " +
                $"SET @SQL='ALTER TABLE {tableName} DROP CONSTRAINT ['+(SELECT name FROM sys.default_constraints WHERE parent_object_id = object_id('{tableName}') AND col_name(parent_object_id, parent_column_id) = '{colName}')+']'; " +
                $"PRINT @SQL; " +
                $"EXEC(@SQL);")
                      {
                              SuppressTransaction = suppressTransaction
                      };
            migration.AddOperation(sql);
        }
    }
}