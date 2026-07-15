using UnityEngine;

public class SessionStats
{
    public int PeopleGained;
    public int PeopleLost;
    public float TotalDarkCandleTime;
    public int FailedMessagesCount;
    public int CollisionCount;
    public int InitialCrowdCount;
    public int FinalCrowdCount;
    public int TotalGuestsSpawned;

    public float MaxExpectedDarkTime = 60f;
    public int MaxExpectedFailedMessages = 8;
    public int MaxExpectedCollisions = 15;

    public int NetPeople => InitialCrowdCount + PeopleGained - PeopleLost - FinalCrowdCount;

    public float SatisfactionScore
    {
        get
        {
            float maxDarkTime = Mathf.Max(TotalDarkCandleTime, MaxExpectedDarkTime);
            float darkScore = Mathf.Clamp01(TotalDarkCandleTime / maxDarkTime);

            float maxFailedMessages = Mathf.Max(FailedMessagesCount, MaxExpectedFailedMessages);
            float msgScore = Mathf.Clamp01(FailedMessagesCount / maxFailedMessages);

            float maxCollisions = Mathf.Max(CollisionCount, MaxExpectedCollisions);
            float collisionScore = Mathf.Clamp01(CollisionCount / maxCollisions);

            float avgBadness = (darkScore + msgScore + collisionScore) / 3f;
            return 1f - avgBadness;
        }
    }

    public int FinalVote
    {
        get
        {
            int totalPeople = PeopleGained + PeopleLost;
            float peopleRatio = totalPeople > 0 ? (float)PeopleGained / totalPeople : 0.5f;

            float weightedScore = peopleRatio * 0.4f + SatisfactionScore * 0.6f;
            return Mathf.Clamp(Mathf.RoundToInt(weightedScore * 9f + 1f), 1, 10);
        }
    }

    public string GainedWordKey
    {
        get
        {
            float ratio = TotalGuestsSpawned > 0 ? (float)PeopleGained / TotalGuestsSpawned : 0f;
            if (ratio >= 0.66f) return ScoreWordKeys.Gained_Tier4;
            if (ratio >= 0.33f) return ScoreWordKeys.Gained_Tier3;
            if (ratio > 0f) return ScoreWordKeys.Gained_Tier2;
            return ScoreWordKeys.Gained_Tier1;
        }
    }

    public string LostWordKey
    {
        get
        {
            float ratio = TotalGuestsSpawned > 0 ? (float)PeopleLost / TotalGuestsSpawned : 0f;
            if (ratio <= 0f) return ScoreWordKeys.Lost_Tier4;
            if (ratio <= 0.33f) return ScoreWordKeys.Lost_Tier3;
            if (ratio <= 0.66f) return ScoreWordKeys.Lost_Tier2;
            return ScoreWordKeys.Lost_Tier1;
        }
    }

    public string GeneralSatisfactionWordKey
    {
        get
        {
            if (SatisfactionScore >= 0.8f) return ScoreWordKeys.Satisfaction_Tier4;
            if (SatisfactionScore >= 0.5f) return ScoreWordKeys.Satisfaction_Tier3;
            if (SatisfactionScore >= 0.25f) return ScoreWordKeys.Satisfaction_Tier2;
            return ScoreWordKeys.Satisfaction_Tier1;
        }
    }

    public string Grade
    {
        get
        {
            return FinalVote switch
            {
                10 => "S",
                9 => "A",
                8 => "B",
                7 => "C",
                6 => "D",
                5 => "E",
                _ => "F"
            };
        }
    }
}
