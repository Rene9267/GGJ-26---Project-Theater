using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class Tutorial_GameController : MonoBehaviour
{
    #region Parameters
    [Header("Controller")]
    [SerializeField] private CutSceneController _cutSceneController;
    [SerializeField] private PlayerController _player;
    [SerializeField] private CrowdTutorial _crowdTutorial;
    [SerializeField] private GuestTutorial _guestTutorial;
    [SerializeField] private CandleTutorial _candleTutorial;
    [SerializeField] private TutorialColliderChecker _movementTutorial;

    [Header("References")]
    // [SerializeField] private Animator _animator;
    [SerializeField] private Animator _tutorialUIAnimator;
    [SerializeField] private AudioSource _smokeParticlesAudioSource;
    [SerializeField] private WayPointNavigator _fakePlayer;


    [Header("Cutscene")]
    [SerializeField] private PlayableDirector _introCutscene;
    [SerializeField] private PlayableDirector _tutorialSkipCutscene;
    [SerializeField] private PlayableDirector _playtutorialCutscene;
    [SerializeField] private PlayableDirector _skipTutorialCutscene;
    [SerializeField] private PlayableDirector _endTutorialCutscene;


    [Header("Particle Particle")]
    //Particle
    [SerializeField] private SmokeController _smokeController_movementTutorial;
    [SerializeField] private SmokeController _smokeController_1;
    [SerializeField] private SmokeController _smokeController_2;
    [SerializeField] private SmokeController _smokeController_GuestTutorial;
    [SerializeField] private SmokeController _smokeController_CandleTuorial;


    private static readonly int _ExitTutorial = Animator.StringToHash("ExitTutorial");
    private static readonly int _tutorialUIWASD = Animator.StringToHash("WASD");
    private static readonly int _tutorialUIMessage = Animator.StringToHash("Message");
    private static readonly int _tutorialUIGuests = Animator.StringToHash("Guest");
    private static readonly int _tutorialUICandle = Animator.StringToHash("Candle");
    private static readonly int _tutorialUIDisappear = Animator.StringToHash("CloseTip");
    private static readonly int _tutorialUIEnd = Animator.StringToHash("End");
    private readonly string _gamePlayScene = "Scene_Main";
    #endregion

    #region Unity Methods

    void OnValidate()
    {
        if (_tutorialUIAnimator == null)
        {
            DevLog.LogError("Tutorial UI Animator is not assigned in the inspector.", this);
        }
        if (_guestTutorial == null)
        {
            DevLog.LogError("Tutorial_GuestController is not assigned in the inspector.", this);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnLevelFinishedLoading;
    }

    #endregion

    #region Class Methods

    private void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        DevLog.Log($"La scena {scene.name} è completamente caricata!");

        AskTutorial().Forget();
    }

    private async UniTask CutsceneStartOpera()
    {
        await _cutSceneController.PlayCutscene(_introCutscene);
    }

    private async UniTask AskTutorial()
    {
        await UniTask.Delay(1000);
        await CutsceneStartOpera();
        await _cutSceneController.PlayCutscene(_tutorialSkipCutscene);
    }
    
    public async void StartTutorial()
    {
        _fakePlayer.gameObject.SetActive(true);
        UniTask fakePlayerNavigation = _fakePlayer.StartNavigation();
        UniTask cutscenePlay = _cutSceneController.PlayCutscene(_playtutorialCutscene);

        await UniTask.WhenAll(fakePlayerNavigation, cutscenePlay);

        _fakePlayer.gameObject.SetActive(false);
        _player.gameObject.SetActive(true);
        _player.CanMove = true;
        WasdTutorialStart().Forget();
    }


    public async void SkipTutorial()
    {
        await _cutSceneController.PlayCutscene(_skipTutorialCutscene);
        SceneManager.LoadScene(_gamePlayScene);
    }

    private async UniTask WasdTutorialStart()
    {
        _tutorialUIAnimator.SetTrigger(_tutorialUIWASD);
        await UniTask.Delay(1000);
        _smokeParticlesAudioSource.Play();
        await UniTask.Delay(200);
        _smokeController_movementTutorial.PlaySmokeEffect();

        _movementTutorial.gameObject.SetActive(true);

        _movementTutorial.OnPlayerReachArea += () =>
        {
            DevLog.Log("WASD Tutorial Completed", this);
            WasdTutorialComplete();
        };
    }

    private async void WasdTutorialComplete()
    {
        _tutorialUIAnimator.SetTrigger(_tutorialUIDisappear);
        _smokeParticlesAudioSource.Play();
        await UniTask.Delay(200);
        _smokeController_movementTutorial.PlaySmokeEffect();

        _movementTutorial.gameObject.SetActive(false);
        await UniTask.Delay(2000);
        _movementTutorial.OnPlayerReachArea -= WasdTutorialComplete;
        BringMessageTutorial().Forget();
    }

    private async UniTask BringMessageTutorial()
    {
        _tutorialUIAnimator.SetTrigger(_tutorialUIMessage);
        await UniTask.Delay(1000);
        _crowdTutorial.gameObject.SetActive(true);
        await _crowdTutorial.StartTutorial();

        _tutorialUIAnimator.SetTrigger(_tutorialUIDisappear);
        _smokeParticlesAudioSource.Play();
        await UniTask.Delay(200);
        _smokeController_1.PlaySmokeEffect();
        _smokeController_2.PlaySmokeEffect();

        _crowdTutorial.gameObject.SetActive(false);
        await UniTask.Delay(2000);

        LeadPeopleToLobbyTutorial().Forget();
    }

    private async UniTask LeadPeopleToLobbyTutorial()
    {
        _tutorialUIAnimator.SetTrigger(_tutorialUIGuests);
        await UniTask.Delay(1000);

        _smokeParticlesAudioSource.Play();
        await UniTask.Delay(200);
        _smokeController_GuestTutorial.PlaySmokeEffect();

        _guestTutorial.gameObject.SetActive(true);
        await _guestTutorial.StartGuestTutorial();

        _tutorialUIAnimator.SetTrigger(_tutorialUIDisappear);
        await UniTask.Delay(2000);

        CandleLightTutorial().Forget();
    }

    private async UniTask CandleLightTutorial()
    {
        _tutorialUIAnimator.SetTrigger(_tutorialUICandle);
        await UniTask.Delay(1000);

        _smokeParticlesAudioSource.Play();
        await UniTask.Delay(200);
        _smokeController_CandleTuorial.PlaySmokeEffect();

        _candleTutorial.StartTutorial();

        _candleTutorial.OnCandleTutorialComplete += () =>
        {
            DevLog.Log("Candle Tutorial Complete", this);
            CandleLightTutorialComplete();
        };
    }

    private async void CandleLightTutorialComplete()
    {
        _tutorialUIAnimator.SetTrigger(_tutorialUIDisappear);
        _smokeParticlesAudioSource.Play();
        await UniTask.Delay(200);

        _smokeController_CandleTuorial.PlaySmokeEffect();

        _candleTutorial.OnCandleTutorialComplete -= CandleLightTutorialComplete;
        await UniTask.Delay(2000);
        _tutorialUIAnimator.SetTrigger(_tutorialUIEnd);
        await UniTask.Delay(6000);
        await CutsceneMiscellaneous.PlayCutscene(_endTutorialCutscene, this.gameObject);
        await UniTask.Delay(2000);
        SceneManager.LoadScene(_gamePlayScene);
    }
    #endregion
}