using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ErrorEventArgs = Newtonsoft.Json.Serialization.ErrorEventArgs;

namespace samplenuunlmanimator;

internal class Json
{
    public static T Load<T>(string path) where T : new()
    {
        bool flag = File.Exists(path);
        if (!flag)
        {
            Save(new T(), path);
        }
        string str = File.ReadAllText(path, Encoding.UTF8);
        JsonSerializerSettings settings = new JsonSerializerSettings
        {
            Error = delegate (object? se, ErrorEventArgs ev)
            {
                ev.ErrorContext.Handled = true;
            }
        };
        T obj = JsonConvert.DeserializeObject<T>(str.Replace("\\", "\\\\"), settings);
        if (flag)
        {
            Save(obj, path);
        }
        return obj;
    }

    public static void Save(object obj, string path)
    {
        string contents = JsonConvert.SerializeObject(obj, Formatting.Indented).Replace("\\\\", "\\");
        try
        {
            File.WriteAllText(path, contents);
        }
        catch
        {
        }
    }

    public static T LoadFromString<T>(string content) where T : new()
    {
        JsonSerializerSettings settings = new JsonSerializerSettings
        {
            Error = delegate (object? se, ErrorEventArgs ev)
            {
                ev.ErrorContext.Handled = true;
            }
        };
        return JsonConvert.DeserializeObject<T>(content.Replace("\\", "\\\\"), settings);
    }
}
