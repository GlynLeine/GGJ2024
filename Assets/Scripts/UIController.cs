using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public TMP_Text loadingText;
    public Button connectButton;

    public void ConnectButtonImpl()
    {
        connectButton.gameObject.SetActive(false);
        loadingText.gameObject.SetActive(true);
    }
}
