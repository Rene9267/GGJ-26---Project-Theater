using UnityEngine;

class WindowController : MonoBehaviour
{
    #region Unity Standard Methods

    void Start()
    {
        if (GameSettings.Instance)
            ApplyWindowSettings();
    }

    #endregion
    
    #region Class Methods

    public void ApplyWindowSettings()
    {
        Screen.fullScreenMode = GameSettings.Instance.WindowModeIndex;
        DevLog.Log($"WindowController: Applicata modalità {Screen.fullScreenMode}");
    }

    #endregion
}
