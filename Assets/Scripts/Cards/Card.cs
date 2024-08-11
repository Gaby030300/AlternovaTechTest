using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    [Header("Card Properties")]
    [SerializeField] private Image cardImage;
    [SerializeField] private Sprite hiddenImage;
    [SerializeField] private Sprite revealedImage;
    [SerializeField] private TMP_Text _cardNumber;
    
    public int CardNumber => cardNumber;
    public int cardNumber;
   
    [Header("Score")]
    public int _cardScore;
    
    [Header("Card Button")]
    [SerializeField] private Button _cardButton;
    
    private bool _isRevealed;

    private void Start()
    {
        cardImage.sprite = hiddenImage;
        _cardButton.onClick.AddListener(OnCardClick);
        _cardNumber.gameObject.SetActive(false);
    }
    
    public void Setup(int number)
    {
        cardNumber = number;
        _cardNumber.text = number.ToString();
    }

    public void OnCardClick()
    {
        AudioManager.instance.PlaySFX("Swipe");
        
        if (_isRevealed || FindObjectOfType<GameController>().IsChecking) return;
        
        RevealCard();
        FindObjectOfType<GameController>().OnCardRevealed(this);
    }

    private void RevealCard()
    {
        cardImage.sprite = revealedImage;  
        _cardNumber.gameObject.SetActive(true);
        _isRevealed = true;
    }

    public void HideCard()
    {
        cardImage.sprite = hiddenImage; 
        _cardNumber.gameObject.SetActive(false);
        _isRevealed = false;
    }
    
    
}
