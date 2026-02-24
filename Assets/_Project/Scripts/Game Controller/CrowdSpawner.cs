using System.Collections.Generic;
using UnityEngine;

public class CrowdSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private List<Transform> _crowdSpawnPoint;
    [SerializeField] private GameObject _crowdObject;
    [SerializeField] private Transform _crowdFather;

    [Header("Settings")]
    [SerializeField] private float _radius;

    private List<Crowd> _crowds;


  
    private void OnValidate()
    {
        if (_crowdSpawnPoint == null || _crowdSpawnPoint.Count <= 0)
        {
            DevLog.LogWarning("Crowd spawn points are not assigned in CrowdSpawner.");
        }
    }

    public List<Crowd> InitializeCrowds()
    {
        _crowds = new List<Crowd>();
        int croudNumber = Random.Range(4, _crowdSpawnPoint.Count + 1);
        List<Transform> duplicateSpawnPoint = new List<Transform>(_crowdSpawnPoint);

        for (int i = 0; i < croudNumber; i++)
        {
            int randomIndex = Random.Range(0, duplicateSpawnPoint.Count);
            Transform spawnTransform = duplicateSpawnPoint[randomIndex];
            duplicateSpawnPoint.RemoveAt(randomIndex);
            Vector3 spawnPoint = CalculateSpawnPoint(spawnTransform.position);

            GameObject crowd = Instantiate(_crowdObject, spawnPoint, Quaternion.identity, _crowdFather);

            crowd.TryGetComponent(out Crowd crowdComponent);
            if (crowdComponent != null)
            {
                crowdComponent.SpawnGuests();
                _crowds.Add(crowdComponent);
            }
        }

        return _crowds;
    }

    private Vector3 CalculateSpawnPoint(Vector3 spawnPositionOffset)
    {
        Vector2 randomPoint2D = Random.insideUnitCircle * _radius;
        Vector3 spawnPosition = new Vector3(randomPoint2D.x, 0, randomPoint2D.y) + spawnPositionOffset;

        return spawnPosition;
    }

    private void OnDrawGizmos()
    {
        if (_crowdSpawnPoint == null || _crowdSpawnPoint.Count <= 0) return;

        foreach (var item in _crowdSpawnPoint)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(item.position, _radius);
        }
    }
}
