using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using Dapper;
using Dapper.Contrib.Extensions;
using SiteServer.Plugin;
using SS.Filter.Model;

namespace SS.Filter.Provider
{
    public static class TagDao
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

        private static readonly ObjectCache Cache = MemoryCache.Default;
        private static readonly CacheItemPolicy Policy = new CacheItemPolicy();
        private const string CacheNameTagInfoList = nameof(CacheNameTagInfoList);

        private static List<TagInfo> CacheGetTagInfoList(int fieldId, int parentId)
        {
            return Cache[$"{CacheNameTagInfoList}.{fieldId}.{parentId}"] as List<TagInfo>;
        }

        private static void CacheSetTagInfoList(int fieldId, int parentId, List<TagInfo> list)
        {
            Cache.Set($"{CacheNameTagInfoList}.{fieldId}.{parentId}", list, Policy);
        }

        private static void CacheRemoveTagInfoList(int fieldId, int parentId)
        {
            Cache.Remove($"{CacheNameTagInfoList}.{fieldId}.{parentId}");
        }

        public static void Insert(int fieldId, int parentId, TagInfo tagInfo)
        {
            if (tagInfo == null) return;
            tagInfo.FieldId = fieldId;
            tagInfo.ParentId = parentId;
            CacheRemoveTagInfoList(fieldId, parentId);

            using (var connection = Context.DatabaseApi.GetConnection(Context.ConnectionString))
            {
                tagInfo.Id = (int)connection.Insert(tagInfo);
            }
        }

        public static void Update(int fieldId, int parentId, TagInfo tagInfo)
        {
            if (tagInfo == null) return;
            tagInfo.FieldId = fieldId;
            tagInfo.ParentId = parentId;
            CacheRemoveTagInfoList(fieldId, parentId);

            using (var connection = Context.DatabaseApi.GetConnection(Context.ConnectionString))
            {
                connection.Update(tagInfo);
            }
        }

        public static void Delete(int fieldId, int parentId, List<TagInfo> tagInfoList)
        {
            CacheRemoveTagInfoList(fieldId, parentId);

            var idList = tagInfoList.Select(x => x.Id).ToList();

            var sqlString = $"DELETE FROM {TableName} WHERE {nameof(TagInfo.ParentId)} IN ({string.Join(",", idList)})";

            Context.DatabaseApi.ExecuteNonQuery(Context.ConnectionString, sqlString);

            using (var connection = Context.DatabaseApi.GetConnection(Context.ConnectionString))
            {
                connection.Delete(tagInfoList);
            }
        }

        public static List<TagInfo> GetTagInfoList(int fieldId, int parentId)
        {
            List<TagInfo> list = CacheGetTagInfoList(fieldId, parentId);
            if (list != null) return list;

            var sqlString = $"SELECT * FROM {TableName} WHERE {nameof(TagInfo.FieldId)} = @{nameof(TagInfo.FieldId)} AND {nameof(TagInfo.ParentId)} = @{nameof(TagInfo.ParentId)} ORDER BY {nameof(TagInfo.Taxis)}, {nameof(TagInfo.Id)}";

            using (var connection = Context.DatabaseApi.GetConnection(Context.ConnectionString))
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

        public static TagInfo GetTagInfo(int tagId)
        {
            var sqlString = $"SELECT * FROM {TableName} WHERE {nameof(TagInfo.Id)} = @{nameof(TagInfo.Id)}";

            using (var connection = Context.DatabaseApi.GetConnection(Context.ConnectionString))
            {
                return connection.QuerySingleOrDefault<TagInfo>(sqlString, new TagInfo
                {
                    Id = tagId
                });
            }
        }
    }
}
