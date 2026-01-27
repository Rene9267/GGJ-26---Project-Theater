using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CrowdInteractionArea : MonoBehaviour
{
    [SerializeField] private Image _areaImage;

    public event Action OnPlayerEntered;
    public event Action OnPlayerExited;

    private void OnValidate()
    {
        if (_areaImage == null)
        {
            Debug.LogWarning("Area Image ref is missing");
        }
    }

    public void SetImageColor(Color color)
    {
        _areaImage.color = color;
    }

    public IEnumerator AreaImageRotate(float rotationSpeed, bool clockwise = true)
    {
        float direction = clockwise ? -1f : 1f;

        while (true)
        {
            transform.Rotate(0, direction * rotationSpeed * Time.deltaTime, 0, Space.Self);
            yield return null;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        OnPlayerEntered?.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        OnPlayerExited?.Invoke();
    }

}
