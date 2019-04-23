using System;
using System.Web.Http;
using SiteServer.Plugin;
using SS.Filter.Core;

namespace SS.Filter.Controllers.Pages
{
    [RoutePrefix("pages/templates")]
    public class PagesTemplatesController : ApiController
    {
        private const string Route = "";

        [HttpGet, Route(Route)]
        public IHttpActionResult List()
        {
            try
            {
                var request = Context.AuthenticatedRequest;

                var siteId = request.GetQueryInt("siteId");
                if (!request.IsAdminLoggin || !request.AdminPermissions.HasSitePermissions(siteId, Utils.PluginId)) return Unauthorized();

                return Ok(new
                {
                    Value = TemplateManager.GetTemplateInfoList()
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpDelete, Route(Route)]
        public IHttpActionResult Delete()
        {
            try
            {
                var request = Context.AuthenticatedRequest;

                var siteId = request.GetQueryInt("siteId");
                if (!request.IsAdminLoggin || !request.AdminPermissions.HasSitePermissions(siteId, Utils.PluginId)) return Unauthorized();

                var name = request.GetQueryString("name");
                TemplateManager.DeleteTemplate(name);

                return Ok(new
                {
                    Value = TemplateManager.GetTemplateInfoList()
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}
