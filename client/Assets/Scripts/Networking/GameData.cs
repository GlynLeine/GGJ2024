using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public struct Score
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
    public static List<Score> playerScores = new List<Score>();

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;

        SceneManager.LoadScene(mainMenuSceneName);
    }

    public static void AddScore(string name, float score)
    {
        playerScores.Add(new Score(name, score));
    }

    public static List<Score> GetSortedScoreList()
    {
        LoadScoresFromFile();

        List<Score> sorted = playerScores;

        sorted.Sort((a, b) => b.score.CompareTo(a.score));

        return sorted;
    }

    public static Score GetHighestScore()
    {
        LoadScoresFromFile();
        var sortedList = GetSortedScoreList();
        return sortedList[0];
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
            playerScores.Add(score);
            i++;
        }
    }

    public static void WriteScoresToFile()
    {
        PlayerPrefs.DeleteAll();
        int i = 0;
        foreach (var entry in playerScores)
        {
            PlayerPrefs.SetString($"Score [{i}]", JsonUtility.ToJson(entry));
            i++;
        }
        PlayerPrefs.Save();
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public static string FormatToTime(Score entry)
    {
        int minutes = Mathf.FloorToInt(entry.score / 60.0f);

        string minutesText = minutes < 10 ? "0" + minutes : minutes.ToString();

        int seconds = Mathf.FloorToInt(entry.score - (minutes * 60));

        string secondsText = seconds < 10 ? "0" + seconds : seconds.ToString();

        int microseconds = Mathf.FloorToInt((entry.score - (seconds + (minutes * 60))) * 100);

        string microsecondsText = microseconds < 10 ? "0" + microseconds : microseconds.ToString();

        return minutesText + ":" + secondsText + "." + microsecondsText;
    }

    TimeScore timeScore;

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name.Equals(mainMenuSceneName))
        {
            m_startButton = FindAnyObjectByType<StartButton>()?.GetComponent<Button>();
            m_playerName = FindAnyObjectByType<PlayerName>()?.GetComponent<TMP_InputField>();

            m_startButton.onClick.AddListener(() =>
            {
                PlayerName = m_playerName.text;
            });

            if (playerScores.Count > 0)
            {
                var entry = GetHighestScore();
                FindAnyObjectByType<TopScorererUI>().topScorererName.text = $"{entry.name}, {FormatToTime(entry)} minutes";
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
        //if (playerScores.ContainsKey(PlayerName))
        //{
        //    if (playerScores[PlayerName] > timeScore.currentTime)
        //    {
        //        playerScores[PlayerName] = timeScore.currentTime;
        //    }
        //}
        //else
        //{

        //}

        playerScores.Add(new Score(PlayerName, timeScore.currentTime));

        WriteScoresToFile();
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
