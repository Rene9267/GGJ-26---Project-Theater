using TMPro;
using UnityEngine;

public class GlobalUIController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _peopleNumber;

    public void SetPeopleNumber(int newCount)
    {
        _peopleNumber.text = newCount.ToString();
    }
}
