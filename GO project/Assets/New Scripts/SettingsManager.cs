using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsManager : Singleton<SettingsManager>
{
    public KeyCode settingsKey;
    public GameObject settingsRoot;

    void Update()
    {
        if(settingsRoot == null) return;

        if(Input.GetKeyDown(settingsKey))
        {
            ToggleOpen();
        }
    }

    public void ToggleOpen()
    {
        settingsRoot.SetActive(!settingsRoot.activeSelf);
    }
}
