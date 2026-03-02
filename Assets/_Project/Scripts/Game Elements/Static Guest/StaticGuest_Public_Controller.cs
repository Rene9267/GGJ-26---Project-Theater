using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticGuest_Public_Controller : MonoBehaviour
{
    #region Parameters
    public bool SetRandomMaterialOnAwake = true;

    [SerializeField] protected StaticGuestSettings _settings;
    [SerializeField] protected Animator _animator;
    [SerializeField] protected List<GameObject> _bodyPartToChangeColor;
    [SerializeField] protected int _idleAnimationIndex = 0;

    protected List<Renderer> _renderers = new();
    protected MaterialPropertyBlock _sharedPropBlock;
    protected static readonly int _indexNextIdle = Animator.StringToHash("Static_Index");
    protected static readonly int _customIdle = Animator.StringToHash("NewIdle");

    #endregion

    #region Unity Methods
    void OnValidate()
    {
        if (SetRandomMaterialOnAwake)
        {
            if (_settings == null)
            {
                DevLog.LogError($"[{this.gameObject}]: Mancno i setting");
            }
        }
    }

    void Awake()
    {
        if (SetRandomMaterialOnAwake)
        {
            int randomColorIndex = Random.Range(0, _settings.BodyColor.Count);
            Color chosenColor = _settings.BodyColor[randomColorIndex];
            SetMaterialColor(chosenColor);
        }
    }

    void OnEnable()
    {
        StartCoroutine(RandomIdleRoutine());
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    #endregion

    public void SetMaterialColor(Color chosenColor)
    {
        _sharedPropBlock = new MaterialPropertyBlock();

        foreach (var obj in _bodyPartToChangeColor)
        {
            if (obj.TryGetComponent<Renderer>(out var renderer))
            {
                _renderers.Add(renderer);

                renderer.GetPropertyBlock(_sharedPropBlock);

                _sharedPropBlock.SetColor("_Color", chosenColor);

                renderer.SetPropertyBlock(_sharedPropBlock);
            }
        }
    }

    protected IEnumerator RandomIdleRoutine()
    {
        while (true)
        {
            float waitTime = Random.Range(3f, 15f);
            yield return new WaitForSeconds(waitTime);

            int randomIndex = Random.Range(0, _idleAnimationIndex);
            _animator.SetInteger(_indexNextIdle, randomIndex);
            _animator.SetTrigger(_customIdle);
        }
    }
}
