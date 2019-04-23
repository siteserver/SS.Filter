using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using SiteServer.Plugin;
using SS.Filter.Core;

namespace SS.Filter.Controllers
{
    [RoutePrefix("")]
    public class FilterController : ApiController
    {
        private const string RouteFields = "fields";
        private const string RouteTagsId = "tags/{tagId:int}";
        private const string RouteValuesActionsSearch = "values/actions/search";

        [HttpGet, Route(RouteFields)]
        public IHttpActionResult GetFields()
        {
            try
            {
                var request = Context.AuthenticatedRequest;
                var siteId = request.GetQueryInt("siteId");
                if (siteId == 0)
                {
                    throw new Exception("参数不正确：siteId");
                }
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

        [HttpGet, Route(RouteTagsId)]
        public IHttpActionResult GetTagInfo(string tagId)
        {
            try
            {
                var request = Context.AuthenticatedRequest;
                var fieldId = request.GetQueryInt("fieldId");
                if (fieldId == 0)
                {
                    throw new Exception("参数不正确：fieldId");
                }

                var tagInfo = Main.TagRepository.GetTagInfo(Utils.ToInt(tagId));
                if (tagInfo != null)
                {
                    tagInfo.TagInfoList = Main.TagRepository.GetTagInfoList(fieldId, tagInfo.Id);
                    tagInfo.Tags = tagInfo.TagInfoList.Select(x => x.Title).ToList();
                }

                return Ok(new
                {
                    Value = tagInfo
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost, Route(RouteValuesActionsSearch)]
        public IHttpActionResult Search()
        {
            try
            {
                var request = Context.AuthenticatedRequest;
                var siteId = request.GetQueryInt("siteId");
                if (siteId == 0)
                {
                    throw new Exception("参数不正确：siteId");
                }

                var channelId = request.GetQueryInt("channelId");

                var top = request.GetQueryInt("top", 20);
                var skip = request.GetQueryInt("skip");

                var fieldInfoList = request.GetPostObject<List<FieldInfo>>();

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
