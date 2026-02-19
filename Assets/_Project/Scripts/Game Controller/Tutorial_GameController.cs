using UnityEngine;

public class Tutorial_GameController : MonoBehaviour
{
    #region Parameters

    [SerializeField] private Animator _animator;


    private static readonly int _showTutorial = Animator.StringToHash("StartCameraMotion");
    #endregion


    #region Unity Methods

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    #endregion
}
