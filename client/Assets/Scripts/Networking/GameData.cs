using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

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

    [SerializeField] private string gameSceneName;
    [SerializeField] private string mainMenuSceneName;

    private Button m_startButton;
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
            m_startButton = GameObject.FindAnyObjectByType<StartButton>()?.GetComponent<Button>();
            m_playerName = GameObject.FindAnyObjectByType<PlayerName>()?.GetComponent<TMP_InputField>();

            m_startButton.onClick.AddListener(() =>
            {
                PlayerName = m_playerName.text;
            });

            if (playerScores.Count > 0)
            {
                var pair = GetHighestScore();

                int minutes = Mathf.FloorToInt(pair.score / 60.0f);

                string minutesText = minutes < 10 ? "0" + minutes : minutes.ToString();

                int seconds = Mathf.FloorToInt(pair.score - (minutes * 60));

                string secondsText = seconds < 10 ? "0" + seconds : seconds.ToString();

                int microseconds = Mathf.FloorToInt((pair.score - (seconds + (minutes * 60))) * 100);

                string microsecondsText = microseconds < 10 ? "0" + microseconds : microseconds.ToString();

                FindAnyObjectByType<TopScorererUI>().topScorererName.text = $"{pair.name}, {minutesText + ":" + secondsText + "." + microsecondsText} minutes";
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
        if (playerScores.ContainsKey(PlayerName))
        {
            if (playerScores[PlayerName] > timeScore.currentTime)
            {
                playerScores[PlayerName] = timeScore.currentTime;
            }
        }
        else
        {
            playerScores.Add(PlayerName, timeScore.currentTime);
        }

        WriteScoresToFile();
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
