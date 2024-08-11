using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject startMenu;
    [SerializeField] private Button playButton;

    public Action OnPlayGame;
    
    private void Start()
    {
        playButton.onClick.AddListener(OnPlayButton);
    }

    private void OnPlayButton()
    {
        CloseWindow();
        OnPlayGame?.Invoke();
    }


    public void CloseWindow()
    {
        startMenu.transform.DOScale(Vector3.zero, 1).SetEase(Ease.InBack);
    }
}
