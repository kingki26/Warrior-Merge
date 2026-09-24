using UnityEngine;

public class SettingsUI : MonoBehaviour
{
    [SerializeField] private GameObject settingsPopup;

    public void OpenSettings()
    {
        if (settingsPopup == null)
        {
            return;
        }

        settingsPopup.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsPopup == null)
        {
            return;
        }

        settingsPopup.SetActive(false);
    }
}