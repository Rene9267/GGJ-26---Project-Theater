using System.Collections.Generic;
using UnityEngine;
using Action = System.Action;
using Random = UnityEngine.Random;

struct CrowdLink
{
    public Crowd Sender;
    public Crowd Receiver;
    public Color CircleColor;
}

public class MessageController : MonoBehaviour
{
    #region Parameters
    [SerializeField] private MessageSettings _settings;

    //=== Crowd References ===//
    private Dictionary<Color, CrowdLink> _crowdLinks = new();
    private List<Crowd> _availableCrowds = new();

    //=== Color References ===//
    private List<Color> _availableColors = new();

    //Ascoltato da GameContrller
    public event Action OnTaskFailed;
    public event Action OnMessageDelivered;

    #endregion


    #region Unity Methods
    private void OnValidate()
    {
        if (_settings == null)
        {
            DevLog.LogError("Message Settings reference is missing in MessageController.");
        }
    }

    private void Awake()
    {
        if (_settings.CircleColor == null || _settings.CircleColor.Count <= 0)
        {

#if UNITY_EDITOR
            Debug.LogError("No colors available in MessageSettings. Please add colors to the CircleColor list.");
#endif
            return;
        }

        _availableColors = new List<Color>(_settings.CircleColor);
    }

    #endregion


    #region Class Methods
    public void GetCrowds(List<Crowd> crowds)
    {
        if (crowds != null && crowds.Count > 0)
            _availableCrowds = crowds;
    }

    [ContextMenu("Genera Link Manuale")]
    public void CreateCrowdLink()
    {
        if (_availableCrowds.Count < 2) return;

        int randomSenderIndex = Random.Range(0, _availableCrowds.Count);
        Crowd sender = _availableCrowds[randomSenderIndex];
        _availableCrowds.RemoveAt(randomSenderIndex);

        int randomReceiverIndex = Random.Range(0, _availableCrowds.Count);
        Crowd receiver = _availableCrowds[randomReceiverIndex];
        _availableCrowds.RemoveAt(randomReceiverIndex);

        int randomColorIndex = Random.Range(0, _availableColors.Count);
        Color circleColor = _availableColors[randomColorIndex];
        _availableColors.RemoveAt(randomColorIndex);

        sender.EnableSender(circleColor, receiver);

        receiver.EnableReceiver(circleColor);

        _crowdLinks.Add(circleColor, new CrowdLink
        {
            Sender = sender,
            Receiver = receiver,
            CircleColor = circleColor
        });

        receiver.OnInteractionComplete += HandleMessageTaskCompleted;
        sender.OnTaskFailed += HandleMessageTaskFailed;
    }

    private void HandleMessageTaskCompleted(Color color)
    {
        if (_crowdLinks.ContainsKey(color))
        {
            var link = _crowdLinks[color];
            if (link.Receiver != null)
                link.Receiver.OnInteractionComplete -= HandleMessageTaskCompleted;
            if (link.Sender != null)
                link.Sender.OnTaskFailed -= HandleMessageTaskFailed;

            link.Sender.ResetCrowd();
            link.Receiver.ResetCrowd();

            if (!_availableCrowds.Contains(link.Receiver)) _availableCrowds.Add(link.Receiver);
            if (!_availableCrowds.Contains(link.Sender)) _availableCrowds.Add(link.Sender);
            if (!_availableColors.Contains(link.CircleColor)) _availableColors.Add(link.CircleColor);
            _crowdLinks.Remove(color);

            OnMessageDelivered?.Invoke();
        }
    }

    public void HandleMessageTaskFailed(Color color)
    {
        HandleMessageTaskCompleted(color);

        OnTaskFailed?.Invoke();
    }

    public void StopAllMessages()
    {
        foreach (var link in _crowdLinks.Values)
        {
            if (link.Receiver != null)
            {
                link.Receiver.OnInteractionComplete -= HandleMessageTaskCompleted;
                link.Receiver.ResetCrowd(); 
            }

            if (link.Sender != null)
            {
                link.Sender.OnTaskFailed -= HandleMessageTaskFailed;
                link.Sender.ResetCrowd(); 
            }
        }
        _crowdLinks.Clear();
    }
    
    #endregion
}