using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BouncesUI : MonoBehaviour
{
    public GrenadeLauncher grenadeLauncher;

    private TMPro.TMP_Text m_text;

    private void Awake()
    {
        m_text = GetComponent<TMPro.TMP_Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if (grenadeLauncher == null)
        {
            m_text.text = string.Empty;
        }
        else
        {
            m_text.text = "Bounces: " + grenadeLauncher.amountOfBounces;
        }
    }
}
