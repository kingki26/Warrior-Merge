using UnityEngine;

public class DefeatUI : MonoBehaviour
{
    [Header("Defeat Panel")]
    [SerializeField] private GameObject defeatPanel;

    private void Start()
    {
        HideDefeat();
    }

    public void ShowDefeat()
    {
        if (defeatPanel != null)
        {
            defeatPanel.SetActive(true);
        }
    }

    public void HideDefeat()
    {
        if (defeatPanel != null)
        {
            defeatPanel.SetActive(false);
        }
    }
}