using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SelectionButton : MonoBehaviour
{
    #region Variables
    public UnityEvent<int> OnSelectionChange = new UnityEvent<int>();

    [SerializeField] private UnityEngine.UI.Button _next;
    [SerializeField] private UnityEngine.UI.Button _previous;

    [SerializeField] private List<GameObject> _selections;

    public int CurrentListIndex {get; private set;}

    private GameObject _lastSelection;
    #endregion

    #region Unity Standard Methods
    void OnValidate()
    {
        if (_next == null)
            DevLog.LogError($"[{this}]: The Next button is null", this);
        if (_previous == null)
            DevLog.LogError($"[{this}]: The Previous button is null", this);
        if (_selections == null || _selections.Count <= 0)
            DevLog.LogError($"[{this}]: No selection option are available", this);
    }

    void OnEnable()
    {
        _next.onClick.AddListener(NextSelection);
        _previous.onClick.AddListener(PreviousSelection);
    }

    void OnDisable()
    {
        _next.onClick.RemoveListener(NextSelection);
        _previous.onClick.RemoveListener(PreviousSelection);
    }

    #endregion

    #region Class Methods

    private void NextSelection()
    {
        DevLog.Log("Stai premendo il bottone Next");
        CurrentListIndex++;
        NewSelection();
    }

    private void PreviousSelection()
    {
        DevLog.Log("Stai premendo il bottone prev");
        CurrentListIndex--;
        NewSelection();
    }

    private void NewSelection()
    {
        OnSelectionChange?.Invoke(CurrentListIndex);

        if (_lastSelection != null)
            _lastSelection.SetActive(false);

        _lastSelection = _selections[CurrentListIndex];
        _lastSelection.SetActive(true);

        if (CurrentListIndex - 1 < 0)
        {
            _previous.interactable = false;
        }
        else if (_previous.interactable == false)
        {
            _previous.interactable = true;
        }

        if (CurrentListIndex + 1 >= _selections.Count)
        {
            _next.interactable = false;
        }
        else if (_next.interactable == false)
        {
            _next.interactable = true;
        }
    }

    public void SelectPrecise(int index)
    {
        if (index >= _selections.Count)
        {
            DevLog.LogError($"[{this}]: Indice iniettato troppo grande: {index}", this);
            return;
        }
        CurrentListIndex = index; 
        
        DevLog.Log("Lingua correttamente cambiata", this);
        NewSelection();
    }

    #endregion
}
