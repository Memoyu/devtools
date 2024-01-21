using FreeSql.Internal.Model;

namespace DevTools.Models
{
    public class SearchRecordPagingInfo : BasePagingInfo
    {
        public int Env { get; set; }

        public string KeyWord { get; set; }
    }
}
