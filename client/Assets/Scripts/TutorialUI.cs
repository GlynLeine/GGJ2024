using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TutorialUI : MonoBehaviour
{
    [SerializeField] private Button m_playButton;
    // Start is called before the first frame update
    void Start()
    {
        m_playButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("Game");
        });
    }
}
