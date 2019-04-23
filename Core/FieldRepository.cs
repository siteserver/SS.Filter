using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using Datory;
using SiteServer.Plugin;

namespace SS.Filter.Core
{
    public class FieldRepository
    {
        private static string GetCacheKey(int siteId)
        {
            return $"{typeof(FieldRepository)}.{siteId}";
        }

        private List<FieldInfo> CacheGetFieldInfoList(int siteId)
        {
            ObjectCache cache = MemoryCache.Default;
            return cache[GetCacheKey(siteId)] as List<FieldInfo>;
        }

        private void CacheSetFieldInfoList(int siteId, List<FieldInfo> list)
        {
            ObjectCache cache = MemoryCache.Default;
            var policy = new CacheItemPolicy
            {
                SlidingExpiration = new TimeSpan(0, 0, 30, 0)
            };
            cache.Set(GetCacheKey(siteId), list, policy);
        }

        private void CacheRemoveFieldInfoList(int siteId)
        {
            ObjectCache cache = MemoryCache.Default;
            cache.Remove(GetCacheKey(siteId));
        }

        private readonly Repository<FieldInfo> _repository;

        public FieldRepository()
        {
            _repository = new Repository<FieldInfo>(Context.Environment.DatabaseType, Context.Environment.ConnectionString);
        }

        public string TableName => _repository.TableName;

        public List<TableColumn> TableColumns => _repository.TableColumns;

        private static class Attr
        {
            public const string Id = nameof(FieldInfo.Id);
            public const string SiteId = nameof(FieldInfo.SiteId);
            public const string Taxis = nameof(FieldInfo.Taxis);
        }

        public void Insert(int siteId, FieldInfo fieldInfo)
        {
            if (fieldInfo == null) return;
            fieldInfo.SiteId = siteId;
            CacheRemoveFieldInfoList(siteId);

            fieldInfo.Id = _repository.Insert(fieldInfo);
        }

        public void Update(int siteId, FieldInfo fieldInfo)
        {
            if (fieldInfo == null) return;
            fieldInfo.SiteId = siteId;
            CacheRemoveFieldInfoList(siteId);

            _repository.Update(fieldInfo);
        }

        public bool Delete(int siteId, int fieldId)
        {
            CacheRemoveFieldInfoList(siteId);

            return _repository.Delete(fieldId);
        }

        public List<FieldInfo> GetFieldInfoList(int siteId)
        {
            var list = CacheGetFieldInfoList(siteId);
            if (list != null) return list;

            list = _repository.GetAll(Q.Where(Attr.SiteId, siteId).OrderBy(Attr.Taxis, Attr.Id)).ToList();

            CacheSetFieldInfoList(siteId, list);

            return list;
        }
    }
}
