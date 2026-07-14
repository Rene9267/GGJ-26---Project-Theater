using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct BodyPartColorTarget
{
    public GameObject BodyPart;
    [Tooltip("L'indice del materiale da colorare (0 = primo materiale, 1 = secondo, ecc.)")]
    public int MaterialIndex;
}

public class StaticGuest_Public_Controller : MonoBehaviour
{
    #region Parameters
    public bool SetRandomMaterialOnAwake = true;

    [SerializeField] protected StaticGuestSettings _settings;
    [SerializeField] protected Animator _animator;

    [SerializeField] protected List<BodyPartColorTarget> _bodyPartToChangeColor;

    [SerializeField] protected int _idleAnimationIndex = 0;

    protected List<Renderer> _renderers = new();
    protected MaterialPropertyBlock _sharedPropBlock;
    protected static readonly int _indexNextIdle = Animator.StringToHash("Static_Index");
    protected static readonly int _customIdle = Animator.StringToHash("NewIdle");
    protected static readonly int _happyEndingHash = Animator.StringToHash("HappyEnding");
    protected static readonly int _sadEndingHash = Animator.StringToHash("SadEnding");

    #endregion

    #region Unity Methods
    void OnValidate()
    {
        if (SetRandomMaterialOnAwake)
        {
            if (_settings == null)
            {
                DevLog.LogError($"[{this.gameObject}]: Mancano i setting");
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
        GameController.OnEndingAnimation += SetEndingAnimation;
        StartCoroutine(RandomIdleRoutine());
    }

    void OnDisable()
    {
        GameController.OnEndingAnimation -= SetEndingAnimation;
        StopAllCoroutines();
    }

    #endregion

    public void SetMaterialColor(Color chosenColor)
    {
        _sharedPropBlock = new MaterialPropertyBlock();

        foreach (var target in _bodyPartToChangeColor)
        {
            if (target.BodyPart != null && target.BodyPart.TryGetComponent<Renderer>(out var renderer))
            {
                if (!_renderers.Contains(renderer))
                {
                    _renderers.Add(renderer);
                }

                renderer.GetPropertyBlock(_sharedPropBlock, target.MaterialIndex);

                _sharedPropBlock.SetColor("_Color", chosenColor);

                renderer.SetPropertyBlock(_sharedPropBlock, target.MaterialIndex);
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

    public void SetEndingAnimation(bool isHappy)
    {
        if (isHappy)
            _animator.SetBool(_happyEndingHash, true);
        else
            _animator.SetBool(_sadEndingHash, true);
    }
}