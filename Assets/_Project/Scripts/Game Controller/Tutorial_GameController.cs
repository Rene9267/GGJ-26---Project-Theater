using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Tutorial_GameController : MonoBehaviour
{
    #region Parameters
    [SerializeField] private Animator _animator;
    [SerializeField] private Animator _tutorialUIAnimator;
    [SerializeField] private TutorialColliderChecker _movementTutorial;
    [SerializeField] private PlayerController _player;
    [SerializeField] private CrowdTutorial _crowdTutorial;
    [SerializeField] private GuestTutorial _guestTutorial;
    [SerializeField] private CandleTutorial _candleTutorial;
    [SerializeField] private AudioSource _smokeParticlesAudioSource;
    [SerializeField] private AudioClip _theaterEnterClip;


    //Particle
    [SerializeField] private SmokeController _smokeController_movementTutorial;
    [SerializeField] private SmokeController _smokeController_1;
    [SerializeField] private SmokeController _smokeController_2;
    [SerializeField] private SmokeController _smokeController_GuestTutorial;
    [SerializeField] private SmokeController _smokeController_CandleTuorial;


    private static readonly int _showTutorial = Animator.StringToHash("StartCameraMotion");
    private static readonly int _ExitTutorial = Animator.StringToHash("ExitTutorial");
    private static readonly int _tutorialUIStartAppear = Animator.StringToHash("StartTutorial");
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
        if (_animator == null)
        {
            DevLog.LogError("Animator is not assigned in the inspector.", this);
        }
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

        StartTutorial().Forget();
    }

    private async UniTask CutsceneStartOpera()
    {
        _smokeParticlesAudioSource.PlayOneShot(_theaterEnterClip);
        _animator.SetTrigger(_showTutorial);
        await UniTask.Delay(4000);
    }

    private async UniTask StartTutorial()
    {
        await UniTask.Delay(1000);
        await CutsceneStartOpera();
        await UniTask.Delay(3000);
        _player.gameObject.SetActive(true);
        _player.CanMove = true;
        WasdTutorialStart().Forget();
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

        _candleTutorial.OnCandleTutorialComplete+= () =>
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
        await UniTask.Delay(3000);
        _tutorialUIAnimator.SetTrigger(_tutorialUIEnd);
        await UniTask.Delay(2000);
        _animator.SetTrigger(_ExitTutorial);
        await UniTask.Delay(2000);
        SceneManager.LoadScene(_gamePlayScene);
    }
    #endregion
}