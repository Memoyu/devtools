using CommunityToolkit.Mvvm.ComponentModel;
using FreeSql.DataAnnotations;
using DevTools.Common;

namespace DevTools.Models
{
    [Table(Name = "User")]
    public class User
    {
        [Column(IsIdentity = true)]
        public long Id { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public int Env { get; set; }

        public bool Default { get; set; }

        public UserCmbDto ToCmbDto()
        {
            return new UserCmbDto { Id = Id, Display = $"{UserName}-({Password})" };
        }

        public UserDto ToDto()
        {
            return new UserDto { Id = Id, Env = Env, UserName = UserName, Password = Password, Default = Default };
        }
    }

    public class UserCmbDto
    {
        public long Id { get; set; }

        public string Display { get; set; }
    }

    public class UserDto : ObservableObject
    {
        public long Id { get; set; }

        private string userName;
        public string UserName { get => userName; set => SetProperty(ref userName, value); }

        private string password;
        public string Password { get => password; set => SetProperty(ref password, value); }

        public int env;
        public int Env { get => env; set => SetProperty(ref env, value); }

        public string EnvName { get => ((EnvEnum)Env).GetDescription(); set { } }

        private bool @default;
        public bool Default { get => @default; set => SetProperty(ref @default, value); }

        public User ToEntity()
        {
            return new User
            {
                Id = Id,
                UserName = UserName,
                Password = Password,
                Env = Env,
                Default = Default
            };
        }
    }
}
