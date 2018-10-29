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
        private const string PluginId = "SS.Filter";

        private static readonly Dictionary<int, ConfigInfo> ConfigInfoDict = new Dictionary<int, ConfigInfo>();

        public static ConfigInfo GetConfigInfo(int siteId)
        {
            if (!ConfigInfoDict.ContainsKey(siteId))
            {
                ConfigInfoDict[siteId] = Context.ConfigApi.GetConfig<ConfigInfo>(PluginId, siteId) ?? new ConfigInfo();
            }
            return ConfigInfoDict[siteId];
        }

        public override void Startup(IService service)
        {
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
                        },
                        new Menu
                        {
                            Text = "前台页面标签",
                            Href = "pages/pageStyle.html"
                        }
                    }
                })
                .AddDatabaseTable(FieldDao.TableName, FieldDao.Columns)
                .AddDatabaseTable(TagDao.TableName, TagDao.Columns)
                .AddDatabaseTable(ValueDao.TableName, ValueDao.Columns)
                .AddContentMenu(contentInfo => new Menu
                {
                    Text = "设置筛选项",
                    Href = "pages/pageCheck.html"
                })
                .AddContentColumn("筛选项", GetFilterColumnHtml)
                ;

            service.RestApiGet += Service_RestApiGet;
            service.RestApiPost += Service_RestApiPost;
            service.RestApiPut += Service_RestApiPut;
            service.RestApiDelete += Service_RestApiDelete;
        }

        private string GetFilterColumnHtml(IContentContext contentContext)
        {
            var builder = new StringBuilder();

            var fieldInfoList = FieldDao.GetFieldInfoList(contentContext.SiteId);
            foreach (var fieldInfo in fieldInfoList)
            {
                fieldInfo.TagInfoList = TagDao.GetTagInfoList(fieldInfo.Id, 0);
                if (fieldInfo.TagInfoList == null || fieldInfo.TagInfoList.Count == 0) continue;

                fieldInfo.CheckedTagIds = ValueDao.GetTagIdList(contentContext.SiteId, contentContext.ChannelId, contentContext.ContentId, fieldInfo.Id);

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

        private object Service_RestApiGet(object sender, RestApiEventArgs args)
        {
            if (Utils.EqualsIgnoreCase(args.RouteResource, FieldsController.Name))
            {
                return FieldsController.List(args.Request);
            }
            if (Utils.EqualsIgnoreCase(args.RouteResource, TagsController.Name) && !string.IsNullOrEmpty(args.RouteId))
            {
                return TagsController.Get(args.Request, args.RouteId);
            }

            throw new Exception("请求的资源不在服务器上");
        }

        private object Service_RestApiPost(object sender, RestApiEventArgs args)
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

        private object Service_RestApiPut(object sender, RestApiEventArgs args)
        {
            if (Utils.EqualsIgnoreCase(args.RouteResource, FieldsController.Name))
            {
                return FieldsController.Update(args.Request);
            }
            if (Utils.EqualsIgnoreCase(args.RouteResource, TagsController.Name) && !string.IsNullOrEmpty(args.RouteId))
            {
                return TagsController.Update(args.Request, args.RouteId);
            }
            if (Utils.EqualsIgnoreCase(args.RouteResource, ValuesController.Name) && !string.IsNullOrEmpty(args.RouteId))
            {
                return ValuesController.Update(args.Request, args.RouteId);
            }

            throw new Exception("请求的资源不在服务器上");
        }

        private object Service_RestApiDelete(object sender, RestApiEventArgs args)
        {
            if (Utils.EqualsIgnoreCase(args.RouteResource, FieldsController.Name) && !string.IsNullOrEmpty(args.RouteId))
            {
                return FieldsController.Delete(args.Request, args.RouteId);
            }

            throw new Exception("请求的资源不在服务器上");
        }
    }
}