using System.Collections.Generic;
using UnityEngine;

public class LetterIcon_Controller : MonoBehaviour
{
  [SerializeField] private List<GameObject> _messageIcon;

    void OnValidate()
    {
        if(_messageIcon == null || _messageIcon.Count<=0 )
        {
            DevLog.LogError($"[{this.gameObject}]: Mancano i riferimenti alla Ui 3D del messaggio");
        }
    }

    void OnEnable()
    {
        int randomIndex = Random.Range(0,_messageIcon.Count);
        _messageIcon[randomIndex].SetActive(true);
    }

    void OnDisable()
    {
        foreach(var obj in _messageIcon)
        {
            obj.SetActive(false);
        }
    }
}
