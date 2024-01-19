using FreeSql.DataAnnotations;

namespace DevTools.Models
{
    [Table(Name = "UserLoginRecord")]
    public class UserLoginRecord
    {
        [Column(IsIdentity = true)]
        public long Id { get; set; }

        public int Env { get; set; }

        public string Cookies { get; set; }

        public DateTime LoginDate { get; set; }

        public DateTime ExpiredDate { get; set; }
    }

    public class UserLoginCookie
    {
        public string Name { get; set; }

        public string Value { get; set; }
    }
}
