using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using Datory;
using SiteServer.Plugin;

namespace SS.Filter.Core
{
    public class TagRepository
    {
        private readonly Repository<TagInfo> _repository;

        public TagRepository()
        {
            _repository = new Repository<TagInfo>(Context.Environment.DatabaseType, Context.Environment.ConnectionString);
        }

        public string TableName => _repository.TableName;

        public List<TableColumn> TableColumns => _repository.TableColumns;

        private static class Attr
        {
            public const string Id = nameof(TagInfo.Id);
            public const string FieldId = nameof(TagInfo.FieldId);
            public const string ParentId = nameof(TagInfo.ParentId);
            public const string Taxis = nameof(TagInfo.Taxis);
        }

        private static string GetCacheKey(int fieldId, int parentId)
        {
            return $"{typeof(TagRepository)}.{fieldId}.{parentId}";
        }

        private List<TagInfo> CacheGetTagInfoList(int fieldId, int parentId)
        {
            ObjectCache cache = MemoryCache.Default;
            return cache[GetCacheKey(fieldId, parentId)] as List<TagInfo>;
        }

        private void CacheSetTagInfoList(int fieldId, int parentId, List<TagInfo> list)
        {
            ObjectCache cache = MemoryCache.Default;
            var policy = new CacheItemPolicy
            {
                SlidingExpiration = new TimeSpan(0, 0, 30, 0)
            };
            cache.Set(GetCacheKey(fieldId, parentId), list, policy);
        }

        private void CacheRemoveTagInfoList(int fieldId, int parentId)
        {
            ObjectCache cache = MemoryCache.Default;
            cache.Remove(GetCacheKey(fieldId, parentId));
        }

        public void Insert(int fieldId, int parentId, TagInfo tagInfo)
        {
            if (tagInfo == null) return;
            tagInfo.FieldId = fieldId;
            tagInfo.ParentId = parentId;
            CacheRemoveTagInfoList(fieldId, parentId);

            tagInfo.Id = _repository.Insert(tagInfo);
        }

        public void Update(int fieldId, int parentId, TagInfo tagInfo)
        {
            if (tagInfo == null) return;
            tagInfo.FieldId = fieldId;
            tagInfo.ParentId = parentId;
            CacheRemoveTagInfoList(fieldId, parentId);

            _repository.Update(tagInfo);
        }

        public void Delete(int fieldId, int parentId, List<TagInfo> tagInfoList)
        {
            CacheRemoveTagInfoList(fieldId, parentId);

            var idList = tagInfoList.Select(x => x.Id).ToList();

            _repository.Delete(Q.WhereIn(Attr.ParentId, idList));

            _repository.Delete(Q.WhereIn(Attr.Id, idList));
        }

        public List<TagInfo> GetTagInfoList(int fieldId, int parentId)
        {
           var  list = CacheGetTagInfoList(fieldId, parentId);
            if (list != null) return list;

            list = _repository.GetAll(Q.Where(Attr.FieldId, fieldId).Where(Attr.ParentId, parentId)
                .OrderBy(Attr.Taxis, Attr.Id)).ToList();

            CacheSetTagInfoList(fieldId, parentId, list);

            return list;
        }

        public TagInfo GetTagInfo(int tagId)
        {
            return _repository.Get(tagId);
        }
    }
}
