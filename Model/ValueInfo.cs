using Dapper.Contrib.Extensions;
using SS.Filter.Provider;

namespace SS.Filter.Model
{
    [Table(ValueDao.TableName)]
    public class ValueInfo
    {
        public int Id { get; set; }

        public int SiteId { get; set; }

        public int ChannelId { get; set; }

        public int ContentId { get; set; }

        public int FieldId { get; set; }

        public int TagId { get; set; }
    }
}