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
    [SerializeField] private MessageSettings _settings;

    //=== Crowd References ===//
    private Dictionary<Color, CrowdLink> _crowdLinks = new();
    private List<Crowd> _availableCrowds = new();

    //=== Color References ===//
    private List<Color> _availableColors = new();

    //Ascoltato da GameContrller
    public event Action OnTaskFailed;

    private void OnValidate()
    {
        if (_settings == null)
        {
            Debug.LogError("Message Settings reference is missing in MessageController.");
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

        sender.EnableSender(circleColor);
        receiver.EnableReciver(circleColor);

        _crowdLinks.Add(circleColor, new CrowdLink
        {
            Sender = sender,
            Receiver = receiver,
            CircleColor = circleColor
        });

        receiver.OnInteracionComplete += HandleMessageTaskCompleted;
        sender.OnTaskFailed += HandleMessageTaskFailed;
    }

    private void HandleMessageTaskCompleted(Color color)
    {
        if (_crowdLinks.ContainsKey(color))
        {
            var tmpSender = _crowdLinks[color].Sender;
            tmpSender.ResetCrowd();


            var tmpReciver = _crowdLinks[color].Receiver;
            tmpReciver.ResetCrowd();

            tmpReciver.OnInteracionComplete -= HandleMessageTaskCompleted;
            tmpSender.OnTaskFailed -= HandleMessageTaskFailed;

            var tmpColor = _crowdLinks[color].CircleColor;

            if (_availableCrowds.Contains(tmpReciver) == false)
                _availableCrowds.Add(tmpReciver);
            if (_availableCrowds.Contains(tmpSender) == false)
                _availableCrowds.Add(tmpSender);
            if (_availableColors.Contains(tmpColor) == false)
                _availableColors.Add(tmpColor);

            _crowdLinks.Remove(color);
        }
    }

    public void HandleMessageTaskFailed(Color color)
    {
        HandleMessageTaskCompleted(color);

        OnTaskFailed?.Invoke();
    }

}
