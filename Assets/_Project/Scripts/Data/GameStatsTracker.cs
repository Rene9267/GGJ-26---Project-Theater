using UnityEngine;

public class GameStatsTracker : MonoBehaviour
{
    public int GuestsSeated { get; private set; }
    public int GuestsLeft { get; private set; }
    public int MessagesDelivered { get; private set; }
    public int MessagesFailed { get; private set; }
    public int PlayerStuns { get; private set; }
    public int DarknessEvents { get; private set; }
    public int TotalMessages { get; private set; }

    public void AddGuestsSeated(int count) => GuestsSeated += count;
    public void AddGuestsLeft(int count) => GuestsLeft += count;
    public void AddMessageDelivered() { MessagesDelivered++; TotalMessages++; }
    public void AddMessageFailed() { MessagesFailed++; TotalMessages++; }
    public void AddPlayerStun() => PlayerStuns++;
    public void AddDarknessEvent() => DarknessEvents++;

    public float CalculateSatisfaction(int remainingGuests, int initialGuests)
    {
        float guestScore = Mathf.Clamp01((float)remainingGuests / Mathf.Max(1, initialGuests));

        float msgRatio = TotalMessages > 0 ? (float)MessagesDelivered / TotalMessages : 1f;

        float stunPenalty = Mathf.Clamp01(1f - PlayerStuns * 0.05f);
        float darkPenalty = Mathf.Clamp01(1f - DarknessEvents * 0.1f);
        float leftPenalty = Mathf.Clamp01(1f - (float)GuestsLeft / Mathf.Max(1, GuestsSeated + GuestsLeft));

        return Mathf.Clamp01((guestScore * 0.4f + msgRatio * 0.3f + stunPenalty * 0.1f + darkPenalty * 0.1f + leftPenalty * 0.1f));
    }

    public int CalculateScore(float satisfaction)
    {
        if (satisfaction <= 0f) return 1;
        return Mathf.Clamp(Mathf.RoundToInt(satisfaction * 10), 1, 10);
    }

    public void Reset()
    {
        GuestsSeated = 0;
        GuestsLeft = 0;
        MessagesDelivered = 0;
        MessagesFailed = 0;
        PlayerStuns = 0;
        DarknessEvents = 0;
        TotalMessages = 0;
    }
}
