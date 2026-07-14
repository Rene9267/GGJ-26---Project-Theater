using UnityEngine;

public class StaticGuest_Controller : StaticGuest_Public_Controller
{
    #region Parameters
    [SerializeField] private Animation _animtion;
    [SerializeField] private GameObject _hurryUpIcon;

    private readonly string _hurryUp = "AC_HurryUP";

    #endregion

    #region Unity Methods

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
