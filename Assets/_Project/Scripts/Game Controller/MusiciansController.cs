using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class MusiciansController : MonoBehaviour
{
    [SerializeField] private List<Animation> _musiciansAnimations;

    private readonly string _musicianDance = "AC_Musician";

    public async void Awake()
    {
        if (_musiciansAnimations == null) return;

        Queue<Animation> music = new();

        ShuffleAndEnqueue(_musiciansAnimations, music);

        int queueCount = music.Count;

        for (int i = 0; i < queueCount; i++)
        {
            await UniTask.Delay(Random.Range(0, 200));
            music.Dequeue().Play(_musicianDance);
        }
    }

    private void ShuffleAndEnqueue(List<Animation> list, Queue<Animation> queue)
    {
        System.Random rng = new();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            var value = list[k];
            list[k] = list[n];
            list[n] = value;
        }

        queue.Clear();
        foreach (var item in list)
        {
            queue.Enqueue(item);
        }
    }
}
