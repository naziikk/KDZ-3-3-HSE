using System.Text;
using System.Text.Json;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Botik;
public class JsonProcessing
{
    /// <summary>
    /// Метод считывает данные с json файла.
    /// </summary>
    /// <param name="stream"></param>
    /// <returns> массив объектов </returns>
    public static List<Monument> Read(MemoryStream stream)
    {
        stream.Seek(0, SeekOrigin.Begin); 
        using (StreamReader reader = new StreamReader(stream))
        {
            string jsonData = reader.ReadToEnd(); 
            if (!string.IsNullOrEmpty(jsonData))
            {
                return JsonSerializer.Deserialize<List<Monument>>(jsonData);
            }
        }
        return new List<Monument>(); 
    }
    /// <summary>
    /// Метод Json записывает данные в поток.
    /// </summary>
    /// <param name="lst"></param>
    /// <returns> поток </returns>
    public static MemoryStream Write(List<Monument> lst)
    {
        MemoryStream stream = new MemoryStream();
        string json = JsonSerializer.Serialize(lst, new JsonSerializerOptions { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
        byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
        stream.Write(jsonBytes, 0, jsonBytes.Length);
        stream.Seek(0, SeekOrigin.Begin);
        return stream;
    }
}