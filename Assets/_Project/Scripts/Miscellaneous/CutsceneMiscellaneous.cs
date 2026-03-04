using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;

public static class CutsceneMiscellaneous 
{
    public static async UniTask PlayCutscene(PlayableDirector director, GameObject context)
    {
        DevLog.Log("Starting cutscene: " + director.name, context);

        director.Play();

        await UniTask.WaitUntil(() => director.state != PlayState.Playing || director.time >= director.duration,
        cancellationToken: context.GetCancellationTokenOnDestroy());

        ReleaseElement(director);

        DevLog.Log("Cutscene finished: " + director.name, context);
    }

    private static void ReleaseElement(PlayableDirector director)
    {
        director.RebindPlayableGraphOutputs();
        director.playableAsset = null;
        director.enabled = false;
    }
}
