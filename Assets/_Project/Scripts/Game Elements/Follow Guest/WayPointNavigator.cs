using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class WayPointNavigator : MonoBehaviour
{
    #region Parameters
    [Header("Path Settings")]
    [Tooltip("La sequenza di punti che l'oggetto dovrà seguire")]
    public List<Transform> Waypoints;

    [Header("Movement Settings")]
    public float MoveSpeed = 5f;
    public float RotationSpeed = 10f;
    [Tooltip("Distanza minima per considerare raggiunto il waypoint")]
    public float ArrivalThreshold = 0.2f;

    [Header("Events")]
    [Tooltip("Evento richiamato quando l'oggetto arriva all'ultimo punto")]
    public UnityEvent OnPathCompleted;

    private Animator _animator;
    private int _animIDWalking;

    #endregion

    #region Unity Methods
    private void Awake()
    {
        TryGetComponent(out _animator);
        if(_animator == null)
        {
            DevLog.LogWarning($"[{gameObject.name}] Non è stato trovato un Animator, il WayPointNavigator funzionerà ma senza animazioni.");
            _animator = GetComponentInChildren<Animator>();
            if(_animator == null)
            {
                DevLog.LogWarning($"[{gameObject.name}] Non è stato trovato un Animator neanche nei figli, assicurati che ci sia un Animator per le animazioni di camminata.");
            }
        }

        _animIDWalking = Animator.StringToHash("IsWalking");
    }

    #endregion

    #region Class Methods
    public async UniTask StartNavigation()
    {
        if (Waypoints == null || Waypoints.Count == 0)
        {
            Debug.LogWarning($"[{gameObject.name}] Nessun waypoint assegnato!");
            return;
        }

        if (_animator != null) _animator.SetBool(_animIDWalking, true);

        foreach (var wp in Waypoints)
        {
            if (wp == null) continue;

            while (Vector3.Distance(transform.position, wp.position) > ArrivalThreshold)
            {
                transform.position = Vector3.MoveTowards(transform.position, wp.position, MoveSpeed * Time.deltaTime);

                Vector3 direction = (wp.position - transform.position).normalized;
                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, RotationSpeed * Time.deltaTime);
                }

                await UniTask.Yield(PlayerLoopTiming.Update);
            }
        }

        if (_animator != null) _animator.SetBool(_animIDWalking, false);
        OnPathCompleted?.Invoke();
    }

    #endregion
}
