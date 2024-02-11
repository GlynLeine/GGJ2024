using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UINavButton : MonoBehaviour
{
    [SerializeField] private GameObject m_toDisable;
    [SerializeField] private GameObject m_toEnable;
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(() => 
        {
            m_toDisable.gameObject.SetActive(false);
            m_toEnable.gameObject.SetActive(true);
        });
    }
}
