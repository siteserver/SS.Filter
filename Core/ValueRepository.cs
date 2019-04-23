using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dapper;
using Datory;
using SiteServer.Plugin;

namespace SS.Filter.Core
{
    public class ValueRepository
    {
        private readonly Repository<ValueInfo> _repository;

        public ValueRepository()
        {
            _repository = new Repository<ValueInfo>(Context.Environment.DatabaseType, Context.Environment.ConnectionString);
        }

        public string TableName => _repository.TableName;

        public List<TableColumn> TableColumns => _repository.TableColumns;

        private static class Attr
        {
            public const string SiteId = nameof(ValueInfo.SiteId);
            public const string ChannelId = nameof(ValueInfo.ChannelId);
            public const string ContentId = nameof(ValueInfo.ContentId);
            public const string FieldId = nameof(ValueInfo.FieldId);
            public const string TagId = nameof(ValueInfo.TagId);
        }

        public List<int> GetTagIdList(int siteId, int channelId, int contentId, int fieldId)
        {
            if (channelId == 0 || contentId == 0) return new List<int>();

            return _repository.GetAll<int>(Q
                .Select(Attr.TagId)
                .Where(Attr.SiteId, siteId)
                .Where(Attr.ChannelId, channelId)
                .Where(Attr.ContentId, contentId)
                .Where(Attr.FieldId, fieldId)
            ).ToList();
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

            //using (var rdr = Context.DatabaseApi.ExecuteReader(Context.ConnectionString, sqlString))
            //{
            //    while (rdr.Read())
            //    {
            //        list.Add(new Tuple<int, int>(Context.DatabaseApi.GetInt(rdr, 0), Context.DatabaseApi.GetInt(rdr, 1)));
            //    }
            //    rdr.Close();
            //}

            using (var connection = new Connection(Context.Environment.DatabaseType, Context.Environment.ConnectionString))
            {
                connection.Open();

                using (var rdr = connection.ExecuteReader(sqlString))
                {
                    while (rdr.Read() && !rdr.IsDBNull(0) && !rdr.IsDBNull(1))
                    {
                        list.Add(new Tuple<int, int>(rdr.GetInt32(0), rdr.GetInt32(1)));
                    }
                    rdr.Close();
                }
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

            valueInfo.Id = _repository.Insert(valueInfo);
        }

        public bool Delete(int id)
        {
            return _repository.Delete(id);
        }

        public void DeleteAll(int siteId, int channelId, int contentId, int fieldId)
        {
            _repository.Delete(Q
                .Where(Attr.SiteId, siteId)
                .Where(Attr.ChannelId, channelId)
                .Where(Attr.ContentId, contentId)
                .Where(Attr.FieldId, fieldId)
            );
        }

        public void Delete(int siteId, int channelId, int contentId, int fieldId, int tagId)
        {
            _repository.Delete(Q
                .Where(Attr.SiteId, siteId)
                .Where(Attr.ChannelId, channelId)
                .Where(Attr.ContentId, contentId)
                .Where(Attr.FieldId, fieldId)
                .Where(Attr.TagId, tagId)
            );
        }
    }
}
