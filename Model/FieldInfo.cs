using System.Collections.Generic;
using Dapper.Contrib.Extensions;
using SS.Filter.Provider;

namespace SS.Filter.Model
{
    [Table(FieldDao.TableName)]
    public class FieldInfo
    {
        public int Id { get; set; }

        public int SiteId { get; set; }

        public int Taxis { get; set; }

        public string Title { get; set; }

        public string InputType { get; set; }

        [Computed]
        public List<string> Tags { get; set; }

        [Computed]
        public List<TagInfo> TagInfoList { get; set; }

        [Computed]
        public List<int> CheckedTagIds { get; set; }
    }
}