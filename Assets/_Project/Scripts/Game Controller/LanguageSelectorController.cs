using System.Collections;
using UnityEngine;
using UnityEngine.Localization.Settings;

class LanguageSelectorController : MonoBehaviour
{
    #region Variables
    private bool _active = false;
    #endregion

    #region Unity Standard Methods
    void Start()
    {
        if (SystemSavingController.Instance)
            ChangeLanguage(GameSettings.Instance.Language);
    }
    #endregion

    #region Class Methods
    public void ChangeLanguage(int localeID)
    {
        if (LocalizationSettings.SelectedLocale == LocalizationSettings.AvailableLocales.Locales[localeID])
        {
            DevLog.LogWarning("Language is already selected", this);
            return;
        }

        if (_active) return;
        
        DevLog.Log("Procedo al cambio lingua", this);

        StartCoroutine(SetLocale(localeID));
    }

    private IEnumerator SetLocale(int localeID)
    {
        _active = true;
        yield return LocalizationSettings.InitializationOperation;

        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[localeID];
        _active = false;
    }
    #endregion

}