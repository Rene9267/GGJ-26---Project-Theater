using UnityEngine;

public class Candle_FlameEffect : MonoBehaviour
{
    #region Variables
    public bool IsRandomBoolSpeed = false;
    public Vector2 PulseSpeedRange = new(2f, 5f);

    [SerializeField] private Renderer _renderer;
    [SerializeField] private GameObject _flameModel;

    private MaterialPropertyBlock _propBlock;
    private static readonly int PulseSpeedID = Shader.PropertyToID("_PulseSpeed");
    #endregion


    #region Unity Standard Methods

    void OnValidate()
    {
        if(_renderer == null)
            DevLog.LogError("Candle_FlameEffect: Riferimento al renderer assente. Aggiungilo per poter modificare la velocità di pulsazione.", this);
        if (_flameModel == null)
            DevLog.LogError("Candle_FlameEffect: Riferimento al modello della fiamma assente. Aggiungilo per poter disattivare la fiamma.", this);
    }

    void Awake()
    {
        if (_renderer == null)
        {
            if (TryGetComponent(out _renderer) == false)
            {
                DevLog.LogWarning("Candle_FlameEffect: No Renderer component found on the GameObject. I'll start searching in childers.");
                _renderer = GetComponentInChildren<Renderer>();
                if (_renderer == null)
                {
                    DevLog.LogError("Candle_FlameEffect: No Renderer component found in the GameObject or its children. Please add a Renderer component to use the flame effect.");
                    return;
                }
            }
        }

        _propBlock = new MaterialPropertyBlock();

    }

    void Start()
    {
        if (IsRandomBoolSpeed)
        {
            SetRandomPulseSpeed();
        }
    }

    #endregion


    #region Class Methods

    public void SetPulseSpeed(float speed)
    {
        _renderer.GetPropertyBlock(_propBlock);
        _propBlock.SetFloat(PulseSpeedID, speed);
        _renderer.SetPropertyBlock(_propBlock);
    }

    private void SetRandomPulseSpeed()
    {
        float randomSpeed = Random.Range(PulseSpeedRange.x, PulseSpeedRange.y);
        SetPulseSpeed(randomSpeed);
    }

#if UNITY_EDITOR
    [ContextMenu("Shut Down Light")]
    public void ShutDownLight()
    {
        if (_renderer == null)
        {
            DevLog.LogError("Candle_FlameEffect: No Renderer component found. Cannot shut down light.");
            return;
        }
        if (_flameModel == null)
        {
            DevLog.LogError("Candle_FlameEffect: No flame model assigned. Cannot shut down light.");
            return;
        }
        
        _flameModel.SetActive(false);
        _renderer.gameObject.SetActive(false);
    }
#endif

    #endregion
}
