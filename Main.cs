using System;
using System.Collections.Generic;
using System.Text;
using SiteServer.Plugin;
using SS.Filter.Controllers;
using SS.Filter.Core;
using SS.Filter.Model;
using SS.Filter.Provider;

namespace SS.Filter
{
    public class Main : PluginBase
    {
        private static readonly Dictionary<int, ConfigInfo> ConfigInfoDict = new Dictionary<int, ConfigInfo>();

        public ConfigInfo GetConfigInfo(int siteId)
        {
            if (!ConfigInfoDict.ContainsKey(siteId))
            {
                ConfigInfoDict[siteId] = ConfigApi.GetConfig<ConfigInfo>(siteId) ?? new ConfigInfo();
            }
            return ConfigInfoDict[siteId];
        }

        public static Main Instance { get; private set; }

        public FieldDao FieldDao { get; private set; }
        public TagDao TagDao { get; private set; }
        public ValueDao ValueDao { get; private set; }

        public override void Startup(IService service)
        {
            Instance = this;

            FieldDao = new FieldDao(ConnectionString, DataApi);
            TagDao = new TagDao(ConnectionString, DataApi);
            ValueDao = new ValueDao(ConnectionString, DataApi);

            service
                .AddSiteMenu(siteId => new Menu
                {
                    Text = "筛选",
                    IconClass = "ion-funnel",
                    Menus = new List<Menu>
                    {
                        new Menu
                        {
                            Text = "内容筛选",
                            Href = "pages/pageSearch.html"
                        },
                        new Menu
                        {
                            Text = "分类设置",
                            Href = "pages/pageFields.html"
                        }
                    }
                })
                .AddDatabaseTable(FieldDao.TableName, FieldDao.Columns)
                .AddDatabaseTable(TagDao.TableName, TagDao.Columns)
                .AddDatabaseTable(ValueDao.TableName, ValueDao.Columns)
                .AddContentMenu(new Menu
                {
                    Text = "设置筛选项",
                    Href = "pages/pageCheck.html"
                })
                .AddContentColumn("筛选项", GetFilterColumnHtml)
                ;

            service.ApiGet += Service_ApiGet;
            service.ApiPost += Service_ApiPost;
            service.ApiPut += Service_ApiPut;
            service.ApiDelete += Service_ApiDelete;
        }

        private string GetFilterColumnHtml(IContentContext contentContext)
        {
            var builder = new StringBuilder();

            var fieldInfoList = Instance.FieldDao.GetFieldInfoList(contentContext.SiteId);
            foreach (var fieldInfo in fieldInfoList)
            {
                fieldInfo.TagInfoList = Instance.TagDao.GetTagInfoList(fieldInfo.Id, 0);
                if (fieldInfo.TagInfoList == null || fieldInfo.TagInfoList.Count == 0) continue;

                fieldInfo.CheckedTagIds = Instance.ValueDao.GetTagIdList(contentContext.SiteId, contentContext.ChannelId, contentContext.ContentId, fieldInfo.Id);

                if (fieldInfo.CheckedTagIds == null || fieldInfo.CheckedTagIds.Count == 0) continue;

                var tagInfoList = fieldInfo.TagInfoList.FindAll(x => fieldInfo.CheckedTagIds.Contains(x.Id));
                if (tagInfoList.Count == 0) continue;

                foreach (var tagInfo in tagInfoList)
                {
                    builder.Append($@"<span class=""badge badge-light"">{tagInfo.Title}</span>");
                }
            }

            return builder.ToString();
        }

        private object Service_ApiGet(object sender, ApiEventArgs args)
        {
            if (Utils.EqualsIgnoreCase(args.RouteResource, FieldsController.Name))
            {
                return FieldsController.List(args.Request);
            }
            if (Utils.EqualsIgnoreCase(args.RouteResource, TagsController.Name) && args.RouteId > 0)
            {
                return TagsController.Get(args.Request, args.RouteId);
            }

            throw new Exception("请求的资源不在服务器上");
        }

        private object Service_ApiPost(object sender, ApiEventArgs args)
        {
            if (Utils.EqualsIgnoreCase(args.RouteResource, FieldsController.Name))
            {
                return FieldsController.Create(args.Request);
            }
            if (Utils.EqualsIgnoreCase(args.RouteResource, ValuesController.Name) && Utils.EqualsIgnoreCase(args.RouteAction, nameof(ValuesController.Search)))
            {
                return ValuesController.Search(args.Request);
            }

            throw new Exception("请求的资源不在服务器上");
        }

        private object Service_ApiPut(object sender, ApiEventArgs args)
        {
            if (Utils.EqualsIgnoreCase(args.RouteResource, FieldsController.Name))
            {
                return FieldsController.Update(args.Request);
            }
            if (Utils.EqualsIgnoreCase(args.RouteResource, TagsController.Name) && args.RouteId > 0)
            {
                return TagsController.Update(args.Request, args.RouteId);
            }
            if (Utils.EqualsIgnoreCase(args.RouteResource, ValuesController.Name) && args.RouteId > 0)
            {
                return ValuesController.Update(args.Request, args.RouteId);
            }

            throw new Exception("请求的资源不在服务器上");
        }

        private object Service_ApiDelete(object sender, ApiEventArgs args)
        {
            if (Utils.EqualsIgnoreCase(args.RouteResource, FieldsController.Name) && args.RouteId > 0)
            {
                return FieldsController.Delete(args.Request, args.RouteId);
            }

            throw new Exception("请求的资源不在服务器上");
        }
    }
}