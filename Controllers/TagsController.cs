using System;
using System.Collections.Generic;
using System.Linq;
using SiteServer.Plugin;
using SS.Filter.Model;

namespace SS.Filter.Controllers
{
    public static class TagsController
    {
        public const string Name = "tags";

        public static TagInfo Get(IRequest request, int tagId)
        {
            var fieldId = request.GetQueryInt("fieldId");
            if (fieldId == 0)
            {
                throw new Exception("参数不正确：fieldId");
            }

            var tagInfo = Main.Instance.TagDao.GetTagInfo(tagId);
            if (tagInfo != null)
            {
                tagInfo.TagInfoList = Main.Instance.TagDao.GetTagInfoList(fieldId, tagInfo.Id);
                tagInfo.Tags = tagInfo.TagInfoList.Select(x => x.Title).ToList();
            }

            return tagInfo;
        }

        public static TagInfo Update(IRequest request, int tagId)
        {
            if (!request.IsAdminLoggin)
            {
                throw new Exception("未授权请求");
            }

            var tagInfoToUpdate = request.GetPostObject<TagInfo>();

            if (tagInfoToUpdate == null)
            {
                throw new Exception("参数不正确");
            }

            var fieldId = tagInfoToUpdate.FieldId;
            var parentId = tagInfoToUpdate.Id;

            tagInfoToUpdate.Id = tagId;
            Main.Instance.TagDao.Update(tagInfoToUpdate.FieldId, tagInfoToUpdate.ParentId, tagInfoToUpdate);

            var tagInfoList = Main.Instance.TagDao.GetTagInfoList(fieldId, parentId);
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
                Main.Instance.TagDao.Delete(fieldId, parentId, tagInfoListToDelete);
            }

            var taxis = 1;
            foreach (var tag in tagInfoToUpdate.Tags)
            {
                var tagInfo = tagInfoList.Find(t => t.Title == tag);
                if (tagInfo == null)
                {
                    Main.Instance.TagDao.Insert(fieldId, parentId, new TagInfo
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
                    Main.Instance.TagDao.Update(fieldId, parentId, tagInfo);
                }

                taxis++;
            }

            return tagInfoToUpdate;
        }
    }
}
