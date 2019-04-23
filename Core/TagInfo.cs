using System.Collections.Generic;
using Datory;

namespace SS.Filter.Core
{
    [Table("ss_filter_tag")]
    public class TagInfo : Entity
    {
        [TableColumn]
        public int FieldId { get; set; }

        [TableColumn]
        public int ParentId { get; set; }

        [TableColumn]
        public int Taxis { get; set; }

        [TableColumn]
        public string Title { get; set; }

        public List<string> Tags { get; set; }

        public List<TagInfo> TagInfoList { get; set; }
    }
}