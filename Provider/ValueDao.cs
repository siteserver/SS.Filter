using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dapper;
using Dapper.Contrib.Extensions;
using SiteServer.Plugin;
using SS.Filter.Model;

namespace SS.Filter.Provider
{
    public class ValueDao
    {
        public const string TableName = "ss_filter_value";

        public static List<TableColumn> Columns => new List<TableColumn>
        {
            new TableColumn
            {
                AttributeName = nameof(ValueInfo.Id),
                DataType = DataType.Integer
            },
            new TableColumn
            {
                AttributeName = nameof(ValueInfo.SiteId),
                DataType = DataType.Integer
            },
            new TableColumn
            {
                AttributeName = nameof(ValueInfo.ChannelId),
                DataType = DataType.Integer
            },
            new TableColumn
            {
                AttributeName = nameof(ValueInfo.ContentId),
                DataType = DataType.Integer
            },
            new TableColumn
            {
                AttributeName = nameof(ValueInfo.FieldId),
                DataType = DataType.Integer
            },
            new TableColumn
            {
                AttributeName = nameof(ValueInfo.TagId),
                DataType = DataType.Integer
            }
        };

        private readonly string _connectionString;
        private readonly IDataApi _helper;

        public ValueDao(string connectionString, IDataApi helper)
        {
            _connectionString = connectionString;
            _helper = helper;
        }

        public List<int> GetTagIdList(int siteId, int channelId, int contentId, int fieldId)
        {
            if (channelId == 0 || contentId == 0) return new List<int>();

            var sqlString = $"SELECT {nameof(ValueInfo.TagId)} FROM {TableName} WHERE {nameof(ValueInfo.SiteId)} = @{nameof(ValueInfo.SiteId)} AND {nameof(ValueInfo.ChannelId)} = @{nameof(ValueInfo.ChannelId)} AND {nameof(ValueInfo.ContentId)} = @{nameof(ValueInfo.ContentId)} AND {nameof(ValueInfo.FieldId)} = @{nameof(ValueInfo.FieldId)}";

            using (var connection = _helper.GetConnection(_connectionString))
            {
                return connection.Query<int>(sqlString, new ValueInfo
                {
                    SiteId = siteId,
                    ChannelId = channelId,
                    ContentId = contentId,
                    FieldId = fieldId
                }).ToList();
            }
        }

        public List<Tuple<int, int>> GetChannelIdContentIdTupleList(int siteId, int channelId, List<FieldInfo> fieldInfoList)
        {
            if (fieldInfoList == null || fieldInfoList.Count == 0) return new List<Tuple<int, int>>();

            var list = new List<Tuple<int, int>>();

            var sqlList = new List<string>();

            foreach (var fieldInfo in fieldInfoList)
            {
                if (fieldInfo.CheckedTagIds == null || fieldInfo.CheckedTagIds.Count == 0) continue;

                var sql = $"SELECT DISTINCT {nameof(ValueInfo.ChannelId)}, {nameof(ValueInfo.ContentId)} FROM {TableName} WHERE {nameof(ValueInfo.SiteId)} = {siteId} AND {nameof(ValueInfo.FieldId)} = {fieldInfo.Id} ";

                var whereString = new StringBuilder("AND (");

                foreach (var tagId in fieldInfo.CheckedTagIds)
                {
                    whereString.Append($"{nameof(ValueInfo.TagId)} = {tagId} OR ");
                }

                whereString.Length -= 4;

                whereString.Append(")");

                sql += whereString;

                if (channelId > 0)
                {
                    sql += $@" AND {nameof(ValueInfo.ChannelId)} = {channelId}";
                }

                sqlList.Add(sql);
            }

            if (sqlList.Count == 0) return new List<Tuple<int, int>>();

            var sqlString = string.Empty;

            if (sqlList.Count == 1)
            {
                sqlString = sqlList[0];
            }
            else
            {
                for (var i = 0; i < sqlList.Count; i++)
                {
                    if (i == 0)
                    {
                        sqlString +=
                            $"SELECT A.{nameof(ValueInfo.ChannelId)}, A.{nameof(ValueInfo.ContentId)} FROM ({sqlList[i]}) AS A";
                    }
                    else
                    {
                        sqlString +=
                            $" INNER JOIN ({sqlList[i]}) as A{i} on A.{nameof(ValueInfo.ChannelId)} = A{i}.{nameof(ValueInfo.ChannelId)} and A.{nameof(ValueInfo.ContentId)} = A{i}.{nameof(ValueInfo.ContentId)}";
                    }
                }
            }

            using (var rdr = _helper.ExecuteReader(_connectionString, sqlString))
            {
                while (rdr.Read())
                {
                    list.Add(new Tuple<int, int>(_helper.GetInt(rdr, 0), _helper.GetInt(rdr, 1)));
                }
                rdr.Close();
            }

            return list;
        }

        public void Insert(int siteId, int channelId, int contentId, int fieldId, int tagId)
        {
            var valueInfo = new ValueInfo
            {
                SiteId = siteId,
                ChannelId = channelId,
                ContentId = contentId,
                FieldId = fieldId,
                TagId = tagId
            };
            using (var connection = _helper.GetConnection(_connectionString))
            {
                valueInfo.Id = (int)connection.Insert(valueInfo);
            }
        }

        public bool Delete(int id)
        {
            using (var connection = _helper.GetConnection(_connectionString))
            {
                return connection.Delete(new ValueInfo
                {
                    Id = id
                });
            }
        }

        public void DeleteAll(int siteId, int channelId, int contentId, int fieldId)
        {
            var sqlString = $"DELETE FROM {TableName} WHERE {nameof(ValueInfo.SiteId)} = {siteId} AND {nameof(ValueInfo.ChannelId)} = {channelId} AND {nameof(ValueInfo.ContentId)} = {contentId} AND {nameof(ValueInfo.FieldId)} = {fieldId}";

            _helper.ExecuteNonQuery(_connectionString, sqlString);
        }

        public void Delete(int siteId, int channelId, int contentId, int fieldId, int tagId)
        {
            var sqlString = $"DELETE FROM {TableName} WHERE {nameof(ValueInfo.SiteId)} = {siteId} AND {nameof(ValueInfo.ChannelId)} = {channelId} AND {nameof(ValueInfo.ContentId)} = {contentId} AND {nameof(ValueInfo.FieldId)} = {fieldId} AND {nameof(ValueInfo.TagId)} = {tagId}";

            _helper.ExecuteNonQuery(_connectionString, sqlString);
        }
    }
}
