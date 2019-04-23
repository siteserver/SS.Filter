using System;
using System.Linq;
using System.Web.Http;
using SiteServer.Plugin;
using SS.Filter.Core;

namespace SS.Filter.Controllers.Pages
{
    [RoutePrefix("pages/check")]
    public class PagesCheckController : ApiController
    {
        private const string Route = "";

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

        [HttpPost, Route(Route)]
        public IHttpActionResult UpdateValue()
        {
            try
            {
                var request = Context.AuthenticatedRequest;
                var siteId = request.GetQueryInt("siteId");
                if (!request.IsAdminLoggin || !request.AdminPermissions.HasSitePermissions(siteId, Utils.PluginId)) return Unauthorized();

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
                var fieldId = request.GetPostInt("fieldId");
                var tagId = request.GetPostInt("tagId");

                if (isMultiple)
                {
                    if (isAdd)
                    {
                        Main.ValueRepository.Insert(siteId, channelId, contentId, fieldId, tagId);
                    }
                    else
                    {
                        Main.ValueRepository.Delete(siteId, channelId, contentId, fieldId, tagId);
                    }
                }
                else
                {
                    Main.ValueRepository.DeleteAll(siteId, channelId, contentId, fieldId);

                    if (isAdd)
                    {
                        Main.ValueRepository.Insert(siteId, channelId, contentId, fieldId, tagId);
                    }
                }

                return Ok(new
                {
                    Value = true
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}
