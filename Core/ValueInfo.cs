using Datory;

namespace SS.Filter.Core
{
    [Table("ss_filter_value")]
    public class ValueInfo : Entity
    {
        [TableColumn]
        public int SiteId { get; set; }

        [TableColumn]
        public int ChannelId { get; set; }

        [TableColumn]
        public int ContentId { get; set; }

        [TableColumn]
        public int FieldId { get; set; }

        [TableColumn]
        public int TagId { get; set; }
    }
}