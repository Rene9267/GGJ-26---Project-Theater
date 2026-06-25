using UnityEngine;

public class SessionStats
{
    public int PeopleGained;
    public int PeopleLost;
    public int TotalGuestsSpawned;
    public float TotalDarkCandleTime;
    public int FailedMessagesCount;
    public int CollisionCount;

    public float MaxExpectedDarkTime = 60f;
    public int MaxExpectedFailedMessages = 8;
    public int MaxExpectedCollisions = 15;

    public int GainedScore
    {
        get
        {
            int total = TotalGuestsSpawned;
            if (total <= 0) return 5;

            float ratio = (float)PeopleGained / total;
            return Mathf.Clamp(Mathf.RoundToInt(Mathf.Pow(ratio, 0.65f) * 9f + 1f), 1, 10);
        }
    }

    public int LostScore
    {
        get
        {
            int total = TotalGuestsSpawned;
            if (total <= 0) return 10;

            float ratio = (float)PeopleLost / total;
            float inverted = 1f - Mathf.Pow(ratio, 0.7f);
            return Mathf.Clamp(Mathf.RoundToInt(inverted * 9f + 1f), 1, 10);
        }
    }

    public float SatisfactionScore
    {
        get
        {
            float darkScore = Mathf.Min(TotalDarkCandleTime / MaxExpectedDarkTime, 1f);
            float msgScore = Mathf.Min((float)FailedMessagesCount / MaxExpectedFailedMessages, 1f);
            float collisionScore = Mathf.Min((float)CollisionCount / MaxExpectedCollisions, 1f);

            float avgBadness = (darkScore + msgScore + collisionScore) / 3f;
            return 1f - avgBadness;
        }
    }

    public string SatisfactionLabel
    {
        get
        {
            float raw = SatisfactionScore;
            if (raw >= 0.8f) return "Ottimo";
            if (raw >= 0.6f) return "Buono";
            if (raw >= 0.4f) return "Mediocre";
            if (raw >= 0.2f) return "Scarso";
            return "Pessimo";
        }
    }

    public int FinalVote
    {
        get
        {
            float gainedWeighted = (GainedScore - 1) / 9f;
            float lostWeighted = (LostScore - 1) / 9f;
            float weightedScore = gainedWeighted * 0.35f + lostWeighted * 0.35f + SatisfactionScore * 0.3f;
            return Mathf.Clamp(Mathf.RoundToInt(weightedScore * 9f + 1f), 1, 10);
        }
    }
}
