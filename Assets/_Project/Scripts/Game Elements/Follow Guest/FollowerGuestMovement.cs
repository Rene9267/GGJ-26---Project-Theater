using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FollowerGuestMovement : MonoBehaviour
{
    [Header("Targeting")]
    private Transform _target;
    [Tooltip("Posizione relativa al target (es. 0, 1, -2 per stare dietro e un po' sopra)")]
    public Vector3 LocalOffset = new Vector3(0, 1.2f, -1.8f);

    [Header("Molla (Spring Physics)")]
    public float Stiffness = 250f;
    public float Damping = 20f;

    [Header("Limiti e Guinzaglio (Leash)")]
    [Tooltip("Distanza massima oltre la quale la molla smette di allungarsi e trascina l'oggetto")]
    public float MaxDistanceLeash = 5f;
    [Tooltip("Distanza minima dal centro del player per il Bump")]
    public float MinDistanceBump = 0.8f;
    public float BounceForce = 12f;
    
    [Header("Speed Limit")]
    public float MaxSpeed = 20f;

    [Header("Rotazione")]
    public float RotationSpeed = 10f;
    public float CurrentSpeed => _rb != null ? _rb.linearVelocity.magnitude : 0f;
    
    private Rigidbody _rb;
    private bool _isBouncing = false;

    private float _defaultStiffness;
    private float _defaultLeash;
    private float _defaultMaxSpeed;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();

        _rb.useGravity = false;
        _rb.isKinematic = true;
        _rb.linearDamping = 0;  
        _rb.interpolation = RigidbodyInterpolation.Interpolate;

        _defaultStiffness = Stiffness;
        _defaultLeash = MaxDistanceLeash;
        MaxSpeed = Random.Range(20,100);
        _defaultMaxSpeed = MaxSpeed;
    }

    private void OnDisable()

    {
        _rb.useGravity = false;
        _rb.isKinematic = true;
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
    }

    private void FixedUpdate()
    {
        if (_target == null) return;

        Vector3 desiredWorldPos = _target.TransformPoint(LocalOffset);
        Vector3 currentPos = transform.position;

        float distFromDesired = Vector3.Distance(currentPos, desiredWorldPos);
        if (distFromDesired > MaxDistanceLeash)
        {
            Vector3 directionToTarget = (desiredWorldPos - currentPos).normalized;
            transform.position = desiredWorldPos - (directionToTarget * MaxDistanceLeash);
        }

        Vector3 displacement = desiredWorldPos - transform.position;
        Vector3 springForce = displacement * Stiffness;
        Vector3 dampingForce = _rb.linearVelocity * Damping;

        Vector3 totalForce = springForce - dampingForce;

        _rb.AddForce(totalForce);

        if (_rb.linearVelocity.magnitude > MaxSpeed)
        {
            _rb.linearVelocity = _rb.linearVelocity.normalized * MaxSpeed;
        }

        RotateTowardsTarget();

        HandlePlayerBump();
    }

    public void SetExitMode(bool isExiting)
    {
        if (isExiting)
        {
            Stiffness = 30f;           
            MaxDistanceLeash = 1000f;  
            MaxSpeed = 4f;            
        }
        else
        {
            Stiffness = _defaultStiffness;
            MaxDistanceLeash = _defaultLeash;
            MaxSpeed = _defaultMaxSpeed;
        }
    }

    private void RotateTowardsTarget()
    {
        Vector3 lookDir = (_target.position - transform.position).normalized;
        if (lookDir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(lookDir);
            _rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRot, Time.fixedDeltaTime * RotationSpeed));
        }
    }

    private void HandlePlayerBump()
    {
        float distToPlayer = Vector3.Distance(transform.position, _target.position);

        if (distToPlayer < MinDistanceBump && !_isBouncing)
        {
            Vector3 bounceDir = (transform.position - _target.position).normalized;

            _rb.AddForce(bounceDir * BounceForce, ForceMode.Impulse);

            _isBouncing = true;
            Invoke(nameof(ResetBounce), 0.2f); 
        }
    }

    public void SetUpTarget(Transform target)
    {
        _target = target;
        _rb.isKinematic = false;
    }


    private void ResetBounce() => _isBouncing = false;

    private void OnDrawGizmosSelected()
    {
        if (_target == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(_target.TransformPoint(LocalOffset), 0.3f);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_target.position, MinDistanceBump);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(_target.TransformPoint(LocalOffset), MaxDistanceLeash);
    }
}
