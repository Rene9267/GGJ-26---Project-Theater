using Cysharp.Threading.Tasks;
using UnityEngine.Localization.Settings;

public static class LocalizationService
{
    public const string MainTable = "Main Table";

    public static async UniTask<string> GetLocalizedStringAsync(string table, string key)
    {
        var handle = LocalizationSettings.StringDatabase.GetLocalizedStringAsync(table, key);
        await handle.Task;
        return handle.Result ?? key;
    }
}
