using Newtonsoft.Json;
using System.Text;
using ErrorEventArgs = Newtonsoft.Json.Serialization.ErrorEventArgs;

namespace SharpFramework.Utils
{
    /// <summary>
    /// Jsonファイルからオブジェクトを読み書きするクラス。
    /// </summary>
    public class Json
    {
        /// <summary>
        /// ファイルからJSONを読み込み、オブジェクトを返します。
        /// </summary>
        public static T Load<T>(string path) where T : new()
        {
            if (!File.Exists(path))
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
            if (File.Exists(path))
            {
                Save(obj, path);
            }
            return obj;
        }

        /// <summary>
        /// オブジェクトをJSON形式でファイルに保存します。
        /// </summary>
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

        /// <summary>
        /// StringからJSONを読み込み、オブジェクトを返します。
        /// </summary>
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

}