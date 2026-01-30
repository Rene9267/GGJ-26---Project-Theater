using System.Collections.Generic;
using UnityEngine;

public class StaticGuest_Controller : MonoBehaviour
{
    [SerializeField] private StaticGuestSettings _settings;
    [SerializeField] private Animation _animtion;
    [SerializeField] private GameObject _hurryUpIcon;
    [SerializeField] private List<GameObject> _bodyPartToChangeColor;

    public bool SetRandomMaterialOnAwake = true;

    private List<Renderer> _renderers = new();
    private MaterialPropertyBlock _sharedPropBlock;
    private readonly string _hurryUp = "AC_HurryUP";

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

    public void HurryUp()
    {
        _animtion.Play(_hurryUp);
    }

    public void StopHurry()
    {
        _animtion.Stop();
        _hurryUpIcon.SetActive(false);
    }
}
