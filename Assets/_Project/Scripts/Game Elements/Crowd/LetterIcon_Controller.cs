using System.Collections.Generic;
using UnityEngine;

public class LetterIcon_Controller : MonoBehaviour
{
    [SerializeField] private List<GameObject> _messageSenderIcon;
    [SerializeField] private List<GameObject> _messageReciverIcon;
    public GameObject MyIcon { get; private set; }

    public int iconIndex = -1;

    void OnValidate()
    {
        if (_messageSenderIcon == null || _messageSenderIcon.Count <= 0)
        {
            DevLog.LogError($"[{this.gameObject}]: Mancano i riferimenti alla Ui 3D del messaggio");
        }
    }

    void OnDisable()
    {
        foreach (var obj in _messageSenderIcon)
        {
            obj.SetActive(false);
        }
    }

    public void SetUpIcon(InteractType type)
    {
        if (type == InteractType.MessageSender)
        {
            iconIndex = Random.Range(0, _messageSenderIcon.Count);
            MyIcon = _messageSenderIcon[iconIndex];
            MyIcon.SetActive(true);
        }
    }

    public void SelectReciverIcon(int index)
    {
        MyIcon = _messageReciverIcon[index];
        MyIcon.SetActive(true);
    }

    public void ShowIcon(InteractType type)
    {
        HideAllIcons(); 

        if (type == InteractType.MessageSender)
        {
            int randomIndex = Random.Range(0, _messageSenderIcon.Count);
            MyIcon = _messageSenderIcon[randomIndex];
        }
        else if (type == InteractType.MessageReciver)
        {
            int randomIndex = Random.Range(0, _messageReciverIcon.Count);
            MyIcon = _messageReciverIcon[randomIndex];
        }

        if (MyIcon != null) MyIcon.SetActive(true);
    }

    public void HideAllIcons()
    {
        _messageSenderIcon.ForEach(x => x.SetActive(false));
        _messageReciverIcon.ForEach(x => x.SetActive(false));
        MyIcon = null;
    }


}
