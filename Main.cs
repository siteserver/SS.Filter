using System.Collections.Generic;
using System.Text;
using SiteServer.Plugin;
using SS.Filter.Core;

namespace SS.Filter
{
    public class Main : PluginBase
    {
        public static FieldRepository FieldRepository { get; set; }
        public static TagRepository TagRepository { get; set; }
        public static ValueRepository ValueRepository { get; set; }

        public override void Startup(IService service)
        {
            FieldRepository = new FieldRepository();
            TagRepository = new TagRepository();
            ValueRepository = new ValueRepository();

            service
                .AddSiteMenu(siteId => new Menu
                {
                    Text = "筛选",
                    IconClass = "ion-funnel",
                    Href = "pages/search.html",
                    Menus = new List<Menu>
                    {
                        new Menu
                        {
                            Text = "内容筛选",
                            Href = "pages/search.html"
                        },
                        new Menu
                        {
                            Text = "分类设置",
                            Href = "pages/fields.html"
                        },
                        new Menu
                        {
                            Text = "筛选模板",
                            Href = "pages/templates.html"
                        }
                    }
                })
                .AddDatabaseTable(FieldRepository.TableName, FieldRepository.TableColumns)
                .AddDatabaseTable(TagRepository.TableName, TagRepository.TableColumns)
                .AddDatabaseTable(ValueRepository.TableName, ValueRepository.TableColumns)
                .AddContentMenu(contentInfo => new Menu
                {
                    Text = "设置筛选项",
                    Href = "pages/check.html",
                    Target = "_layer"
                })
                .AddContentColumn("筛选项", GetFilterColumnHtml)
                .AddStlElementParser(StlFilter.ElementName, StlFilter.Parse)
                ;
        }

        private string GetFilterColumnHtml(IContentContext contentContext)
        {
            var builder = new StringBuilder();

            var fieldInfoList = FieldRepository.GetFieldInfoList(contentContext.SiteId);
            foreach (var fieldInfo in fieldInfoList)
            {
                fieldInfo.TagInfoList = TagRepository.GetTagInfoList(fieldInfo.Id, 0);
                if (fieldInfo.TagInfoList == null || fieldInfo.TagInfoList.Count == 0) continue;

                fieldInfo.CheckedTagIds = ValueRepository.GetTagIdList(contentContext.SiteId, contentContext.ChannelId, contentContext.ContentId, fieldInfo.Id);

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
    }
}