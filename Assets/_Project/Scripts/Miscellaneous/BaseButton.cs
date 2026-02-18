using UnityEngine;
using UnityEngine.UI;

public class BaseButton : MonoBehaviour
{
    #region Variables
    [SerializeField] private AudioClip _hoverAudioClip;
    [SerializeField] private AudioClip _clickAudioClip;
    [SerializeField] private AudioSource _audioSource;

    private Button _myButton;
    #endregion

    #region Unity Standard Methods
    void Awake()
    {
        if (TryGetComponent(out _myButton) == false)
        {
            DevLog.LogError($"[{this}]: No Button Component on this element", this);
        }
    }

    void OnEnable()
    {
        if (_myButton)
        {
            _myButton.onClick.AddListener(PlayClickAudioClip);
        }
    }

    void OnDisable()
    {
        if (_myButton)
            _myButton.onClick.RemoveListener(PlayClickAudioClip);
    }

    #endregion

    #region Class Methods
    public void PlayHoverAudioClip()
    {
        if (_audioSource == null || _hoverAudioClip == null)
        {
            DevLog.LogWarning($"[{this}], Hover clip or audio Source is null", this);
            return;
        }

        _audioSource.PlayOneShot(_hoverAudioClip);
    }

    public void PlayClickAudioClip()
    {
        if (_audioSource == null || _hoverAudioClip == null)
        {
            DevLog.LogWarning($"[{this}], Hover clip or audio Source is null", this);
            return;
        }

        _audioSource.PlayOneShot(_clickAudioClip);
    }
    #endregion

}
