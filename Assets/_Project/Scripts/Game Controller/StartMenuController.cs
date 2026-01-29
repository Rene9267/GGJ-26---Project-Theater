using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartMenuController : MonoBehaviour
{
    private readonly string _gamePlayScene = "Scene_Main";
    [SerializeField] private CanvasGroup myCanvasGroup;
    [SerializeField] private CanvasGroup myImage;

[SerializeField] private Animation _animation;
private readonly string FadeIn = "AC_FadeInCanvas";
    private readonly string FadeOut = "AC_FadeOutCanvas";


    void Start()
    {
        StartGame();
    }
    
    private async void StartGame()
    {
        await UniTask.Delay(500);
        _animation.Play(FadeIn);
    }

    public async void OnStartClick()
    {
        await UniTask.Delay(500);
        _animation.Play(FadeOut);
        await UniTask.Delay(1200);

        SceneManager.LoadScene(_gamePlayScene);
    }

    public async void OnExitClick()
    {
        await UniTask.Delay(1000);

        Application.Quit();
    }
}
