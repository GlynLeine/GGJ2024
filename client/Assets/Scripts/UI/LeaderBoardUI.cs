using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LeaderBoardUI : MonoBehaviour
{
    [SerializeField] private Transform m_names;
    [SerializeField] private Transform m_scores;
    private GameData m_gameData;
    void Start()
    {
        m_gameData = FindObjectOfType<GameData>();
        if (m_gameData == null)
            return;

        var list = GameData.GetSortedScoreList();
        for (int i = 0; i < m_names.childCount; i++)
        {
            var name = "";
            var score = "";

            if (i < list.Count)
            {
                name = list[i].name;
                score = GameData.FormatToTime(list[i]);
            }

            m_names.GetChild(i).GetComponent<TMP_Text>().text = name;
            m_scores.GetChild(i).GetComponent<TMP_Text>().text = score;
        }
    }
}
