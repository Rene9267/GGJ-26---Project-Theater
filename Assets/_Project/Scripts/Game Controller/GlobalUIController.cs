using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

public class GlobalUIController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _peopleNumber;
    [SerializeField] private Animation _anim;

    private readonly string FadeIn = "AC_FadeInCanvas_Start";

    public void SetPeopleNumber(int newCount)
    {
        _peopleNumber.text = newCount.ToString();
    }

    public void StartUp()
    {
        _anim.Play(FadeIn);
    }
}
