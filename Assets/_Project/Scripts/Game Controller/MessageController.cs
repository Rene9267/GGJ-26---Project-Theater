using System.Collections.Generic;
using UnityEngine;

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
    private List<CrowdLink> _crowdLinks = new();
    private List<Crowd> _availableCrowds = new();

    //=== Color References ===//
    private List<Color> _availableColors = new();

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

        _availableColors = _settings.CircleColor;
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

        _crowdLinks.Add(new CrowdLink
        {
            Sender = sender,
            Receiver = receiver,
            CircleColor = circleColor
        });
    }
}
