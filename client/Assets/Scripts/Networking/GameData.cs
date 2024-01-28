using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;


struct Score
{
    public string name;
    public int score;

    public Score(string _name, int _score)
    {
        name = _name;
        score = _score;
    }
}
public class GameData : MonoBehaviour
{
    [SerializeField] private string gameSceneName;
    [SerializeField] private string mainMenuSceneName;

    public static Dictionary<string, int> playerScores = new Dictionary<string, int>();

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public static void AddScore(string name, int score)
    {
        playerScores.Add(name, score);
    }

    public static (string name, int score) GetHighestScore()
    {
        int MaxScore = 0;
        string topPlayer = "";
        foreach (var pair in playerScores)
        {
            if (pair.Value > MaxScore)
            {
                MaxScore = pair.Value;
                topPlayer = pair.Key;
            }
        }

        return (topPlayer, MaxScore);
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

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name.Equals(mainMenuSceneName) && playerScores.Count > 0)
        {
            var pair = GetHighestScore();
            FindAnyObjectByType<TopScorererUI>().topScorererName.text = $"{pair.name}: {pair.score}";
        }

    }
}
