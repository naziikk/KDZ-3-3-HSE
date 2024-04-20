using Telegram.Bot;
namespace Botik;
public class Helpers
{
    /// <summary>
    /// Форматирует двумерный массив строк в формат CSV.
    /// </summary>
    /// <param name="ar">Двумерный массив строк для форматирования в CSV формат.</param>
    /// <returns>Массив строк, представляющий собой CSV данные.</returns>
    public static string[] CsvFormat(string[][] ar)
    {
        try
        {
            string[] arr = new string[ar.Length+2];
            arr[0] = "\"ID\";\"SculpName\";\"Photo\";\"Author\";" +
                     "\"ManufactYear\";\"Material\";" +
                     "\"Description\";\"LocationPlace\";\"Longitude_WGS84\";\"" +
                     "Latitude_WGS84\";\"global_id\";" +
                     "\"geodata_center\";\"geoarea\";";
            
            arr[1] = "\"Код\";\"Наименование скульптуры\";\"Фотография\";\"Автор\";" +
                     "\"Год изготовления\";\"Материал изготовления\";" +
                     "\"Описание\";\"Месторасположение\";\"Долгота в WGS-84\";\"" +
                     "Широта в WGS-84\";\"global_id\";" +
                     "\"geodata_center\";\"geoarea\";";
            for (int i = 0; i < ar.Length; i++)
            {
                arr[i+2] = '"' + String.Join("\";\"", ar[i]) + "\";";
            }
            return arr;
        }
        catch
        {
            Console.WriteLine("Ошибка");
            return null;
        }
        return null;
    }
    /// <summary>
    /// Формирует двумерный массив строк из List<Monument> для последующего форматирования в CSV.
    /// </summary>
    /// <param name="monuments">Список монументов для преобразования в CSV формат.</param>
    /// <returns>Двумерный массив строк, представляющий собой данные для CSV.</returns>
    public static string[][] PreCsvFormat(List<Monument> monuments)
    {
        try
        {
            if (monuments.Count == 0)
            {
                Console.WriteLine("Список пуст.");
                return null;
            }

            string[][] csvData = new string[monuments.Count][];
            for (int i = 0; i < monuments.Count; i++)
            {
                csvData[i] = new string[]
                {
                    monuments[i].Id,
                    monuments[i].SculpName,
                    monuments[i].Photo,
                    monuments[i].Author,
                    monuments[i].ManufactYear,
                    monuments[i].Material,
                    monuments[i].Description,
                    monuments[i].LocationPlace,
                    monuments[i].LongitudeWGS84,
                    monuments[i].LatitudeWGS84,
                    monuments[i].GlobalId,
                    monuments[i].GeoDataCenter,
                    monuments[i].GeoArea
                };
            }

            return csvData;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при форматировании CSV: {ex.Message}");
            return null;
        }
    }
    /// <summary>
    /// Преобразует список строк, представляющих собой данные CSV, в List<Monument>.
    /// </summary>
    /// <param name="arr">Список строк CSV для преобразования.</param>
    /// <returns>Список монументов, полученных из CSV данных.</returns>
    public static List<Monument> SplitAr(List<string> arr)
    {
        try
        {
            List<Monument> lst = new List<Monument>();
            foreach (var line in arr)
            {
                string[] arr1 = line.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < arr1.Length; i++)
                {
                    string a = "";
                    for (int j = 0; j < arr1[i].Length; j++)
                    {
                        if (arr1[i][j] != '"' && arr1[i][j] != '»' && arr1[i][j] != '«')
                        {
                            a += arr1[i][j];
                        }
                    }
                    arr1[i] = a;
                }
                if (arr1.Length != 13)
                {
                    Console.WriteLine($"Ошибка обработки строки: {line}");
                    continue;
                }
                lst.Add(new Monument(arr1[0], arr1[1], arr1[2], arr1[3], arr1[4], arr1[5], arr1[6], arr1[7],
                    arr1[8], arr1[9], arr1[10], arr1[11], arr1[12]));
            }
            return lst;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return null;
    }
}