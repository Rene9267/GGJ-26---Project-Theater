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

    [Header("Pause Settings")]
    [SerializeField] private GameObject _tutorialPauseMenuRoot;
    [SerializeField] private Animator _tutorialPauseAnimator;
    [SerializeField] private PauseController _pauseController;

    [Header("References")]
    [SerializeField] private Animator _tutorialUIAnimator;
    [SerializeField] private AudioSource _smokeParticlesAudioSource;
    [SerializeField] private WayPointNavigator _fakePlayer;

    [Header("Cutscene")]
    [SerializeField] private PlayableDirector _introCutscene;
    [SerializeField] private PlayableDirector _tutorialSkipCutscene;
    [SerializeField] private PlayableDirector _playtutorialCutscene;
    [SerializeField] private PlayableDirector _skipTutorialCutscene;
    [SerializeField] private PlayableDirector _endTutorialCutscene;

    [Header("Particles")]
    [SerializeField] private SmokeController _smokeController_movementTutorial;
    [SerializeField] private SmokeController _smokeController_1;
    [SerializeField] private SmokeController _smokeController_2;
    [SerializeField] private SmokeController _smokeController_GuestTutorial;
    [SerializeField] private SmokeController _smokeController_CandleTuorial;


    private static readonly int IsPauseTrigger = Animator.StringToHash("IsPause");
    private static readonly int IsEndPauseTrigger = Animator.StringToHash("IsEndPause");

    private readonly string _gamePlayScene = "Scene_Main";
    #endregion

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
    }

    private void Start()
    {
        // Spostiamo qui l'iscrizione agli eventi! 
        // In Start() siamo sicuri che il PauseController abbia già completato il suo Awake().
        if (PauseController.Instance != null)
        {
            PauseController.Instance.OnPauseToggled += HandleTutorialPause;
            PauseController.Instance.OnResumeRequested += StartExitAnimation;
        }
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnLevelFinishedLoading;
        if (PauseController.Instance != null)
        {
            PauseController.Instance.OnPauseToggled -= HandleTutorialPause;
            PauseController.Instance.OnResumeRequested -= StartExitAnimation;
        }
    }

    private void HandleTutorialPause(bool isPaused)
    {
        DevLog.Log($"[Tutorial_GameController] Pausa toggled: {(isPaused ? "PAUSED" : "RESUMED")}");
        if (_tutorialPauseAnimator == null || _tutorialPauseMenuRoot == null) return;

        DevLog.Log($"[Tutorial_GameController] Gestendo la pausa. Stato attuale: {(isPaused ? "PAUSED" : "RESUMED")}");
        if (isPaused)
        {
            _tutorialPauseMenuRoot.SetActive(true);

            // Un reset cautelativo male non fa per prevenire trigger "incastrati"
            _tutorialPauseAnimator.ResetTrigger(IsEndPauseTrigger);
            _tutorialPauseAnimator.SetTrigger(IsPauseTrigger);
        }
        else
        {
            // Rimosso il Trigger di uscita da qui.
            // L'uscita è correttamente gestita da StartExitAnimation(), così evitiamo sovrapposizioni.
        }
    }

    private void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        AskTutorial().Forget();
    }

    private async UniTask AskTutorial()
    {
        await UniTask.Delay(1000);
        await _cutSceneController.PlayCutscene(_introCutscene);
        await _cutSceneController.PlayCutscene(_tutorialSkipCutscene);
    }

    public async void StartTutorial()
    {
        _fakePlayer.gameObject.SetActive(true);
        await UniTask.WhenAll(_fakePlayer.StartNavigation(), _cutSceneController.PlayCutscene(_playtutorialCutscene));
        _fakePlayer.gameObject.SetActive(false);
        _player.gameObject.SetActive(true);
        _player.CanMove = true;
        WasdTutorialStart().Forget();
    }

    public void OnPauseOutAnimationComplete()
    {
        // Diciamo al sistema che può effettivamente far ripartire il tempo
        if (PauseController.Instance != null)
        {
            PauseController.Instance.FinalizeResume();
        }

        if (_tutorialPauseMenuRoot != null)
        {
            _tutorialPauseMenuRoot.SetActive(false);
        }
    }

    private void StartExitAnimation()
    {
        if (_tutorialPauseAnimator != null && _tutorialPauseAnimator.isActiveAndEnabled)
        {
            Debug.Log("[Tutorial] Avvio Animazione IsEndPause");
            _tutorialPauseAnimator.ResetTrigger(IsPauseTrigger);
            _tutorialPauseAnimator.SetTrigger(IsEndPauseTrigger);
        }
        else
        {
            // FALLBACK: Se non c'è l'animator, sblocca subito il gioco
            Debug.Log("[Tutorial] Animator non disponibile, sblocco immediato.");
            OnPauseOutAnimationComplete();
        }
    }

    private void HandlePauseVisuals(bool isPaused)
    {
        if (!isPaused) return;

        if (_tutorialPauseMenuRoot != null)
        {
            _tutorialPauseMenuRoot.SetActive(true);

            if (_tutorialPauseAnimator != null)
            {
                Debug.Log("[Tutorial] Avvio Animazione IsPause");
                _tutorialPauseAnimator.SetTrigger(IsPauseTrigger);
            }
            else
            {
                Debug.LogWarning("[Tutorial] Animator Pausa mancante! La UI apparirà ma senza animazione.");
            }
        }
    }

    /// <summary>
    /// Trascina questo metodo nell'evento OnClick() del pulsante "Riprendi".
    /// </summary>
    public void Btn_ResumeTutorial()
    {
        if (PauseController.Instance != null && PauseController.Instance.IsPaused)
        {
            PauseController.Instance.RequestResume();
        }
    }

    /// <summary>
    /// Esempio per un pulsante "Esci al Menu" o "Salta Tutorial" nel menu di pausa.
    /// </summary>
    public void Btn_SkipToMainScene()
    {
        if (PauseController.Instance != null)
        {
            PauseController.Instance.FinalizeResume();
        }
        SceneManager.LoadScene("Scene_Menu");
    }

    public void SkipTutorial() => SceneManager.LoadScene(_gamePlayScene);

    private async UniTask WasdTutorialStart()
    {
        _tutorialUIAnimator.SetTrigger("WASD");
        await UniTask.Delay(1000);
        _smokeParticlesAudioSource.Play();
        _smokeController_movementTutorial.PlaySmokeEffect();
        _movementTutorial.gameObject.SetActive(true);
        _movementTutorial.OnPlayerReachArea += WasdTutorialComplete;
    }

    private async void WasdTutorialComplete()
    {
        _movementTutorial.OnPlayerReachArea -= WasdTutorialComplete;
        _tutorialUIAnimator.SetTrigger("CloseTip");
        _smokeParticlesAudioSource.Play();
        _smokeController_movementTutorial.PlaySmokeEffect();
        _movementTutorial.gameObject.SetActive(false);
        await UniTask.Delay(2000);
        BringMessageTutorial().Forget();
    }

    private async UniTask BringMessageTutorial()
    {
        _tutorialUIAnimator.SetTrigger("Message");
        await UniTask.Delay(1000);
        _crowdTutorial.gameObject.SetActive(true);
        await _crowdTutorial.StartTutorial();
        _tutorialUIAnimator.SetTrigger("CloseTip");
        _smokeParticlesAudioSource.Play();
        _smokeController_1.PlaySmokeEffect();
        _smokeController_2.PlaySmokeEffect();
        _crowdTutorial.gameObject.SetActive(false);
        await UniTask.Delay(2000);
        LeadPeopleToLobbyTutorial().Forget();
    }

    private async UniTask LeadPeopleToLobbyTutorial()
    {
        _tutorialUIAnimator.SetTrigger("Guest");
        await UniTask.Delay(1000);
        _smokeParticlesAudioSource.Play();
        _smokeController_GuestTutorial.PlaySmokeEffect();
        _guestTutorial.gameObject.SetActive(true);
        await _guestTutorial.StartGuestTutorial();
        _tutorialUIAnimator.SetTrigger("CloseTip");
        await UniTask.Delay(2000);
        CandleLightTutorial().Forget();
    }

    private async UniTask CandleLightTutorial()
    {
        _tutorialUIAnimator.SetTrigger("Candle");
        await UniTask.Delay(1000);
        _smokeParticlesAudioSource.Play();
        _smokeController_CandleTuorial.PlaySmokeEffect();
        _candleTutorial.StartTutorial();
        _candleTutorial.OnCandleTutorialComplete += CandleLightTutorialComplete;
    }

    private async void CandleLightTutorialComplete()
    {
        _candleTutorial.OnCandleTutorialComplete -= CandleLightTutorialComplete;
        _tutorialUIAnimator.SetTrigger("CloseTip");
        _smokeParticlesAudioSource.Play();
        _smokeController_CandleTuorial.PlaySmokeEffect();
        await UniTask.Delay(2000);
        _tutorialUIAnimator.SetTrigger("End");
        await UniTask.Delay(6000);
        await CutsceneMiscellaneous.PlayCutscene(_endTutorialCutscene, this.gameObject);
        SceneManager.LoadScene(_gamePlayScene);
    }
}