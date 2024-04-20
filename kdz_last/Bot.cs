using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Microsoft.Extensions.Logging;
namespace Botik;
public class Boticheck
{
    ILogger<Boticheck> _logger;
    TelegramBotClient _botClient;
    Dictionary<long, States.UserState> _userStates = new();
    /// <summary>
    /// Конструктор класса Boticheck.
    /// </summary>
    /// <param name="botToken">Токен бота для доступа к API Telegram.</param>
    public Boticheck(string botToken)
    {
        _botClient = new TelegramBotClient(botToken);
        _logger = LoggerFactory.Create(builder =>
        {
            builder.AddConsole().AddFile(options =>
            {
                options.InternalLogFile = Path.Combine("bin", "logs", "bot.log"); // Путь к файлу логов
            }); 
        }).CreateLogger<Boticheck>();
    }
    List<Monument> lst = new();
    /// <summary>
    /// Асинхронный метод для запуска бота.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены для отслеживания отмены операции.</param>
    public async Task StartBotAsync(CancellationToken cancellationToken = default)
    {
        ReceiverOptions receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>(), // receive all update types except ChatMember related updates
            ThrowPendingUpdates = true
        };

        _botClient.StartReceiving(updateHandler: HandleUpdateAsync, pollingErrorHandler: HandlePollingErrorAsync, receiverOptions: receiverOptions, cancellationToken: cancellationToken
        );
        var me = await _botClient.GetMeAsync(cancellationToken: cancellationToken);
        Console.WriteLine($"Start listening for @{me.Username}");
    }
    /// <summary>
    /// Обработка обновлений от Telegram.
    /// </summary>
    /// <param name="botClient">Экземпляр клиента Telegram Bot API.</param>
    /// <param name="update">Обновление, полученное от Telegram.</param>
    /// <param name="cancellationToken">Токен отмены для отслеживания отмены операции.</param>
    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        try
        {
            var message = update.Message;
            var chatId = message.Chat.Id;
            Console.ForegroundColor = ConsoleColor.Cyan;
            _logger.LogInformation($"Получено '{message.Text}' сообщение в чате {chatId}.");
            _logger.LogInformation("Зарегистрировано {message.text}", DateTimeOffset.Now);
            switch (message.Type)
            {
                case MessageType.Document when Path.GetExtension(message.Document.FileName) == ".csv":
                        _logger.LogInformation("Получен CSV файл.");
                    await HandleCsvFileAsync(botClient, message, cancellationToken);
                    return;
                case MessageType.Document when Path.GetExtension(message.Document.FileName) == ".json":
                    _logger.LogInformation("Получен JSON.");
                    await HandleJsonFileAsync(botClient, message, cancellationToken);
                    return;
            }
            if (message.Type == MessageType.Document && !(Path.GetExtension(message.Document.FileName) == ".csv" 
                                                          || Path.GetExtension(message.Document.FileName) == ".json")) {
                _logger.LogInformation("Отправлен файл с неккоректным разширением.");
                await botClient.SendTextMessageAsync(
                    message.Chat, 
                    "Вы отправили файл с некорректным разширением, пожалуйста отправьте другой файл.",
                    cancellationToken: cancellationToken
                );
                return;
            }
            if (message.Text == "")
            {
                _logger.LogInformation("Получено пустое сообщение.");
                return;
            }
            if (!_userStates.TryGetValue(chatId, out var userState))
            {
                _logger.LogInformation("Получено сообщение от нового пользователя.");
                userState = States.UserState.Default;
                _userStates[chatId] = userState;
            }
            switch (userState)
            {
                case States.UserState.FilterBySculpName:
                {
                    if (message.Text != "/start" && message.Text != "/help" && message.Text != "/menu")
                    {
                        _logger.LogInformation("Фильтрация памятников по названию скульптуры.");
                        List<Monument> temp = lst.Where(v => v.SculpName.Contains(message.Text, StringComparison.OrdinalIgnoreCase)).ToList();
                        if (temp.Count != 0)
                        {
                            lst = temp;
                            await botClient.SendTextMessageAsync(chatId, "Данные были отфильтрованы! Введите /menu для продолжения", cancellationToken: cancellationToken);
                            _userStates[chatId] = States.UserState.GotCsv;
                            return;
                        }
                        else
                        {
                            _logger.LogInformation("Совпадений не найдено для фильтрации по названию скульптуры.");
                            await botClient.SendTextMessageAsync(chatId, "Совпадений не нашлось. Введите новое значение", cancellationToken: cancellationToken);
                            return;
                        }
                    }
                    break;
                }
                case States.UserState.FilterByLocationPlace:
                {
                    if (message.Text != "/start" && message.Text != "/help" && message.Text != "/menu")
                    {
                        _logger.LogInformation("Фильтрация памятников по местоположению.");
                        List<Monument> temp = lst.Where(v => v.LocationPlace.Contains(message.Text, StringComparison.OrdinalIgnoreCase)).ToList();
                        if (temp.Count != 0)
                        {
                            lst = temp;
                            await botClient.SendTextMessageAsync(chatId, "Данные были отфильтрованы! Введите /menu для продолжения", cancellationToken: cancellationToken);
                            _userStates[chatId] = States.UserState.GotCsv;
                            return;
                        }
                        else
                        {
                            _logger.LogInformation("Совпадений не найдено для фильтрации по местоположению.");
                            await botClient.SendTextMessageAsync(chatId, "Совпадений не нашлось. Введите новое значение", cancellationToken: cancellationToken);
                            return;
                        }
                    }
                    break;
                }
                case States.UserState.FilterByMMM:
                {
                    if (message.Text != "/start" && message.Text != "/help" && message.Text != "/menu")
                    {
                        _logger.LogInformation("Фильтрация памятников по году изготовления и материалу.");
                        string[] arr = message.Text.Split(' ');
                        if (arr.Length == 2)
                        {
                            int year;
                            if (int.TryParse(arr[0], out year))
                            {
                                List<Monument> temp = lst.Where(v => v.ManufactYear.Contains(arr[0]) && v.Material.Contains(arr[1], StringComparison.OrdinalIgnoreCase)).ToList();
                                if (temp.Count != 0)
                                {
                                    lst = temp;
                                    await botClient.SendTextMessageAsync(chatId, "Данные были отфильтрованы! Введите /menu для продолжения", cancellationToken: cancellationToken);
                                    _userStates[chatId] = States.UserState.GotCsv;
                                    return;
                                }
                                else
                                {
                                    _logger.LogInformation("Совпадений не найдено для фильтрации по году изготовления и материалу.");
                                    await botClient.SendTextMessageAsync(chatId, "Совпадений не нашлось. Введите два значения через пробел. Например: 9 Бронза.", cancellationToken: cancellationToken);
                                    return;
                                }
                            }
                            else
                            {
                                _logger.LogInformation("Неверный ввод для года изготовления.");
                                await botClient.SendTextMessageAsync(chatId, "Первое значение должно быть целым числом. Введите два значения через пробел. Например: 9 Бронза.", cancellationToken: cancellationToken);
                                return;
                            }
                        }
                        else
                        {
                            _logger.LogInformation("Неверный ввод для года изготовления и материала.");
                            await botClient.SendTextMessageAsync(chatId, "Введите два значения через пробел. Например: 9 Бронза.", cancellationToken: cancellationToken);
                            return;
                        }
                    }
                    break;
                }
            }
        switch (message.Text)
        {
            case "/start":
                _logger.LogInformation("Получена команда '/start'.");
                await botClient.SendTextMessageAsync(chatId, $"Здравствуйте, {message.From.FirstName}!", cancellationToken: cancellationToken);
                await AskForCsvOrJsonFile(botClient, chatId, cancellationToken);
                _userStates[chatId] = States.UserState.waitForFile;
                break;
            case "JSON":
                if (_userStates[chatId] == States.UserState.waitForFile)
                {
                    _logger.LogInformation("Запрошен Json файл.");
                    await AskForJsonFile(botClient, chatId, cancellationToken);
                }
                else
                {
                    await botClient.SendTextMessageAsync(chatId, $"Я вас не понимаю.", cancellationToken: cancellationToken);
                }
                break;
            case "CSV":
                if (_userStates[chatId] == States.UserState.waitForFile)
                {
                    _logger.LogInformation("Запрошен Json файл.");
                    await AskForCsvFile(botClient, chatId, cancellationToken);
                }
                else
                {
                    await botClient.SendTextMessageAsync(chatId, $"Я вас не понимаю.", cancellationToken: cancellationToken);
                }
                break;
            case "/help":
                _logger.LogInformation("Получена команда '/help'.");
                string helpText = @"
*Скачать обработанный файл в формате CSV или JSON*
_Доступно после предоставления CSV или JSON файла_

*Отсортировать по одному из полей*
_Доступно после предоставления CSV или JSON файла_

*Загрузить CSV файл на обработку*

*Произвести выборку по одному из полей*
_Доступно после предоставления CSV или JSON файла_

*Загрузить JSON файл на обработку*
_Доступно после предоставления CSV или JSON файла_
";
                await botClient.SendTextMessageAsync(chatId, helpText, parseMode: ParseMode.Markdown, cancellationToken: cancellationToken);
                break;
            case "/menu":
                _logger.LogInformation("Получена команда '/menu'.");
                if (userState == States.UserState.GotCsv || userState == States.UserState.FieldSelection || userState == States.UserState.ChoosingFormat || userState == States.UserState.SortingSelection || userState == States.UserState.GotJson)
                {
                    await SendStartKeyboard(botClient, chatId, cancellationToken);
                }
                else
                {
                    await AskForCsvOrJsonFile(botClient, chatId, cancellationToken);
                    _userStates[chatId] = States.UserState.waitForFile;
                }
                break;
            case "Загрузить CSV файл на обработку":
                _logger.LogInformation("Получена команда 'Загрузить CSV файл на обработку'.");
                await AskForCsvFile(botClient, chatId, cancellationToken);
                break;
            case "Произвести выборку по одному из полей":
                _logger.LogInformation("Получена команда 'Произвести выборку по одному из полей'.");
                if (lst.Count != 0)
                {
                    await SendFieldSelectionMenu(botClient, chatId, cancellationToken);
                    _userStates[chatId] = States.UserState.FieldSelection;
                }
                else
                {
                    await AskForCsvOrJsonFile(botClient, chatId, cancellationToken);
                    _userStates[chatId] = States.UserState.waitForFile;
                }
                break;
            case "Отсортировать по одному из полей":
                _logger.LogInformation("Получена команда 'Отсортировать по одному из полей'.");
                if (lst.Count != 0)
                {
                    await SendSortingOptionsMenu(botClient, chatId, cancellationToken);
                    _userStates[chatId] = States.UserState.SortingSelection;
                }
                else
                {
                    await AskForCsvOrJsonFile(botClient, chatId, cancellationToken);
                    _userStates[chatId] = States.UserState.waitForFile;
                }
                break;
            case "Скачать обработанный файл в формате CSV или JSON":
                _logger.LogInformation("Получена команда 'Скачать обработанный файл в формате CSV или JSON'.");
                if (lst.Count != 0)
                {
                    SendFileFormatSelectionMenu(botClient, chatId, cancellationToken);
                    _userStates[chatId] = States.UserState.ChoosingFormat;
                }
                else
                {
                    await AskForCsvOrJsonFile(botClient, chatId, cancellationToken);
                    _userStates[chatId] = States.UserState.waitForFile;
                }
                break;
            case "Загрузить JSON файл на обработку":
                _logger.LogInformation("Получена команда 'Загрузить JSON файл на обработку'.");
                if (lst.Count != 0 && userState == States.UserState.GotCsv || userState == States.UserState.FieldSelection || userState == States.UserState.ChoosingFormat || userState == States.UserState.SortingSelection)
                {
                    await AskForJsonFile(botClient, chatId, cancellationToken);
                }
                else
                {
                    await AskForCsvOrJsonFile(botClient, chatId, cancellationToken);
                    _userStates[chatId] = States.UserState.waitForFile;
                }
                break;
            case "SculpName":
                _logger.LogInformation("Получена команда 'SculpName'.");
                if (userState == States.UserState.FieldSelection)
                    await SculpNameChoice(botClient, chatId, cancellationToken);
                else
                    await botClient.SendTextMessageAsync(chatId, "Извините, я не понимаю ваш запрос.", cancellationToken: cancellationToken);
                break;
            case "LocationPlace":
                _logger.LogInformation("Получена команда 'LocationPlace'.");
                if (userState == States.UserState.FieldSelection)
                    await LocationPlaceChoice(botClient, chatId, cancellationToken);
                else
                    await botClient.SendTextMessageAsync(chatId, "Извините, я не понимаю ваш запрос.", cancellationToken: cancellationToken);
                break;
            case "ManufactYear и Material":
                _logger.LogInformation("Получена команда 'ManufactYear и Material'.");
                if (userState == States.UserState.FieldSelection)
                    await ManufactYearMaterialChoice(botClient, chatId, cancellationToken);
                else
                    await botClient.SendTextMessageAsync(chatId, "Извините, я не понимаю ваш запрос.", cancellationToken: cancellationToken);
                break;
            case "SculpName по алфавиту в прямом порядке":
                _logger.LogInformation("Получена команда 'SculpName по алфавиту в прямом порядке'.");
                if (userState == States.UserState.SortingSelection)
                    await SculpNameSortingAcending(botClient, chatId, cancellationToken);
                else
                    await botClient.SendTextMessageAsync(chatId, "Извините, я не понимаю ваш запрос.", cancellationToken: cancellationToken);
                break;
            case "ManufactYear по убыванию":
                _logger.LogInformation("Получена команда 'ManufactYear по убыванию'.");
                if (userState == States.UserState.SortingSelection)
                    await ManufactYearSortingByDescending(botClient, chatId, cancellationToken);
                else
                    await botClient.SendTextMessageAsync(chatId, "Извините, я не понимаю ваш запрос.", cancellationToken: cancellationToken);
                break;
            case "Скачать в формате CSV":
                _logger.LogInformation("Получена команда 'Скачать в формате CSV'.");
                if (userState == States.UserState.ChoosingFormat)
                {
                    ReturnCsvFileToUser(botClient, chatId, cancellationToken);
                }
                else
                    await botClient.SendTextMessageAsync(chatId, "Извините, я не понимаю ваш запрос.", cancellationToken: cancellationToken);
                break;
            case "Скачать в формате JSON":
                _logger.LogInformation("Получена команда 'Скачать в формате JSON'.");
                if (userState == States.UserState.ChoosingFormat)
                {
                    ReturnJsonFileToUser(botClient, chatId, cancellationToken);
                }
                else
                    await botClient.SendTextMessageAsync(chatId, "Извините, я не понимаю ваш запрос.", cancellationToken: cancellationToken);
                break;
            default:
                _logger.LogInformation("Получена неизвестная команда.");
                await botClient.SendTextMessageAsync(chatId, "У меня нет такой команды..", cancellationToken: cancellationToken);
                break;
        }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Ошибка при обновлении данных.");
        }
    }
    /// <summary>
    /// Отправляет пользователю файл в формате JSON.
    /// </summary>
    /// <param name="botClient">Экземпляр клиента Telegram Bot API.</param>
    /// <param name="chatId">Идентификатор чата с пользователем.</param>
    /// <param name="cancellationToken">Токен отмены для отслеживания отмены операции.</param>
    public async Task ReturnJsonFileToUser(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
        try
        {
            if (lst.Count == 0)
            {
                await botClient.SendTextMessageAsync(chatId, "Список объектов пуст. Нет данных для сохранения.", cancellationToken: cancellationToken);
                return;
            }
            MemoryStream stream = JsonProcessing.Write(lst);
            string fileName = "BotikOtrabotalOtlichno.json";

            await botClient.SendDocumentAsync(chatId: chatId, document: InputFile.FromStream(stream, fileName), caption: "Обработанный файл", cancellationToken: cancellationToken);
            _logger.LogInformation("JSON файл успешно отправлен пользователю.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при отправке JSON файла пользователю.");
            await botClient.SendTextMessageAsync(chatId, "Произошла ошибка при скачивании JSON-файла.", cancellationToken: cancellationToken);
        }
    }
    /// <summary>
    /// Отправляет пользователю файл в формате CSV.
    /// </summary>
    /// <param name="botClient">Экземпляр клиента Telegram Bot API.</param>
    /// <param name="chatId">Идентификатор чата с пользователем.</param>
    /// <param name="cancellationToken">Токен отмены для отслеживания отмены операции.</param>
    public async Task ReturnCsvFileToUser(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
        try
        {
            MemoryStream stream = CsvProcessing.Write(lst);
            string fileName = "BotikOtrabotalOtlichno.csv";
            await botClient.SendDocumentAsync(chatId: chatId, document: InputFile.FromStream(stream, fileName), caption: "Обработанный файл");
            _logger.LogInformation("CSV файл успешно отправлен пользователю.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при отправке CSV файла пользователю.");
            await botClient.SendTextMessageAsync(chatId, "Произошла ошибка при скачивании CSV-файла.", cancellationToken: cancellationToken);
        }
    }

    /// <summary>
    /// Отправляет меню выбора формата файла для скачивания.
    /// </summary>
    /// <param name="botClient">Экземпляр клиента Telegram Bot API.</param>
    /// <param name="chatId">Идентификатор чата с пользователем.</param>
    /// <param name="cancellationToken">Токен отмены для отслеживания отмены операции.</param>
    public async Task SendFileFormatSelectionMenu(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
        ReplyKeyboardMarkup replyKeyboardMarkup = new ReplyKeyboardMarkup(new[]
        {
            new KeyboardButton[] { "Скачать в формате CSV" },
            new KeyboardButton[] { "Скачать в формате JSON" }
        })
        {
            ResizeKeyboard = true,
            OneTimeKeyboard = true
        };
        await botClient.SendTextMessageAsync(chatId, "Выберите формат файла для скачивания:", replyMarkup: replyKeyboardMarkup, cancellationToken: cancellationToken);
        _logger.LogInformation("Отправлено меню выбора формата файла для скачивания.");
    }
    /// <summary>
    /// Отправляет меню выбора формата файла для скачивания.
    /// </summary>
    /// <param name="botClient">Экземпляр клиента Telegram Bot API.</param>
    /// <param name="chatId">Идентификатор чата с пользователем.</param>
    /// <param name="cancellationToken">Токен отмены для отслеживания отмены операции.</param>
    public async Task AskForCsvFile(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
        await botClient.SendTextMessageAsync(chatId, "Пожалуйста, отправьте мне CSV файл:", cancellationToken: cancellationToken);
        _userStates[chatId] = States.UserState.waitForCsv;
        _logger.LogInformation("Запрошен CSV файл.");
        
    }
    /// <summary>
    /// Отправляет меню выбора формата файла.
    /// </summary>
    /// <param name="botClient">Экземпляр клиента Telegram Bot API.</param>
    /// <param name="chatId">Идентификатор чата с пользователем.</param>
    /// <param name="cancellationToken">Токен отмены для отслеживания отмены операции.</param>
    public async Task AskForCsvOrJsonFile(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
        ReplyKeyboardMarkup replyKeyboardMarkup = new ReplyKeyboardMarkup(new[]
        {
            new KeyboardButton[] { "CSV" },
            new KeyboardButton[] { "JSON" }
        })
        {
            ResizeKeyboard = true,
            OneTimeKeyboard = true
        };
        await botClient.SendTextMessageAsync(chatId, "Выберите формат файла:", replyMarkup: replyKeyboardMarkup, cancellationToken: cancellationToken);
        _logger.LogInformation("Отправлено меню выбора формата файла.");
    }
    public async Task AskForJsonFile(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
        await botClient.SendTextMessageAsync(chatId, "Пожалуйста, отправьте мне JSON файл:", cancellationToken: cancellationToken);
        _userStates[chatId] = States.UserState.waitForJson;
        _logger.LogInformation("Запрошен JSON файл.");
    }
    /// <summary>
    /// Обрабатывает загрузку CSV файла.
    /// </summary>
    /// <param name="botClient">Экземпляр клиента Telegram Bot API.</param>
    /// <param name="message">Сообщение, содержащее файл.</param>
    /// <param name="cancellationToken">Токен отмены для отслеживания отмены операции.</param>
    public async Task HandleCsvFileAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        try
        {
            var chatId = message.Chat.Id;
            var file = await botClient.GetFileAsync(message.Document.FileId, cancellationToken);
            if (_userStates[chatId] == States.UserState.GotJson)
            {
                await botClient.SendTextMessageAsync(chatId, @"*Не удалось получить файл или вы передали файл неккоректного формата.*",parseMode:ParseMode.Markdown, cancellationToken: cancellationToken);
                return;
            }
            
            using (var stream = new MemoryStream())
            {
                await botClient.DownloadFileAsync(file.FilePath, stream, cancellationToken);

                stream.Position = 0;

                lst = CsvProcessing.Read(stream, botClient, chatId, cancellationToken: cancellationToken);
            }

            if (lst.Count != 0)
            {
                await botClient.SendTextMessageAsync(chatId, $"Файл {Path.GetFileName(message.Text)} успешно загружен.", cancellationToken: cancellationToken);
                await SendStartKeyboard(botClient, chatId, cancellationToken);
                _userStates[chatId] = States.UserState.GotCsv;
                _logger.LogInformation($"Файл {Path.GetFileName(message.Text)} успешно загружен.");
            }
            else
            {
                await botClient.SendTextMessageAsync(chatId, "Некорректный путь к файлу или файл не является CSV файлом. Пожалуйста, отправьте мне корректный путь к файлу с CSV данными:", cancellationToken: cancellationToken);
                _logger.LogError("Некорректный путь к файлу или файл не является CSV файлом.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке CSV-файла.");
            Console.WriteLine($"Ошибка при обработке CSV-файла: {ex.Message}");
        }
    }
    /// <summary>
    /// Обрабатывает загрузку JSON файла.
    /// </summary>
    /// <param name="botClient">Экземпляр клиента Telegram Bot API.</param>
    /// <param name="message">Сообщение, содержащее файл.</param>
    /// <param name="cancellationToken">Токен отмены для отслеживания отмены операции.</param>
    public async Task HandleJsonFileAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        try
        {
            var chatId = message.Chat.Id;
            var file = await botClient.GetFileAsync(message.Document.FileId, cancellationToken);
            if (_userStates[chatId] == States.UserState.waitForCsv)
            {
                await botClient.SendTextMessageAsync(chatId, @"*Не удалось получить файл или вы передали файл неккоректного формата.*",parseMode:ParseMode.Markdown, cancellationToken: cancellationToken);
                return;
            }

            using (var stream = new MemoryStream())
            {
                await botClient.DownloadFileAsync(file.FilePath, stream, cancellationToken);
                stream.Position = 0;
                lst = JsonProcessing.Read(stream);
            }

            if (lst.Count != 0)
            {
                await botClient.SendTextMessageAsync(chatId, $"Файл {Path.GetFileName(message.Text)} успешно загружен.", cancellationToken: cancellationToken);
                await SendStartKeyboard(botClient, chatId, cancellationToken);
                _userStates[chatId] = States.UserState.GotJson;
                _logger.LogInformation($"Файл {Path.GetFileName(message.Text)} успешно загружен.");
            }
            else
            {
                await botClient.SendTextMessageAsync(chatId, "Некорректный путь к файлу или файл не является JSON файлом. Пожалуйста, отправьте мне корректный путь к файлу с JSON данными:", cancellationToken: cancellationToken);
                _logger.LogError("Некорректный путь к файлу или файл не является JSON файлом.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке JSON-файла.");
            Console.WriteLine($"Ошибка при обработке JSON-файла: {ex.Message}");
        }
    }
    /// <summary>
    /// Отправляет пользователю клавиатуру стартового меню.
    /// </summary>
    /// <param name="botClient">Экземпляр клиента Telegram Bot API.</param>
    /// <param name="chatId">Идентификатор чата с пользователем.</param>
    /// <param name="cancellationToken">Токен отмены для отслеживания отмены операции.</param>
    public async Task SendStartKeyboard(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
        try
        {
            ReplyKeyboardMarkup replyKeyboardMarkup = new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton[] { "Загрузить CSV файл на обработку", "Произвести выборку по одному из полей" },
                new KeyboardButton[] { "Отсортировать по одному из полей", "Скачать обработанный файл в формате CSV или JSON" },
                new KeyboardButton[] { "Загрузить JSON файл на обработку" }
            })
            {
                ResizeKeyboard = true,
                OneTimeKeyboard = true
            };

            await botClient.SendTextMessageAsync(chatId, "Выберите действие:", replyMarkup: replyKeyboardMarkup, cancellationToken: cancellationToken);
            _logger.LogInformation("Отправлено стартовое меню.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при отправке стартового меню.");
        }
    }
    // <summary>
    /// Отправляет пользователю меню выбора поля для запроса.
    /// </summary>
    /// <param name="botClient">Экземпляр клиента Telegram Bot API.</param>
    /// <param name="chatId">Идентификатор чата с пользователем.</param>
    /// <param name="cancellationToken">Токен отмены для отслеживания отмены операции.</param>
    public async Task SendFieldSelectionMenu(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
        try
        {
            ReplyKeyboardMarkup replyKeyboardMarkup = new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton[] { "SculpName", "LocationPlace" },
                new KeyboardButton[] { "ManufactYear и Material" },
            })
            {
                ResizeKeyboard = true,
                OneTimeKeyboard=true
            };
            await botClient.SendTextMessageAsync(chatId, "Выберите по какому из полей совершить выборку:", replyMarkup: replyKeyboardMarkup, cancellationToken: cancellationToken);
            _logger.LogInformation("Отправлено меню выбора поля для запроса.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при отправке меню выбора поля для запроса.");
            throw;
        }
    }
    // <summary>
    /// Отправляет пользователю меню выбора поля для сортировки.
    /// </summary>
    /// <param name="botClient">Экземпляр клиента Telegram Bot API.</param>
    /// <param name="chatId">Идентификатор чата с пользователем.</param>
    /// <param name="cancellationToken">Токен отмены для отслеживания отмены операции.</param>
    public async Task SendSortingOptionsMenu(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
        try
        {
            ReplyKeyboardMarkup replyKeyboardMarkup = new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton[] { "SculpName по алфавиту в прямом порядке" },
                new KeyboardButton[] { "ManufactYear по убыванию" },
            })
            {
                ResizeKeyboard = true,
                OneTimeKeyboard=true
            };
            await botClient.SendTextMessageAsync(chatId, "Выберите по какому из полей отсортировать данные:", replyMarkup: replyKeyboardMarkup, cancellationToken: cancellationToken);
            _logger.LogInformation("Отправлено меню выбора поля для сортировки.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при отправке меню выбора поля для сортировки.");
            throw;
        }
    }
    /// <summary>
    /// Совершает выборку (все строки файла с конкретным значением) по полю SculpName.
    /// </summary>
    /// <param name="botClient">Экземпляр клиента Telegram Bot API.</param>
    /// <param name="chatId">Идентификатор чата с пользователем.</param>
    /// <param name="cancellationToken">
    public async Task SculpNameChoice(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
         _userStates[chatId] = States.UserState.FilterBySculpName;
        await botClient.SendTextMessageAsync(chatId, "Отправьте текстовое значение для фильтрации.", cancellationToken: cancellationToken);
        _logger.LogInformation("Пользователь выбирает поле SculpName для фильтрации.");
    }
    /// <summary>
    /// Совершает выборку (все строки файла с конкретным значением) по полю LocationPlace.
    /// </summary>
    /// <param name="botClient">Экземпляр клиента Telegram Bot API.</param>
    /// <param name="chatId">Идентификатор чата с пользователем.</param>
    /// <param name="cancellationToken">
    public async Task LocationPlaceChoice(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
        await botClient.SendTextMessageAsync(chatId, "Отправьте текстовое значение для фильтрации.", cancellationToken: cancellationToken);
        _userStates[chatId] = States.UserState.FilterByLocationPlace;
        _logger.LogInformation("Пользователь выбирает поле LocationPlace для фильтрации.");
    }
    /// <summary>
    /// Совершает выборку (все строки файла с конкретным значением) по полям ManufactYear и Material .
    /// </summary>
    /// <param name="botClient">Экземпляр клиента Telegram Bot API.</param>
    /// <param name="chatId">Идентификатор чата с пользователем.</param>
    /// <param name="cancellationToken">
    public async Task ManufactYearMaterialChoice(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
        await botClient.SendTextMessageAsync(chatId, "Отправьте данные через пробел (год материал).\n" +
                                                     "Пример: 9 Бронза", cancellationToken: cancellationToken);
        _userStates[chatId] = States.UserState.FilterByMMM;
        _logger.LogInformation("Пользователь выбирает поля ManufactYear и Material для фильтрации.");
    }
    /// <summary>
    /// Сортирует по полю SculpName.
    /// </summary>
    /// <param name="botClient">Экземпляр клиента Telegram Bot API.</param>
    /// <param name="chatId">Идентификатор чата с пользователем.</param>
    /// <param name="cancellationToken">
    public async Task SculpNameSortingAcending(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
        await botClient.SendTextMessageAsync(chatId,"Прекрасный выбор!", cancellationToken: cancellationToken);
        List<Monument> temp = lst.OrderBy(el => el.SculpName).ToList();
        lst = temp;
        botClient.SendTextMessageAsync(chatId,"Данные были обновлены! Введите /menu для дальнейшей работы.", cancellationToken: cancellationToken);
        _logger.LogInformation("Список отсортирован по полю SculpName в прямом порядке.");
    }
    /// <summary>
    /// Сортирует по полю ManufactYear.
    /// </summary>
    /// <param name="botClient">Экземпляр клиента Telegram Bot API.</param>
    /// <param name="chatId">Идентификатор чата с пользователем.</param>
    /// <param name="cancellationToken">
    public async Task ManufactYearSortingByDescending(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
        try
        {
            await botClient.SendTextMessageAsync(chatId, "Прекрасный выбор!", cancellationToken: cancellationToken);
            
            List<Monument> temp = lst.OrderByDescending(el => 
            {
                int year;
                return int.TryParse(el.ManufactYear, out year) ? year : 0;
            }).ToList();
            lst = temp;
            await botClient.SendTextMessageAsync(chatId, "Данные были обновлены! Введите /menu для дальнейшей работы.", cancellationToken: cancellationToken);
            _logger.LogInformation("Список отсортирован по полю ManufactYear в прямом порядке.");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    /// <summary>
    /// Обрабатывает ошибки при работе с механизмом опроса Telegram.
    /// </summary>
    /// <param name="botClient">Экземпляр клиента Telegram Bot API.</param>
    /// <param name="exception">Исключение, возникшее при работе с механизмом опроса.</param>
    /// <param name="cancellationToken">Токен отмены для отслеживания отмены операции.</param>
    public async Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        try
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
                
            };
            Console.WriteLine(ErrorMessage);
            _logger.LogError(exception, "Ошибка при работе с механизмом опроса Telegram.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке ошибки опроса Telegram.");
        }
    }
}