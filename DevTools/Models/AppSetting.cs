using FreeSql.DataAnnotations;

namespace DevTools.Models
{
    [Table(Name = "AppSetting")]
    public class AppSetting
    {
        [Column(IsIdentity = true)]
        public long Id { get; set; }
    }
}
