using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private string m_targetScene;
    [SerializeField] private Button m_startButton;
    [SerializeField] private Button m_quitButton;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        m_quitButton.onClick.AddListener(() =>
        {
            Application.Quit();
        });

        m_startButton.onClick.AddListener(()=>
        {
            SceneManager.LoadScene(m_targetScene);
        });
    }
}
