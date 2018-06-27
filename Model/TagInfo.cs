using System.Collections.Generic;
using Dapper.Contrib.Extensions;
using SS.Filter.Provider;

namespace SS.Filter.Model
{
    [Table(TagDao.TableName)]
    public class TagInfo
    {
        public int Id { get; set; }

        public int FieldId { get; set; }

        public int ParentId { get; set; }

        public int Taxis { get; set; }

        public string Title { get; set; }

        [Computed]
        public List<string> Tags { get; set; }

        [Computed]
        public List<TagInfo> TagInfoList { get; set; }
    }
}