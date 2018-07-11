using System;
using System.Collections.Generic;
using System.Linq;
using SiteServer.Plugin;
using SS.Filter.Model;

namespace SS.Filter.Controllers
{
    public static class ValuesController
    {
        public const string Name = "values";

        private class ChannelContent
        {
            public IChannelInfo Channel { get; set; }
            public Dictionary<string, object> Content { get; set; }
            public string ChannelUrl { get; set; }
            public string ContentUrl { get; set; }
        }

        public static object Search(IRequest request)
        {
            var siteId = request.GetQueryInt("siteId");
            if (siteId == 0)
            {
                throw new Exception("参数不正确：siteId");
            }

            var channelId = request.GetQueryInt("channelId");

            var top = request.GetQueryInt("top", 20);
            var skip = request.GetQueryInt("skip");

            var fieldInfoList = request.GetPostObject<List<FieldInfo>>();

            var tupleList = Main.Instance.ValueDao.GetChannelIdContentIdTupleList(siteId, channelId, fieldInfoList);

            var list = new List<ChannelContent>();

            if (tupleList.Count > 0)
            {
                var pageTupleList = tupleList.Skip(skip).Take(top).ToList();

                var siteUrl = Main.Instance.FilesApi.GetSiteUrl(siteId);

                foreach (var tuple in pageTupleList)
                {
                    var channelInfo = Main.Instance.ChannelApi.GetChannelInfo(siteId, tuple.Item1);
                    var contentInfo = Main.Instance.ContentApi.GetContentInfo(siteId, tuple.Item1, tuple.Item2);

                    if (channelInfo == null || contentInfo == null) continue;

                    var content = contentInfo.ToDictionary();
                    if (content.ContainsKey("imageUrl"))
                    {
                        var imageUrl = (string) content["imageUrl"];
                        if (!string.IsNullOrEmpty(imageUrl))
                        {
                            imageUrl = imageUrl.Replace("@/", siteUrl + "/");
                            content["imageUrl"] = imageUrl;
                        }
                    }

                    var channelUrl = Main.Instance.FilesApi.GetChannelUrl(siteId, tuple.Item1);
                    var contentUrl = Main.Instance.FilesApi.GetContentUrl(siteId, tuple.Item1, tuple.Item2);

                    list.Add(new ChannelContent
                    {
                        Channel = channelInfo,
                        Content = content,
                        ChannelUrl = channelUrl,
                        ContentUrl = contentUrl
                    });
                }
            }

            return new
            {
                Value = list,
                tupleList.Count
            };
        }

        public static bool Update(IRequest request, int fieldId)
        {
            if (!request.IsAdminLoggin)
            {
                throw new Exception("未授权请求");
            }

            var siteId = request.GetQueryInt("siteId");
            if (siteId == 0)
            {
                throw new Exception("参数不正确：siteId");
            }
            var channelId = request.GetQueryInt("channelId");
            if (channelId == 0)
            {
                throw new Exception("参数不正确：channelId");
            }
            var contentId = request.GetQueryInt("contentId");
            if (contentId == 0)
            {
                throw new Exception("参数不正确：contentId");
            }

            var isMultiple = request.GetPostBool("isMultiple");
            var isAdd = request.GetPostBool("isAdd");
            var tagId = request.GetPostInt("tagId");

            if (isMultiple)
            {
                if (isAdd)
                {
                    Main.Instance.ValueDao.Insert(siteId, channelId, contentId, fieldId, tagId);
                }
                else
                {
                    Main.Instance.ValueDao.Delete(siteId, channelId, contentId, fieldId, tagId);
                }
            }
            else
            {
                Main.Instance.ValueDao.DeleteAll(siteId, channelId, contentId, fieldId);

                if (isAdd)
                {
                    Main.Instance.ValueDao.Insert(siteId, channelId, contentId, fieldId, tagId);
                }
            }

            return true;
        }
    }
}
