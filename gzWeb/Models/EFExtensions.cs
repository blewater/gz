using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

/// <summary>
/// https://gist.github.com/codedecay/dd7863994e243065f236/5dfe4b021473713dbd87e31e846bdf7eb3461842
/// </summary>
namespace EFExtensions {
    public static class EFExtensions {
        public static EntityOp<TEntity> Upsert<TEntity>(this DbContext context, IEnumerable<TEntity> entity) where TEntity : class {
            return new UpsertOp<TEntity>(context, entity);
        }
    }

    public abstract class EntityOp<TEntity, TRet> {
        public readonly DbContext Context;
        public readonly IEnumerable<TEntity> EntityList;
        public readonly string TableName;

        private readonly List<string> keyNames = new List<string>();
        public IEnumerable<string> KeyNames { get { return keyNames; } }

        private readonly List<string> excludeProperties = new List<string>();

        private static string GetMemberName<T>(Expression<Func<TEntity, T>> selectMemberLambda) {
            var member = selectMemberLambda.Body as MemberExpression;
            if (member == null) {
                throw new ArgumentException("The parameter selectMemberLambda must be a member accessing labda such as x => x.Id", "selectMemberLambda");
            }
            return member.Member.Name;
        }

        public EntityOp(DbContext context, IEnumerable<TEntity> entityList) {
            Context = context;
            EntityList = entityList;

            object[] mappingAttrs = typeof(TEntity).GetCustomAttributes(typeof(TableAttribute), false);
            TableAttribute tableAttr = null;
            if (mappingAttrs.Length > 0) {
                tableAttr = mappingAttrs[0] as TableAttribute;
            }

            if (tableAttr == null) {
                throw new ArgumentException("TEntity is missing TableAttribute", "entityList");
            }

            TableName = tableAttr.Name;
        }

        public abstract TRet Execute();
        public void Run() {
            Execute();
        }

        public EntityOp<TEntity, TRet> Key<TKey>(Expression<Func<TEntity, TKey>> selectKey) {
            keyNames.Add(GetMemberName(selectKey));
            return this;
        }

        public EntityOp<TEntity, TRet> ExcludeField<TField>(Expression<Func<TEntity, TField>> selectField) {
            excludeProperties.Add(GetMemberName(selectField));
            return this;
        }

        public IEnumerable<PropertyInfo> ColumnProperties {
            get {
                return typeof(TEntity).GetProperties().Where(pr => !excludeProperties.Contains(pr.Name));
            }
        }
    }

    public abstract class EntityOp<TEntity> : EntityOp<TEntity, int> {
        public EntityOp(DbContext context, IEnumerable<TEntity> entityList) : base(context, entityList) { }

        public sealed override int Execute() {
            ExecuteNoRet();
            return 0;
        }

        protected abstract void ExecuteNoRet();
    }

    public class UpsertOp<TEntity> : EntityOp<TEntity> {
        public UpsertOp(DbContext context, IEnumerable<TEntity> entityList) : base(context, entityList) { }

        protected override void ExecuteNoRet() {
            StringBuilder sql = new StringBuilder();

            int notNullFields = 0;
            var valueKeyList = new List<string>();
            var columnList = new List<string>();

            var columnProperties = ColumnProperties.ToArray();
            foreach (var p in columnProperties) {
                columnList.Add(p.Name);
                valueKeyList.Add("{" + (notNullFields++) + "}");
            }
            var columns = columnList.ToArray();

            sql.Append("merge into ");
            sql.Append(TableName);
            sql.Append(" as T ");

            sql.Append("using (values (");
            sql.Append(string.Join(",", valueKeyList.ToArray()));
            sql.Append(")) as S (");
            sql.Append(string.Join(",", columns));
            sql.Append(") ");

            sql.Append("on (");
            var mergeCond = string.Join(" and ", KeyNames.Select(kn => "T." + kn + "=S." + kn));
            sql.Append(mergeCond);
            sql.Append(") ");

            sql.Append("when matched then update set ");
            sql.Append(string.Join(",", columns.Select(c => "T." + c + "=S." + c).ToArray()));

            sql.Append(" when not matched then insert (");
            sql.Append(string.Join(",", columns));
            sql.Append(") values (S.");
            sql.Append(string.Join(",S.", columns));
            sql.Append(");");
            var command = sql.ToString();

            foreach (var entity in EntityList) {
                var valueList = new List<object>();

                foreach (var p in columnProperties) {
                    var val = p.GetValue(entity, null);
                    valueList.Add(val ?? DBNull.Value);
                }

                Context.Database.ExecuteSqlCommand(command, valueList.ToArray());
            }
        }
    }
}