using FreeSql.DataAnnotations;

namespace DevTools.Models
{
    [Table(Name = "SearchRecord")]
    public class SearchRecord
    {
        [Column(IsIdentity = true)]
        public long Id { get; set; }

        public int Env { get; set; }

        public string ClientIp { get; set; }

        public string ServiceName { get; set; }

        public string KeyWord { get; set; }

        public string Query { get; set; }

        public string BeginDate { get; set; }

        public string EndDate { get; set; }

        public DateTime RecordDate { get; set; }
    }
}
