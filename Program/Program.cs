namespace Botik;
class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            var bot = new Boticheck("7121570922:AAH8FUWpmZk4xWBxhr7aC5nlHORwamYCF9U");
            await bot.StartBotAsync();
            await Task.Delay(-1);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}