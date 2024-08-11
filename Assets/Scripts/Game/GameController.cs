using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class GameController : MonoBehaviour
{
    private const int MinValueRowColumn = 2;
    private const int MaxValueRowColumn = 8;
    
    [Header("Cards Settings")]
    [SerializeField] private Transform gridParent;
    [SerializeField] private List<GameObject> cardPrefabs;

    [Header("Result Information")] 
    [SerializeField] private int score;
    [SerializeField] private float time;
    [SerializeField] private int clicks;
    [SerializeField] private int pairs;
    
    private int totalPairs;
    
    [Header("Result Information UI")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text clicksText;
    [SerializeField] private TMP_Text pairsText;

    [Header("UI")] 
    [SerializeField] private GameObject endGamePanel;
    [SerializeField] private UIManager uiManager;
    
    [Header ("JSON File Grid Data")]
    [SerializeField] private TextAsset jsonFile;
    
    private BlockList blockList;
    
    [Header("Check Card System")]
    private List<Card> revealedCards = new();

    [Header("Visual Effects")] 
    [SerializeField] private ParticleSystem startParticleSystem;
    
    public bool IsChecking { get; private set; }

    private void Start()
    {
        Time.timeScale = 0;
        
        blockList = JsonUtility.FromJson<BlockList>(jsonFile.text);
        endGamePanel.transform.localScale = Vector3.zero;
        
        uiManager.OnPlayGame += OnPlayGame;
        
        IsChecking = false;
       
    }

    private void OnPlayGame()
    {
        Time.timeScale = 1;
        Invoke(nameof(InitializeGrid), 2);
    }

    private void Update()
    {
        SetTimer();
    }

    private void SetTimer()
    {
        time += Time.deltaTime;

        int minutes = Mathf.FloorToInt(time / 60F);
        int seconds = Mathf.FloorToInt(time % 60F);

        string timeFormatted = string.Format("{0:00}:{1:00}", minutes, seconds);

        timeText.text = timeFormatted;
    }

    private void InitializeGrid()
    {
        startParticleSystem.Play();
        
        int maxRow = 0;
        int maxCol = 0;

        foreach (Block block in blockList.blocks)
        {
            if (block.R > maxRow) maxRow = block.R;
            if (block.C > maxCol) maxCol = block.C;
        }

        maxRow = Mathf.Clamp(maxRow, MinValueRowColumn, MaxValueRowColumn);
        maxCol = Mathf.Clamp(maxCol, MinValueRowColumn, MaxValueRowColumn);

        CreateCards(maxRow, maxCol);
    }

    private void CreateCards(int maxRow, int maxCol)
    {
        foreach (Transform child in gridParent)
        {
            Destroy(child.gameObject);
        }

        float cardWidth = 190.0f;  
        float cardHeight = 190.0f;
        float spacing = 30.0f;

        foreach (Block block in blockList.blocks)
        {
            if (block.number >= 0 && block.number < cardPrefabs.Count)
            {
                GameObject card = Instantiate(cardPrefabs[block.number], gridParent);

                float xPosition = (block.C - 1) * (cardWidth + spacing);
                float yPosition = -(block.R - 1) * (cardHeight + spacing);

                card.transform.localPosition = new Vector3(xPosition, yPosition, 0);

                Card cardController = card.GetComponent<Card>();
                cardController.Setup(block.number);
                
                card.transform.localScale = Vector3.zero;
                card.transform.DOScale(Vector3.one, 2f).SetEase(Ease.OutBack);
            }

        }
    }
    
    private int CalculatePairs()
    {
        int totalBlocks = blockList.blocks.Count;
        totalPairs = totalBlocks / 2;
        return totalPairs;
    }
    
    public void OnCardRevealed(Card revealedCard)
    {
        if (IsChecking) return;

        revealedCards.Add(revealedCard);

        if (revealedCards.Count == 2)
        {
            StartCoroutine(CheckMatch());
        }
    }
    
    private IEnumerator CheckMatch()
    {
        IsChecking = true;

        yield return new WaitForSeconds(1.0f);

        if (revealedCards[0].CardNumber == revealedCards[1].CardNumber)
        {
            foreach (var card in revealedCards)
            {
                score += card._cardScore;
            }

            scoreText.text = score.ToString();
            
            AudioManager.instance.PlaySFX("Correct");
            
            revealedCards.Clear();
            
            pairs++;
            pairsText.text = pairs.ToString();
            
            clicks++;
            clicksText.text = clicks.ToString();

            EndGame();
        }
        else
        {
            revealedCards[0].HideCard();
            revealedCards[1].HideCard();
            
            AudioManager.instance.PlaySFX("Error");
            
            revealedCards.Clear();
            
            clicks++;
            clicksText.text = clicks.ToString();
        }

        IsChecking = false;
    }

    private void EndGame()
    {
        int totalGamePairs = CalculatePairs();
        if (totalGamePairs != pairs) return;
        
        
        OpenPopup();
        SaveResults(clicks, (int) time, pairs, score);
    }

    private void OpenPopup()
    {
        endGamePanel.transform.DOScale(Vector3.one, 2).SetEase(Ease.OutBack).OnComplete(() =>
        {
            Time.timeScale = 0;
        });
    }


    private void SaveResults(int clicks, int time, int pairs, int score)
    {
        GameResult result = new GameResult
        {
            total_clicks = clicks,
            total_time = time,
            pairs = pairs,
            score = score
        };

        string json = JsonUtility.ToJson(result, true);
        System.IO.File.WriteAllText(Application.dataPath + "/Results.json", json);
    }
}
