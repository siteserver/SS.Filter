using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using Dapper;
using Dapper.Contrib.Extensions;
using SiteServer.Plugin;
using SS.Filter.Model;

namespace SS.Filter.Provider
{
    public class TagDao
    {
        public const string TableName = "ss_filter_tag";

        public static List<TableColumn> Columns => new List<TableColumn>
        {
            new TableColumn
            {
                AttributeName = nameof(TagInfo.Id),
                DataType = DataType.Integer
            },
            new TableColumn
            {
                AttributeName = nameof(TagInfo.FieldId),
                DataType = DataType.Integer
            },
            new TableColumn
            {
                AttributeName = nameof(TagInfo.ParentId),
                DataType = DataType.Integer
            },
            new TableColumn
            {
                AttributeName = nameof(TagInfo.Taxis),
                DataType = DataType.Integer
            },
            new TableColumn
            {
                AttributeName = nameof(TagInfo.Title),
                DataType = DataType.VarChar,
                DataLength = 200
            }
        };

        private readonly string _connectionString;
        private readonly IDataApi _helper;
        private readonly ObjectCache _cache = MemoryCache.Default;
        private readonly CacheItemPolicy _policy = new CacheItemPolicy();
        private const string CacheNameTagInfoList = nameof(CacheNameTagInfoList);

        public TagDao(string connectionString, IDataApi helper)
        {
            _connectionString = connectionString;
            _helper = helper;
        }

        private List<TagInfo> CacheGetTagInfoList(int fieldId, int parentId)
        {
            return _cache[$"{CacheNameTagInfoList}.{fieldId}.{parentId}"] as List<TagInfo>;
        }

        private void CacheSetTagInfoList(int fieldId, int parentId, List<TagInfo> list)
        {
            _cache.Set($"{CacheNameTagInfoList}.{fieldId}.{parentId}", list, _policy);
        }

        private void CacheRemoveTagInfoList(int fieldId, int parentId)
        {
            _cache.Remove($"{CacheNameTagInfoList}.{fieldId}.{parentId}");
        }

        public void Insert(int fieldId, int parentId, TagInfo tagInfo)
        {
            if (tagInfo == null) return;
            tagInfo.FieldId = fieldId;
            tagInfo.ParentId = parentId;
            CacheRemoveTagInfoList(fieldId, parentId);

            using (var connection = _helper.GetConnection(_connectionString))
            {
                tagInfo.Id = (int)connection.Insert(tagInfo);
            }
        }

        public void Update(int fieldId, int parentId, TagInfo tagInfo)
        {
            if (tagInfo == null) return;
            tagInfo.FieldId = fieldId;
            tagInfo.ParentId = parentId;
            CacheRemoveTagInfoList(fieldId, parentId);

            using (var connection = _helper.GetConnection(_connectionString))
            {
                connection.Update(tagInfo);
            }
        }

        public void Delete(int fieldId, int parentId, List<TagInfo> tagInfoList)
        {
            CacheRemoveTagInfoList(fieldId, parentId);

            var idList = tagInfoList.Select(x => x.Id).ToList();

            var sqlString = $"DELETE FROM {TableName} WHERE {nameof(TagInfo.ParentId)} IN ({string.Join(",", idList)})";

            _helper.ExecuteNonQuery(_connectionString, sqlString);

            using (var connection = _helper.GetConnection(_connectionString))
            {
                connection.Delete(tagInfoList);
            }
        }

        public List<TagInfo> GetTagInfoList(int fieldId, int parentId)
        {
            List<TagInfo> list = CacheGetTagInfoList(fieldId, parentId);
            if (list != null) return list;

            var sqlString = $"SELECT * FROM {TableName} WHERE {nameof(TagInfo.FieldId)} = @{nameof(TagInfo.FieldId)} AND {nameof(TagInfo.ParentId)} = @{nameof(TagInfo.ParentId)} ORDER BY {nameof(TagInfo.Taxis)}, {nameof(TagInfo.Id)}";

            using (var connection = _helper.GetConnection(_connectionString))
            {
                list = connection.Query<TagInfo>(sqlString, new TagInfo
                {
                    FieldId = fieldId,
                    ParentId = parentId
                }).ToList();
            }

            CacheSetTagInfoList(fieldId, parentId, list);

            return list;
        }

        public TagInfo GetTagInfo(int tagId)
        {
            var sqlString = $"SELECT * FROM {TableName} WHERE {nameof(TagInfo.Id)} = @{nameof(TagInfo.Id)}";

            using (var connection = _helper.GetConnection(_connectionString))
            {
                return connection.QuerySingleOrDefault<TagInfo>(sqlString, new TagInfo
                {
                    Id = tagId
                });
            }
        }
    }
}
