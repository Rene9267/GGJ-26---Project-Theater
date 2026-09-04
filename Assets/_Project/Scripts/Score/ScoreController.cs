using System.Collections.Generic;
using UnityEngine;

public class ScoreController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private OperaSettings _settings;

    [Header("Normalization Caps")]
    [SerializeField] private float _maxExpectedDarkTime = 60f;
    [SerializeField] private int _maxExpectedFailedMessages = 8;
    [SerializeField] private int _maxExpectedCollisions = 15;

    private GuestController _guestController;
    private MessageController _messageController;
    private CandleController _candleController;
    private PlayerController _playerController;

    private int _peopleGained;
    private int _peopleLost;
    private int _failedMessageCount;
    private float _totalDarkCandleTime;
    private Dictionary<Candle, float> _candleDarkStartTimes = new();
    private int _initialCrowdCount;

    public void Connect(GuestController guest, MessageController message, CandleController candle, PlayerController player)
    {
        _guestController = guest;
        _messageController = message;
        _candleController = candle;
        _playerController = player;

        _guestController.OnGuestDropped += OnGuestDropped;
        _guestController.OnTaskFailed += OnGuestTaskFailed;
        _messageController.OnTaskFailed += OnMessageTaskFailed;
        _candleController.OnDarkRise += OnDarkRise;
        _candleController.OnCandleDarkStart += OnCandleDarkStart;
        _candleController.OnCandleTurnedOn += OnCandleTurnedOn;
    }

    public void Disconnect()
    {
        if (_guestController != null)
        {
            _guestController.OnGuestDropped -= OnGuestDropped;
            _guestController.OnTaskFailed -= OnGuestTaskFailed;
        }
        if (_messageController != null)
            _messageController.OnTaskFailed -= OnMessageTaskFailed;
        if (_candleController != null)
        {
            _candleController.OnDarkRise -= OnDarkRise;
            _candleController.OnCandleDarkStart -= OnCandleDarkStart;
            _candleController.OnCandleTurnedOn -= OnCandleTurnedOn;
        }
    }

    public void ResetStats(int initialCrowdCount)
    {
        _peopleGained = 0;
        _peopleLost = 0;
        _failedMessageCount = 0;
        _totalDarkCandleTime = 0f;
        _candleDarkStartTimes.Clear();
        _initialCrowdCount = initialCrowdCount;
    }

    public SessionStats GetFinalStats()
    {
        foreach (var kvp in _candleDarkStartTimes)
            _totalDarkCandleTime += Time.time - kvp.Value;
        _candleDarkStartTimes.Clear();

        var stats = new SessionStats
        {
            PeopleGained = _peopleGained,
            PeopleLost = _peopleLost,
            TotalGuestsSpawned = _guestController != null ? _guestController.TotalGuestsSpawned : 0,
            TotalDarkCandleTime = _totalDarkCandleTime,
            FailedMessagesCount = _failedMessageCount,
            CollisionCount = _playerController != null ? _playerController.CollisionCount : 0
        };

        stats.MaxExpectedDarkTime = _maxExpectedDarkTime;
        stats.MaxExpectedFailedMessages = _maxExpectedFailedMessages;
        stats.MaxExpectedCollisions = _maxExpectedCollisions;

        return stats;
    }

    private void OnGuestDropped(int count)
    {
        if (count > 0) _peopleGained += count;
    }

    private void OnGuestTaskFailed(int lostCount)
    {
        _peopleLost += lostCount;
    }

    private void OnMessageTaskFailed()
    {
        _failedMessageCount++;
        if (_settings != null) _peopleLost += Mathf.Abs(_settings.MessageFailTask);
    }

    private void OnDarkRise()
    {
        if (_settings != null) _peopleLost += Mathf.Abs(_settings.DarkIsRising);
    }

    private void OnCandleDarkStart(Candle candle)
    {
        _candleDarkStartTimes[candle] = Time.time;
    }

    private void OnCandleTurnedOn(Candle candle)
    {
        if (_candleDarkStartTimes.TryGetValue(candle, out float startTime))
        {
            _totalDarkCandleTime += Time.time - startTime;
            _candleDarkStartTimes.Remove(candle);
        }
    }
}
