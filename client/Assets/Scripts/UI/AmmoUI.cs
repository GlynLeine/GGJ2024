using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoUI : MonoBehaviour
{
    public Weapon_Ranged weapon;

    private TMPro.TMP_Text m_text;

    private void Awake()
    {
        m_text = GetComponent<TMPro.TMP_Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if(weapon == null)
        {
            m_text.text = string.Empty;
        }
        else
        {
            m_text.text = "Ammo: " + weapon.currentAmmo;
        }
    }
}
