using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsOpener : MonoBehaviour
{
    public void ToggleSettings()
    {
        SettingsManager.Instance.ToggleOpen();
    }
}
