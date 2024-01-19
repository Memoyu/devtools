using System.ComponentModel;

namespace DevTools.Common
{
    public enum EnvEnum
    {
        [Description("开发")]
        Dev,
        [Description("投产")]
        Prod
    }

    static class EnumExtensions
    {
        public static string GetDescription(this Enum val)
        {
            var field = val.GetType().GetField(val.ToString());
            var customAttribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
            return customAttribute == null ? val.ToString() : ((DescriptionAttribute)customAttribute).Description;
        }
    }
}
