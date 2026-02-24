using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TutorialColliderChecker : MonoBehaviour
{
    #region Parameters
    public event Action OnPlayerReachArea;
    [SerializeField] private Image _rotationBase;
    [SerializeField] private ColliderChecker _colliderChecker;
    #endregion

    #region Unity Methods
    void OnEnable()
    {
        StartCoroutine(AreaImageRotate(20f));
        _colliderChecker.OnThisTriggerEnter += HandleTriggerEnter;
    }

    void OnDisable()
    {
        StopAllCoroutines();
        _colliderChecker.OnThisTriggerEnter -= HandleTriggerEnter;
    }

    #endregion

    #region Class Methods

    public IEnumerator AreaImageRotate(float rotationSpeed, bool clockwise = true)
    {
        float direction = clockwise ? -1f : 1f;

        while (true)
        {
            _rotationBase.transform.Rotate(0, 0, direction * rotationSpeed * Time.deltaTime, Space.Self);
            yield return null;
        }
    }

    private void HandleTriggerEnter()
    {
        OnPlayerReachArea?.Invoke();
    }

    #endregion
}
