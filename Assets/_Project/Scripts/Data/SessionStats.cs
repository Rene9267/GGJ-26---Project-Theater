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

    public int NetPeople => InitialCrowdCount + PeopleGained - PeopleLost - FinalCrowdCount;

    public float SatisfactionScore
    {
        get
        {
            float maxDarkTime = Mathf.Max(TotalDarkCandleTime, 60f);
            float darkScore = Mathf.Clamp01(TotalDarkCandleTime / maxDarkTime);

            float maxFailedMessages = Mathf.Max(FailedMessagesCount, 8);
            float msgScore = Mathf.Clamp01(FailedMessagesCount / maxFailedMessages);

            float maxCollisions = Mathf.Max(CollisionCount, 15);
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
}
