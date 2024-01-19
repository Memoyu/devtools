using System.IO;
using System.Text.Json;

namespace DevTools.Common
{
    public class FileUtil
    {
        public static T? JsonFileReaderToObj<T>(string path)
        {
            if (path == null) throw new ArgumentNullException("path is not null");
            if (!File.Exists(path)) return default;
            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<T>(json);
        }
    }
}
