using UnityEngine;

public class DefeatUI : MonoBehaviour
{
    [Header("Defeat Panel")]
    [SerializeField] private GameObject defeatPanel;

    [Header("Other UI")]
    [SerializeField] private GameObject levelAmount;

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

        if (levelAmount != null)
        {
            levelAmount.SetActive(false);
        }
    }

    public void HideDefeat()
    {
        if (defeatPanel != null)
        {
            defeatPanel.SetActive(false);
        }

        if (levelAmount != null)
        {
            levelAmount.SetActive(true);
        }
    }
}