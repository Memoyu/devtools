using CommunityToolkit.Mvvm.ComponentModel;
using FreeSql.DataAnnotations;
using System.Threading;

namespace DevTools.Models
{
    [Table(Name = "SearchRemark")]
    public class SearchRemark
    {
        [Column(IsIdentity = true)]
        public long Id { get; set; }

        public string Desc { get; set; }

        public string ClientIp { get; set; }

        public string ServiceName { get; set; }

        public string KeyWord { get; set; }

        public string Query { get; set; }

        public DateTime CreateDate { get; set; }

        public SearchRemarkDto ToDto()
        {
            return new SearchRemarkDto
            {
                Desc = Desc,
                ClientIp = ClientIp,
                ServiceName = ServiceName,
                KeyWord = KeyWord,
                Query = Query
            };
        }

        public SearchRecord ToRecord()
        {
            var now = DateTime.Now;
            return new SearchRecord
            {
                ClientIp = ClientIp,
                ServiceName = ServiceName,
                KeyWord = KeyWord,
                Query = Query,
                BeginDate = now.ToString("yyyy-MM-dd 00:00:00"),
                EndDate = now.AddDays(1).ToString("yyyy-MM-dd 00:00:00")
            };
        }
    }

    public class SearchRemarkDto : ObservableObject
    {
        private string desc;
        public string Desc { get => desc; set => SetProperty(ref desc, value); }

        private string clientIp;
        public string ClientIp { get => clientIp; set => SetProperty(ref clientIp, value); }

        private string serviceName;
        public string ServiceName { get => serviceName; set => SetProperty(ref serviceName, value); }

        private string keyWord;
        public string KeyWord { get => keyWord; set => SetProperty(ref keyWord, value); }

        private string query;
        public string Query { get => query; set => SetProperty(ref query, value); }

        public SearchRemark ToEntity()
        {
            return new SearchRemark
            {
                Desc = Desc,
                ClientIp = ClientIp,
                ServiceName = ServiceName,
                KeyWord = KeyWord,
                Query = Query,
                CreateDate = DateTime.Now,
            };
        }
    }
}
