using UnityEngine;

public class VictoryUI : MonoBehaviour
{
    [Header("Victory Panel")]
    [SerializeField] private GameObject victoryPanel;

    [Header("Other UI")]
    [SerializeField] private GameObject levelAmount;

    private void Start()
    {
        HideVictory();
    }

    public void ShowVictory()
    {
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }

        if (levelAmount != null)
        {
            levelAmount.SetActive(false);
        }
    }

    public void HideVictory()
    {
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(false);
        }

        if (levelAmount != null)
        {
            levelAmount.SetActive(true);
        }
    }
}