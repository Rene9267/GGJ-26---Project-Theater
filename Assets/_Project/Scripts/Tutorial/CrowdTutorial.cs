using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class CrowdTutorial : MonoBehaviour
{
    #region Parameters
    [Header("Tutorial Settings")]
    [SerializeField] private List<SimpleCrowd> _crowds; 
    [SerializeField] private Color _circleColor;
   

    private bool _messageDelivered = false;
    #endregion

    #region Class Methods
    public async UniTask StartTutorial()
    {
        foreach (var crowd in _crowds)
        {
            crowd.gameObject.SetActive(true);
            crowd.SpawnGuests();
        }

        await UniTask.Delay(1000);

        SimpleCrowd sender = _crowds[0];
        SimpleCrowd receiver = _crowds[1];

        _messageDelivered = false;
        receiver.OnInteractionComplete += OnComplete;

        sender.EnableSender(_circleColor, receiver);
        receiver.EnableReceiver(_circleColor);

        await UniTask.WaitUntil(() => _messageDelivered);

        receiver.OnInteractionComplete -= OnComplete;

        DevLog.Log("Tutorial dei Messaggi Superato!");
    }

    private void OnComplete()
    {
        _messageDelivered = true;
    }

    [ContextMenu("Genera Link Manuale (Debug)")]
    public void CreateCrowdLink()
    {
        if (_crowds.Count < 2) return;
        _crowds[0].EnableSender(_circleColor, _crowds[1]);
        _crowds[1].EnableReceiver(_circleColor);
    }

    #endregion
}