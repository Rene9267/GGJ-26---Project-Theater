using UnityEngine;

public class Candle_FlameEffect : MonoBehaviour
{
    public bool IsRandomBoolSpeed = false;
    public Vector2 PulseSpeedRange = new(2f, 5f);

    [SerializeField] private Renderer _renderer;
    private MaterialPropertyBlock _propBlock;
    private static readonly int PulseSpeedID = Shader.PropertyToID("_PulseSpeed");

    void Awake()
    {
        if(_renderer == null)
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
        if(IsRandomBoolSpeed)
        {
            SetRandomPulseSpeed();
        }
    }

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

}
