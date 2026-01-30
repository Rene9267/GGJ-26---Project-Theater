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
    [SerializeField] private AudioSource _audio;
    [SerializeField] private Animation _animation;
    private readonly string FadeIn = "AC_FadeInCanvas";
    private readonly string FadeOut = "AC_FadeOutCanvas";
    private readonly string FadeInCredits = "AC_FadeInCredits";
    private readonly string FadeoutCredits= "AC_FadeOutCredits";
    
    private readonly string FadeinTutorial= "AC_FadeinTutorial";


    [SerializeField] private AudioSource _buttonEffect;

    void Start()
    {
        StartGame();
    }

    public void PlayClick(AudioClip newClip)
    {
        _buttonEffect.Stop();
        _buttonEffect.clip = newClip;
        _buttonEffect.Play();
    }

    private async void StartGame()
    {
        await UniTask.Delay(500);
        _animation.Play(FadeIn);
        _audio.Play();
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

    public void ShowCredits()
    {
        _animation.Play(FadeInCredits);
    }
    public void HideCredits()
    {
        _animation.Play(FadeoutCredits);
    }
    public void ShowTutorial()
    {
        _animation.Play(FadeinTutorial);
    }
}
