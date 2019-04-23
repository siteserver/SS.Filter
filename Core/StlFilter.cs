using System.Net;
using SiteServer.Plugin;

namespace SS.Filter.Core
{
    public static class StlFilter
    {
        public const string ElementName = "stl:filter";

        private const string AttributeType = "type";
        private const string AttributeChannelIndex = "channelIndex";

        public static string Parse(IParseContext context)
        {
            var type = "style1";
            var channelIndex = string.Empty;

            foreach (var name in context.StlAttributes.AllKeys)
            {
                var value = context.StlAttributes[name];

                if (Utils.EqualsIgnoreCase(name, AttributeChannelIndex))
                {
                    channelIndex = Context.ParseApi.ParseAttributeValue(value, context);
                }
                else if (Utils.EqualsIgnoreCase(name, AttributeType))
                {
                    type = Context.ParseApi.ParseAttributeValue(value, context);
                }
            }

            var elementId = $"iframe_{Utils.GetShortGuid(false)}";
            var libUrl = Context.PluginApi.GetPluginUrl(Utils.PluginId, "assets/lib/iframe-resizer/iframeResizer-3.6.3.min.js");
            var pageUrl = Context.PluginApi.GetPluginUrl(Utils.PluginId, $"templates/{type}/index.html?siteId={context.SiteId}&apiUrl={WebUtility.UrlEncode(Context.Environment.ApiUrl)}");

            if (!string.IsNullOrEmpty(channelIndex))
            {
                var channelId = Context.ChannelApi.GetChannelId(context.SiteId, channelIndex);
                if (channelId > 0)
                {
                    pageUrl += $"&channelId={channelId}";
                }
            }

            return $@"
<iframe id=""{elementId}"" frameborder=""0"" scrolling=""no"" src=""{pageUrl}"" style=""width: 1px;min-width: 100%;""></iframe>
<script type=""text/javascript"" src=""{libUrl}""></script>
<script type=""text/javascript"">iFrameResize({{log: false}}, '#{elementId}')</script>
";
        }
    }
}
