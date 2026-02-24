using System;
using UnityEngine;

public class ColliderChecker : MonoBehaviour
{
    #region Parameters

    public event Action OnThisTriggerEnter;
    #endregion

    #region Unity Methods

    void OnTriggerEnter(Collider other)
    {
        OnThisTriggerEnter?.Invoke();
        DevLog.Log("Trigger Entered: " + other.gameObject.name, this);
    }
    #endregion

}
