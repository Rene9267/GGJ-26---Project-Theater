using System.Collections.Generic;
using UnityEngine;

public class ActorController : MonoBehaviour
{
    [SerializeField] List<Actor> _myActor;
    [SerializeField] List<AudioClip> _mySpeach = new();

    private float _actTime = 2;
    private float _tmpTime;
    private bool _isActStarted = false;

    public void StartAct()
    {
        _tmpTime = _actTime;
        _isActStarted = true;
    }

    void Update()
    {
        if(_isActStarted)
        {
            _tmpTime -= Time.deltaTime;
            if (_tmpTime <= 0)
            {
                int randomIndex = Random.Range(0, _myActor.Count);
                int randomSpeachIndex = Random.Range(0, _mySpeach.Count);

                _myActor[randomIndex].PrepareMySpeach(_mySpeach[randomSpeachIndex]);
                _tmpTime = _actTime;
            }
        }
    }

    public void StopAct()
    {
        _isActStarted = false;
    }
}
