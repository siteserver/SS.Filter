using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using Dapper;
using Dapper.Contrib.Extensions;
using SiteServer.Plugin;
using SS.Filter.Model;

namespace SS.Filter.Provider
{
    public static class FieldDao
    {
        public const string TableName = "ss_filter_field";

        public static List<TableColumn> Columns => new List<TableColumn>
        {
            new TableColumn
            {
                AttributeName = nameof(FieldInfo.Id),
                DataType = DataType.Integer,
                IsPrimaryKey = true,
                IsIdentity = true
            },
            new TableColumn
            {
                AttributeName = nameof(FieldInfo.SiteId),
                DataType = DataType.Integer
            },
            new TableColumn
            {
                AttributeName = nameof(FieldInfo.Taxis),
                DataType = DataType.Integer
            },
            new TableColumn
            {
                AttributeName = nameof(FieldInfo.Title),
                DataType = DataType.VarChar,
                DataLength = 200
            },
            new TableColumn
            {
                AttributeName = nameof(FieldInfo.InputType),
                DataType = DataType.VarChar,
                DataLength = 50
            }
        };

        private static readonly ObjectCache Cache = MemoryCache.Default;
        public static readonly CacheItemPolicy Policy = new CacheItemPolicy();
        private const string CacheNameFieldInfoList = nameof(CacheNameFieldInfoList);

        private static List<FieldInfo> CacheGetFieldInfoList(int siteId)
        {
            return Cache[CacheNameFieldInfoList + siteId] as List<FieldInfo>;
        }

        private static void CacheSetFieldInfoList(int siteId, List<FieldInfo> list)
        {
            Cache.Set(CacheNameFieldInfoList + siteId, list, Policy);
        }

        private static void CacheRemoveFieldInfoList(int siteId)
        {
            Cache.Remove(CacheNameFieldInfoList + siteId);
        }

        public static void Insert(int siteId, FieldInfo fieldInfo)
        {
            if (fieldInfo == null) return;
            fieldInfo.SiteId = siteId;
            CacheRemoveFieldInfoList(siteId);

            using (var connection = Context.DatabaseApi.GetConnection(Context.ConnectionString))
            {
                fieldInfo.Id = (int)connection.Insert(fieldInfo);
            }
        }

        public static void Update(int siteId, FieldInfo fieldInfo)
        {
            if (fieldInfo == null) return;
            fieldInfo.SiteId = siteId;
            CacheRemoveFieldInfoList(siteId);

            using (var connection = Context.DatabaseApi.GetConnection(Context.ConnectionString))
            {
                connection.Update(fieldInfo);
            }
        }

        public static bool Delete(int siteId, int fieldId)
        {
            CacheRemoveFieldInfoList(siteId);

            using (var connection = Context.DatabaseApi.GetConnection(Context.ConnectionString))
            {
                return connection.Delete(new FieldInfo() { Id = fieldId });
            }
        }

        public static List<FieldInfo> GetFieldInfoList(int siteId)
        {
            List<FieldInfo> list = CacheGetFieldInfoList(siteId);
            if (list != null) return list;

            var sqlString = $"SELECT * FROM {TableName} WHERE {nameof(FieldInfo.SiteId)} = @{nameof(FieldInfo.SiteId)} ORDER BY {nameof(FieldInfo.Taxis)}, {nameof(FieldInfo.Id)}";

            using (var connection = Context.DatabaseApi.GetConnection(Context.ConnectionString))
            {
                list = connection.Query<FieldInfo>(sqlString, new FieldInfo
                {
                    SiteId = siteId
                }).ToList();
            }

            CacheSetFieldInfoList(siteId, list);

            return list;
        }

        public static FieldInfo GetFieldInfo(int id)
        {
            var sqlString = $"SELECT * FROM {TableName} WHERE {nameof(FieldInfo.Id)} = @{nameof(FieldInfo.Id)}";

            using (var connection = Context.DatabaseApi.GetConnection(Context.ConnectionString))
            {
                return connection.QuerySingleOrDefault<FieldInfo>(sqlString, new FieldInfo {Id = id});
            }
        } 
    }
}
