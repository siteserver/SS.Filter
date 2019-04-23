using System;
using System.Linq;
using System.Web.Http;
using SiteServer.Plugin;
using SS.Filter.Core;

namespace SS.Filter.Controllers.Pages
{
    [RoutePrefix("pages/templatesLayerEdit")]
    public class PagesTemplatesLayerEditController : ApiController
    {
        private const string Route = "";

        [HttpGet, Route(Route)]
        public IHttpActionResult Get()
        {
            try
            {
                var request = Context.AuthenticatedRequest;

                var siteId = request.GetQueryInt("siteId");
                if (!request.IsAdminLoggin || !request.AdminPermissions.HasSitePermissions(siteId, Utils.PluginId)) return Unauthorized();

                var name = request.GetQueryString("name");
                var templateInfo = TemplateManager.GetTemplateInfo(name);

                if (!string.IsNullOrEmpty(templateInfo.Publisher))
                {
                    templateInfo = new TemplateInfo();
                }

                return Ok(new
                {
                    Value = templateInfo
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost, Route(Route)]
        public IHttpActionResult Clone()
        {
            try
            {
                var request = Context.AuthenticatedRequest;

                var siteId = request.GetQueryInt("siteId");
                if (!request.IsAdminLoggin || !request.AdminPermissions.HasSitePermissions(siteId, Utils.PluginId)) return Unauthorized();

                var originalName = request.GetPostString("originalName");
                var name = request.GetPostString("name");
                var description = request.GetPostString("description");
                var templateHtml = request.GetPostString("templateHtml");

                var templateInfoList = TemplateManager.GetTemplateInfoList();
                var originalTemplateInfo = templateInfoList.First(x => Utils.EqualsIgnoreCase(originalName, x.Name));

                if (templateInfoList.Any(x => Utils.EqualsIgnoreCase(name, x.Name)))
                {
                    return BadRequest($"标识为 {name} 的模板已存在，请更换模板标识！");
                }

                var templateInfo = new TemplateInfo
                {
                    Name = name,
                    Main = originalTemplateInfo.Main,
                    Publisher = string.Empty,
                    Description = description,
                    Icon = originalTemplateInfo.Icon
                };
                templateInfoList.Add(templateInfo);

                TemplateManager.Clone(originalName, templateInfo, templateHtml);

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

        [HttpPut, Route(Route)]
        public IHttpActionResult Edit()
        {
            try
            {
                var request = Context.AuthenticatedRequest;

                var siteId = request.GetQueryInt("siteId");
                if (!request.IsAdminLoggin || !request.AdminPermissions.HasSitePermissions(siteId, Utils.PluginId)) return Unauthorized();

                var originalName = request.GetPostString("originalName");
                var name = request.GetPostString("name");
                var description = request.GetPostString("description");

                if (Utils.EqualsIgnoreCase(originalName, name))
                {
                    var templateInfoList = TemplateManager.GetTemplateInfoList();
                    var originalTemplateInfo = templateInfoList.First(x => Utils.EqualsIgnoreCase(originalName, x.Name));

                    originalTemplateInfo.Name = name;
                    originalTemplateInfo.Description = description;
                    TemplateManager.Edit(originalTemplateInfo);
                }
                else
                {
                    var templateInfoList = TemplateManager.GetTemplateInfoList();
                    var originalTemplateInfo = templateInfoList.First(x => Utils.EqualsIgnoreCase(originalName, x.Name));

                    if (templateInfoList.Any(x => Utils.EqualsIgnoreCase(name, x.Name)))
                    {
                        return BadRequest($"标识为 {name} 的模板已存在，请更换模板标识！");
                    }

                    var templateInfo = new TemplateInfo
                    {
                        Name = name,
                        Main = originalTemplateInfo.Main,
                        Publisher = string.Empty,
                        Description = description,
                        Icon = originalTemplateInfo.Icon
                    };
                    templateInfoList.Add(templateInfo);

                    TemplateManager.Clone(originalName, templateInfo);

                    TemplateManager.DeleteTemplate(originalName);
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
