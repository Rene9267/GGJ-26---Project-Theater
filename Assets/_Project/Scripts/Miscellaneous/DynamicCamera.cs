using UnityEngine;

public class DynamicCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform player;
    public float smoothSpeed = 5f;

    [Header("Limiti di Movimento (X)")]
    public float minX = -20f;
    public float maxX = 20f; 

    [Header("Parametri Rotazione (Tilt)")]
    [Tooltip("Quanto la camera deve ruotare extra rispetto alla posizione iniziale")]
    public float maxExtraTilt = 15f;
    [Tooltip("La distanza Z dal centro oltre la quale la rotazione è massima")]
    public float tiltSensitivity = 10f;

    private Vector3 _initialOffset;
    private float _initialRotationX;
    private float _startPlayerZ;

    void Start()
    {
        if (player == null) return;

        _initialOffset = transform.position - player.position;
        _initialRotationX = transform.eulerAngles.x;
        _startPlayerZ = player.position.z;
    }

    void LateUpdate()
    {
        if (player == null) return;

        float desiredX = player.position.x + _initialOffset.x;

        float clampedX = Mathf.Clamp(desiredX, minX, maxX);

        Vector3 targetPos = new Vector3(clampedX, transform.position.y, transform.position.z);

        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * smoothSpeed);

        float zDelta = player.position.z - _startPlayerZ;
        float tiltEffect = (zDelta / tiltSensitivity) * maxExtraTilt;
        float finalRotationX = _initialRotationX + tiltEffect;

        transform.rotation = Quaternion.Euler(finalRotationX, transform.eulerAngles.y, transform.eulerAngles.z);
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
       
        Vector3 top = new Vector3(minX, transform.position.y, transform.position.z + 5);
        Vector3 bottom = new Vector3(minX, transform.position.y, transform.position.z - 5);
        Gizmos.DrawLine(new Vector3(minX, -10, transform.position.z), new Vector3(minX, 10, transform.position.z));
        Gizmos.DrawLine(new Vector3(maxX, -10, transform.position.z), new Vector3(maxX, 10, transform.position.z));
    }
}
