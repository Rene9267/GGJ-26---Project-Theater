using UnityEngine;

public class LookAtDirection : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Trascina qui l'oggetto che vuoi guardare")]
    [SerializeField] private Transform _targetToLookAt;

    [Header("Settings")]
    [Tooltip("Quanto velocemente deve girarsi?")]
    [SerializeField] private float _rotationSpeed = 5f;

    [Tooltip("Se VERO, l'oggetto ruota solo a destra/sinistra ma non si inclina su/giù (utile per personaggi a terra).")]
    [SerializeField] private bool _lockYAxis = false;
    private Transform _currentTarget;

    void Start()
    {
        if (_targetToLookAt != null)
        {
            _currentTarget = _targetToLookAt;
        }
        else
        {
            if (Camera.main != null)
            {
                _currentTarget = Camera.main.transform;
            }
            else
            {
                Debug.LogWarning($"[LookAtObject] Attenzione: {name} non ha un target e non trova la Main Camera!");
            }
        }
    }

    void Update()
    {
        if (_currentTarget == null) return;

        Vector3 direction = _currentTarget.position - transform.position;

        if (_lockYAxis)
        {
            direction.y = 0;
        }

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * _rotationSpeed);
        }
    }

    public void SetNewTarget(Transform newTarget)
    {
        _currentTarget = newTarget;
    }
}