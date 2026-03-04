using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;

public class CutSceneController : MonoBehaviour
{
    [SerializeField]private PlayableDirector _introPlayableDirector;
    [SerializeField] private PlayableDirector _endPlayableDirector;

    public async UniTask IntroCutscene()
    {
        await PlayCutscene(_introPlayableDirector);
    }

    public async UniTask EndCutscene()
    {
        await PlayCutscene(_endPlayableDirector);
    }

    public async UniTask PlayCutscene(PlayableDirector director)
    {
        DevLog.Log("Starting cutscene: " + director.name, this);

        director.Play();

        await UniTask.WaitUntil(() => director.state != PlayState.Playing || director.time >= director.duration,
        cancellationToken: this.GetCancellationTokenOnDestroy());

        director.RebindPlayableGraphOutputs();
        director.playableAsset = null;
        director.enabled = false;
        
        DevLog.Log("Cutscene finished: " + director.name, this);
    }

}
