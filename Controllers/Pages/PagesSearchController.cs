using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using SiteServer.Plugin;
using SS.Filter.Core;

namespace SS.Filter.Controllers.Pages
{
    [RoutePrefix("pages/search")]
    public class PagesSearchController : ApiController
    {
        private const string Route = "";
        private const string RouteValues = "values";

        [HttpGet, Route(Route)]
        public IHttpActionResult GetFields()
        {
            try
            {
                var request = Context.AuthenticatedRequest;
                var siteId = request.GetQueryInt("siteId");
                if (!request.IsAdminLoggin || !request.AdminPermissions.HasSitePermissions(siteId, Utils.PluginId)) return Unauthorized();

                var channelId = request.GetQueryInt("channelId");
                var contentId = request.GetQueryInt("contentId");

                var fieldInfoList = Main.FieldRepository.GetFieldInfoList(siteId);
                foreach (var fieldInfo in fieldInfoList)
                {
                    fieldInfo.TagInfoList = Main.TagRepository.GetTagInfoList(fieldInfo.Id, 0);
                    fieldInfo.Tags = fieldInfo.TagInfoList.Select(x => x.Title).ToList();
                    fieldInfo.CheckedTagIds = Main.ValueRepository.GetTagIdList(siteId, channelId, contentId, fieldInfo.Id);
                }

                return Ok(new
                {
                    Value = fieldInfoList
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet, Route(RouteValues)]
        public IHttpActionResult GetValues()
        {
            try
            {
                var request = Context.AuthenticatedRequest;
                var siteId = request.GetQueryInt("siteId");
                if (!request.IsAdminLoggin || !request.AdminPermissions.HasSitePermissions(siteId, Utils.PluginId)) return Unauthorized();

                var channelId = request.GetQueryInt("channelId");

                var top = request.GetQueryInt("top", 20);
                var skip = request.GetQueryInt("skip");

                var fieldInfoList = Main.FieldRepository.GetFieldInfoList(siteId);
                foreach (var fieldInfo in fieldInfoList)
                {
                    fieldInfo.CheckedTagIds = new List<int>();
                    if (!request.IsQueryExists($"{fieldInfo.Id}[]")) continue;

                    var checkedTagIds = request.GetQueryString($"{fieldInfo.Id}[]");
                    fieldInfo.CheckedTagIds = Utils.StringCollectionToIntList(checkedTagIds);
                }

                var tupleList = Main.ValueRepository.GetChannelIdContentIdTupleList(siteId, channelId, fieldInfoList);

                var list = new List<ChannelContent>();

                if (tupleList.Count > 0)
                {
                    var pageTupleList = tupleList.Skip(skip).Take(top).ToList();

                    var siteUrl = Context.SiteApi.GetSiteUrl(siteId);

                    foreach (var tuple in pageTupleList)
                    {
                        var channelInfo = Context.ChannelApi.GetChannelInfo(siteId, tuple.Item1);
                        var contentInfo = Context.ContentApi.GetContentInfo(siteId, tuple.Item1, tuple.Item2);

                        if (channelInfo == null || contentInfo == null) continue;

                        var content = contentInfo.ToDictionary();
                        if (content.ContainsKey("imageUrl"))
                        {
                            var imageUrl = (string)content["imageUrl"];
                            if (!string.IsNullOrEmpty(imageUrl))
                            {
                                imageUrl = imageUrl.Replace("@/", siteUrl + "/");
                                content["imageUrl"] = imageUrl;
                            }
                        }

                        var channelUrl = Context.ChannelApi.GetChannelUrl(siteId, tuple.Item1);
                        var contentUrl = Context.ContentApi.GetContentUrl(siteId, tuple.Item1, tuple.Item2);

                        list.Add(new ChannelContent
                        {
                            Channel = channelInfo,
                            Content = content,
                            ChannelUrl = channelUrl,
                            ContentUrl = contentUrl
                        });
                    }
                }

                return Ok(new
                {
                    Value = list,
                    tupleList.Count
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}
