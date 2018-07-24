using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using Dapper;
using Dapper.Contrib.Extensions;
using SiteServer.Plugin;
using SS.Filter.Model;

namespace SS.Filter.Provider
{
    public class FieldDao
    {
        public const string TableName = "ss_filter_field";

        public static List<TableColumn> Columns => new List<TableColumn>
        {
            new TableColumn
            {
                AttributeName = nameof(FieldInfo.Id),
                DataType = DataType.Integer
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

        private readonly string _connectionString;
        private readonly IDatabaseApi _helper;

        private readonly ObjectCache _cache = MemoryCache.Default;
        private readonly CacheItemPolicy _policy = new CacheItemPolicy();
        private const string CacheNameFieldInfoList = nameof(CacheNameFieldInfoList);

        public FieldDao(string connectionString, IDatabaseApi helper)
        {
            _connectionString = connectionString;
            _helper = helper;
        }

        private List<FieldInfo> CacheGetFieldInfoList(int siteId)
        {
            return _cache[CacheNameFieldInfoList + siteId] as List<FieldInfo>;
        }

        private void CacheSetFieldInfoList(int siteId, List<FieldInfo> list)
        {
            _cache.Set(CacheNameFieldInfoList + siteId, list, _policy);
        }

        private void CacheRemoveFieldInfoList(int siteId)
        {
            _cache.Remove(CacheNameFieldInfoList + siteId);
        }

        public void Insert(int siteId, FieldInfo fieldInfo)
        {
            if (fieldInfo == null) return;
            fieldInfo.SiteId = siteId;
            CacheRemoveFieldInfoList(siteId);

            using (var connection = _helper.GetConnection(_connectionString))
            {
                fieldInfo.Id = (int)connection.Insert(fieldInfo);
            }
        }

        public void Update(int siteId, FieldInfo fieldInfo)
        {
            if (fieldInfo == null) return;
            fieldInfo.SiteId = siteId;
            CacheRemoveFieldInfoList(siteId);

            using (var connection = _helper.GetConnection(_connectionString))
            {
                connection.Update(fieldInfo);
            }
        }

        public bool Delete(int siteId, int fieldId)
        {
            CacheRemoveFieldInfoList(siteId);

            using (var connection = _helper.GetConnection(_connectionString))
            {
                return connection.Delete(new FieldInfo() { Id = fieldId });
            }
        }

        public List<FieldInfo> GetFieldInfoList(int siteId)
        {
            List<FieldInfo> list = CacheGetFieldInfoList(siteId);
            if (list != null) return list;

            var sqlString = $"SELECT * FROM {TableName} WHERE {nameof(FieldInfo.SiteId)} = @{nameof(FieldInfo.SiteId)} ORDER BY {nameof(FieldInfo.Taxis)}, {nameof(FieldInfo.Id)}";

            using (var connection = _helper.GetConnection(_connectionString))
            {
                list = connection.Query<FieldInfo>(sqlString, new FieldInfo
                {
                    SiteId = siteId
                }).ToList();
            }

            CacheSetFieldInfoList(siteId, list);

            return list;
        }

        public FieldInfo GetFieldInfo(int id)
        {
            var sqlString = $"SELECT * FROM {TableName} WHERE {nameof(FieldInfo.Id)} = @{nameof(FieldInfo.Id)}";

            using (var connection = _helper.GetConnection(_connectionString))
            {
                return connection.QuerySingleOrDefault<FieldInfo>(sqlString, new FieldInfo {Id = id});
            }
        } 
    }
}
