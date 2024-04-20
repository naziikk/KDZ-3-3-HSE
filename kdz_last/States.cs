namespace Botik;
public class States
{
    // enum нужен для отслеживания состояний, буквально userState проверяет на каком моменте логики программы пользователь
    public enum UserState
    {
        Default,
        waitForCsv,
        waitForJson,
        waitForFile,
        GotCsv,
        GotJson,
        FieldSelection,
        SortingSelection,
        ChoosingFormat,
        FilterBySculpName,
        FilterByLocationPlace,
        FilterByMMM,
    }
}