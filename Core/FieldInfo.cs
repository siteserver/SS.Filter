using System.Collections.Generic;
using Datory;

namespace SS.Filter.Core
{
    [Table("ss_filter_field")]
    public class FieldInfo : Entity
    {
        [TableColumn]
        public int SiteId { get; set; }

        [TableColumn]
        public int Taxis { get; set; }

        [TableColumn]
        public string Title { get; set; }

        [TableColumn]
        public string InputType { get; set; }

        public List<string> Tags { get; set; }

        public List<TagInfo> TagInfoList { get; set; }

        public List<int> CheckedTagIds { get; set; }
    }
}