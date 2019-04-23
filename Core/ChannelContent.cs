using System.Collections.Generic;
using SiteServer.Plugin;

namespace SS.Filter.Core
{
    public class ChannelContent
    {
        public IChannelInfo Channel { get; set; }
        public IDictionary<string, object> Content { get; set; }
        public string ChannelUrl { get; set; }
        public string ContentUrl { get; set; }
    }
}
