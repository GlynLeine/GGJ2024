using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

struct Score
{
    public string name;
    public float score;

    public Score(string _name, float _score)
    {
        name = _name;
        score = _score;
    }
}

public class GameData : MonoBehaviour
{
    private Button m_startButton;
    [SerializeField] private string gameSceneName;
    [SerializeField] private string mainMenuSceneName;

    private TMP_InputField m_playerName;
    string PlayerName;
    public static Dictionary<string, float> playerScores = new Dictionary<string, float>();

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;

        SceneManager.LoadScene(mainMenuSceneName);
    }

    public static void AddScore(string name, float score)
    {
        playerScores.Add(name, score);
    }

    public static (string name, float score) GetHighestScore()
    {
        LoadScoresFromFile();
        float BestScore = Mathf.Infinity;
        string topPlayer = "";
        foreach (var pair in playerScores)
        {
            if (pair.Value < BestScore)
            {
                BestScore = pair.Value;
                topPlayer = pair.Key;
            }
        }

        return (topPlayer, BestScore);
    }

    public static void LoadScoresFromFile()
    {
        playerScores.Clear();
        int i = 0;
        while (true)
        {
            if (!PlayerPrefs.HasKey($"Score [{i}]"))
                break;
            var score = JsonUtility.FromJson<Score>(PlayerPrefs.GetString($"Score [{i}]"));
            playerScores.Add(score.name, score.score);
            i++;
        }
    }

    public static void WriteScoresToFile()
    {
        PlayerPrefs.DeleteAll();
        int i = 0;
        foreach (var pair in playerScores)
        {
            PlayerPrefs.SetString($"Score [{i}]", JsonUtility.ToJson(new Score(pair.Key, pair.Value)));
            i++;
        }
        PlayerPrefs.Save();
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    TimeScore timeScore;

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name.Equals(mainMenuSceneName))
        {
            m_startButton = GameObject.FindAnyObjectByType<StartButton>().GetComponent<Button>();
            m_playerName = GameObject.FindAnyObjectByType<PlayerName>().GetComponent<TMP_InputField>();

            m_startButton.onClick.AddListener(() =>
            {
                PlayerName = m_playerName.text;
            });

            if (playerScores.Count > 0)
            {
                var pair = GetHighestScore();
                FindAnyObjectByType<TopScorererUI>().topScorererName.text = $"{pair.name}, {pair.score}";
            }
        }

        if (scene.name.Equals(gameSceneName))
        {
            timeScore = FindObjectOfType<TimeScore>();
            timeScore.onGameEnd.AddListener(OnGameEnd);
        }

    }

    void OnGameEnd()
    {
        playerScores.Add(PlayerName, timeScore.currentTime);
        WriteScoresToFile();
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
