using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
namespace Botik;

public class CsvProcessing
{
    /// <summary>
    /// Метод считывает данные с CSV файла.
    /// </summary>
    /// <param name="stream"></param>
    /// <returns> массив объектов </returns>
    public static List<Monument> Read(Stream stream, ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
        var temp = "";
        List<string> rawData = new List<string>();
        try
        {
            using (StreamReader reader = new StreamReader(stream))
            {
                temp = reader.ReadLine(); 
                reader.ReadLine(); 
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    rawData.Add(line);
                }
            }
            // Проверка на соответствие формату варианта.
            if (rawData.Count > 0 && temp == "\"ID\";\"SculpName\";\"Photo\";\"Author\";" +
                "\"ManufactYear\";\"Material\";" +
                "\"Description\";\"LocationPlace\";\"Longitude_WGS84\";\"" +
                "Latitude_WGS84\";\"global_id\";" +
                "\"geodata_center\";\"geoarea\";")
            {
                List<Monument> list = Helpers.SplitAr(rawData);
                return list;
            }
            else
            {
                botClient.SendTextMessageAsync(chatId,"Этот файл не соответсвует формату моего варианта!", cancellationToken: cancellationToken);
            }
        }
        catch
        {
            botClient.SendTextMessageAsync(chatId,"Ошибка", cancellationToken: cancellationToken);
        }
        return null;
    }
    /// <summary>
    /// Метод CSV записывает данные в поток.
    /// </summary>
    /// <param name="lst"></param>
    /// <returns> поток </returns>
    public static MemoryStream Write(List<Monument> lst)
    {
        string[][] arr = Helpers.PreCsvFormat(lst);
        string[] fileContent = Helpers.CsvFormat(arr);
        MemoryStream stream = new MemoryStream();
        StreamWriter writer = new StreamWriter(stream, Encoding.UTF8);
        foreach (var line in fileContent)
        {
            writer.WriteLine(line);
        }

        writer.Flush();
        stream.Position = 0;

        return stream;
    }
}