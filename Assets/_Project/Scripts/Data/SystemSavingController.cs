using System.IO;
using UnityEngine;

public class SystemSavingController : MonoBehaviour
{
    #region Variables
    public static SystemSavingController Instance { get; private set; }
    
    [SerializeField]private GameSettings _settings;

    private string _path;

    #endregion

    #region Unity Standard Methods

    void OnValidate()
    {
        if (_settings == null)
            DevLog.LogError($"[{this}]: GameSettings reference is null", this);
    }

    void Awake()
    {
        _path = Path.Combine(Application.persistentDataPath, "settings.json");

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #endregion

    #region Class Methods
    public void LoadSettings()
    {
        if (File.Exists(_path))
        {
            string json = File.ReadAllText(_path);

            JsonUtility.FromJsonOverwrite(json, _settings);
            DevLog.Log($"Impostazioni caricate al path: {_path}");
        }
        else
        {
            SaveSettings();
        }
    }

    public void SaveSettings()
    {
        string json = JsonUtility.ToJson(_settings, true);

        File.WriteAllText(_path, json);
        DevLog.Log("Impostazioni salvate in: " + _path);
    }

    #endregion
}
