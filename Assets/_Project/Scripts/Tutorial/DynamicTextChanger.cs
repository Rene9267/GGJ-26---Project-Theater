using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization;



public class DynamicTextChanger : MonoBehaviour
{
    [System.Serializable]
    public struct InputIcon
    {
        public string ActionName;
        public string KeyboardIcon;
        public string GamepadIcon;
    }

    #region Parameters
    [Header("Componenti")]
    [SerializeField] private PlayerInput _playerInput;
    [SerializeField] private TextMeshProUGUI _tutorialText;

    [Header("Localizzazione e Icone")]
    [Tooltip("Inserisci qui il riferimento alla tua stringa localizzata (che deve contenere {0})")]
    [SerializeField] private LocalizedString _localizedText;

    [Tooltip("L'elemento 0 sostituirà {0}, l'elemento 1 sostituirà {1}, ecc.")]
    public InputIcon[] DynamicIcons;

    #endregion

    #region Unity Methods

    void OnValidate()
    {
        if (_playerInput == null)
            DevLog.LogWarning("Il riferimento al componente PlayerInput non è stato assegnato. Assicurati di assegnarlo nel Inspector.", this);
        if (_tutorialText == null)
            DevLog.LogWarning("Il riferimento al componente TextMeshProUGUI non è stato assegnato. Assicurati di assegnarlo nel Inspector.", this);
        if (_localizedText == null)
            DevLog.LogWarning("Il riferimento alla stringa localizzata non è stato assegnato. Assicurati di assegnarlo nel Inspector.", this);
    }

    private void OnEnable()
    {
        if (_playerInput != null)
            _playerInput.onControlsChanged += UpdateIcon;

        if (_playerInput != null)
            SetupArguments(_playerInput);

        if (_localizedText != null)
            _localizedText.StringChanged += OnStringChanged;
    }

    private void OnDisable()
    {
        if (_playerInput != null)
            _playerInput.onControlsChanged -= UpdateIcon;

        if (_localizedText != null)
            _localizedText.StringChanged -= OnStringChanged;
    }

    #endregion

    #region Class Methods

    private void SetupArguments(PlayerInput input)
    {
        if (input == null || DynamicIcons == null) return;

        bool isGamepad = input.currentControlScheme == "Gamepad";
        object[] argomenti = new object[DynamicIcons.Length];

        for (int i = 0; i < DynamicIcons.Length; i++)
        {
            argomenti[i] = isGamepad ? DynamicIcons[i].GamepadIcon : DynamicIcons[i].KeyboardIcon;
        }

        _localizedText.Arguments = argomenti;
    }

    private void UpdateIcon(PlayerInput input)
    {
        SetupArguments(input);

        if (_localizedText != null)
            _localizedText.RefreshString();
    }

    private void OnStringChanged(string transalatedAndFormattedText)
    {
        _tutorialText.text = transalatedAndFormattedText;
    }
    
    #endregion
}
