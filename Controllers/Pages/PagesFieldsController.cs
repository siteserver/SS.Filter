using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using SiteServer.Plugin;
using SS.Filter.Core;

namespace SS.Filter.Controllers.Pages
{
    [RoutePrefix("pages/fields")]
    public class FieldsController : ApiController
    {
        private const string Route = "";
        private const string RouteFieldId = "{fieldId:int}";

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

        [HttpGet, Route("{fieldId:int}/{tagId:int}")]
        public IHttpActionResult GetTagInfo(int fieldId, int tagId)
        {
            try
            {
                var request = Context.AuthenticatedRequest;
                var siteId = request.GetQueryInt("siteId");
                if (!request.IsAdminLoggin || !request.AdminPermissions.HasSitePermissions(siteId, Utils.PluginId)) return Unauthorized();

                var tagInfo = Main.TagRepository.GetTagInfo(tagId);
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

        [HttpPut, Route("{fieldId:int}/{tagId:int}")]
        public IHttpActionResult UpdateTagInfo(int fieldId, int tagId)
        {
            try
            {
                var request = Context.AuthenticatedRequest;
                var siteId = request.GetQueryInt("siteId");
                if (!request.IsAdminLoggin || !request.AdminPermissions.HasSitePermissions(siteId, Utils.PluginId)) return Unauthorized();

                var tagInfoToUpdate = request.GetPostObject<TagInfo>();

                if (tagInfoToUpdate == null)
                {
                    throw new Exception("参数不正确");
                }

                var parentId = tagInfoToUpdate.Id;

                tagInfoToUpdate.Id = tagId;
                Main.TagRepository.Update(tagInfoToUpdate.FieldId, tagInfoToUpdate.ParentId, tagInfoToUpdate);

                var tagInfoList = Main.TagRepository.GetTagInfoList(fieldId, parentId);
                if (tagInfoToUpdate.Tags == null)
                {
                    tagInfoToUpdate.Tags = new List<string>();
                }

                var tagInfoListToDelete = new List<TagInfo>();
                foreach (var tagInfo in tagInfoList)
                {
                    if (!tagInfoToUpdate.Tags.Contains(tagInfo.Title))
                    {
                        tagInfoListToDelete.Add(tagInfo);
                    }
                }

                if (tagInfoListToDelete.Count > 0)
                {
                    Main.TagRepository.Delete(fieldId, parentId, tagInfoListToDelete);
                }

                var taxis = 1;
                foreach (var tag in tagInfoToUpdate.Tags)
                {
                    var tagInfo = tagInfoList.Find(t => t.Title == tag);
                    if (tagInfo == null)
                    {
                        Main.TagRepository.Insert(fieldId, parentId, new TagInfo
                        {
                            Id = 0,
                            FieldId = fieldId,
                            ParentId = parentId,
                            Taxis = taxis,
                            Title = tag
                        });
                    }
                    else
                    {
                        tagInfo.Taxis = taxis;
                        Main.TagRepository.Update(fieldId, parentId, tagInfo);
                    }

                    taxis++;
                }

                return Ok(new
                {
                    Value = tagInfoToUpdate
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost, Route(Route)]
        public IHttpActionResult CreateFieldInfo()
        {
            try
            {
                var request = Context.AuthenticatedRequest;
                var siteId = request.GetQueryInt("siteId");
                if (!request.IsAdminLoggin || !request.AdminPermissions.HasSitePermissions(siteId, Utils.PluginId)) return Unauthorized();

                var fieldInfo = request.GetPostObject<FieldInfo>("fieldInfo");
                var tags = request.GetPostObject<List<string>>("tags");

                Main.FieldRepository.Insert(siteId, fieldInfo);

                var fieldId = fieldInfo.Id;

                if (tags == null)
                {
                    tags = new List<string>();
                }

                var taxis = 1;
                foreach (var tag in tags)
                {
                    Main.TagRepository.Insert(fieldId, 0, new TagInfo
                    {
                        Id = 0,
                        FieldId = fieldId,
                        ParentId = 0,
                        Taxis = taxis,
                        Title = tag
                    });

                    taxis++;
                }

                return Ok(new
                {
                    Value = fieldInfo
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPut, Route(Route)]
        public IHttpActionResult UpdateFieldInfo()
        {
            try
            {
                var request = Context.AuthenticatedRequest;
                var siteId = request.GetQueryInt("siteId");
                if (!request.IsAdminLoggin || !request.AdminPermissions.HasSitePermissions(siteId, Utils.PluginId)) return Unauthorized();

                var fieldInfo = request.GetPostObject<FieldInfo>("fieldInfo");
                var tags = request.GetPostObject<List<string>>("tags");

                Main.FieldRepository.Update(siteId, fieldInfo);

                var fieldId = fieldInfo.Id;
                var parentId = 0;

                var tagInfoList = Main.TagRepository.GetTagInfoList(fieldId, parentId);
                if (tags == null)
                {
                    tags = new List<string>();
                }

                var tagInfoListToDelete = new List<TagInfo>();
                foreach (var tagInfo in tagInfoList)
                {
                    if (!tags.Contains(tagInfo.Title))
                    {
                        tagInfoListToDelete.Add(tagInfo);
                    }
                }

                if (tagInfoListToDelete.Count > 0)
                {
                    Main.TagRepository.Delete(fieldId, parentId, tagInfoListToDelete);
                }

                var taxis = 1;
                foreach (var tag in tags)
                {
                    var tagInfo = tagInfoList.Find(t => t.Title == tag);
                    if (tagInfo == null)
                    {
                        Main.TagRepository.Insert(fieldId, parentId, new TagInfo
                        {
                            Id = 0,
                            FieldId = fieldId,
                            ParentId = parentId,
                            Taxis = taxis,
                            Title = tag
                        });
                    }
                    else
                    {
                        tagInfo.Taxis = taxis;
                        Main.TagRepository.Update(fieldId, parentId, tagInfo);
                    }

                    taxis++;
                }

                return Ok(new
                {
                    Value = fieldInfo
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpDelete, Route(RouteFieldId)]
        public IHttpActionResult Delete(string fieldId)
        {
            try
            {
                var request = Context.AuthenticatedRequest;
                var siteId = request.GetQueryInt("siteId");
                if (!request.IsAdminLoggin || !request.AdminPermissions.HasSitePermissions(siteId, Utils.PluginId)) return Unauthorized();

                var deleted =  Main.FieldRepository.Delete(siteId, Utils.ToInt(fieldId));

                return Ok(new
                {
                    Value = deleted
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}
