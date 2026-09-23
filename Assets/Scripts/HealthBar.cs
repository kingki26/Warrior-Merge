using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image fillImage;

    [Header("Settings")]
    [SerializeField] private bool hideWhenFull = false;
    [SerializeField] private bool hideWhenDead = true;

    private UnitHealth unitHealth;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        unitHealth = GetComponentInParent<UnitHealth>();
        canvasGroup = GetComponentInParent<CanvasGroup>();

        Refresh();
    }
    public void Setup()
    {
        if (unitHealth == null)
        {
            unitHealth = GetComponentInParent<UnitHealth>();
        }

        if (canvasGroup == null)
        {
            canvasGroup = GetComponentInParent<CanvasGroup>();
        }

        Show();

        Refresh();
    }
    public void Refresh()
    {
        if (unitHealth == null)
        {
            unitHealth = GetComponentInParent<UnitHealth>();
        }

        if (fillImage == null)
            return;

        if (unitHealth == null)
            return;


        fillImage.fillAmount = unitHealth.GetHealthPercent();


        if (hideWhenFull)
        {
            if (unitHealth.GetCurrentHealth() < unitHealth.GetMaxHealth())
            {
                Show();
            }
            else
            {
                Hide();
            }
        }
        if (hideWhenDead && unitHealth.IsDead())
        {
            Hide();
        }
    }

    public void Show()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponentInParent<CanvasGroup>();
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }
    public void Hide()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponentInParent<CanvasGroup>();
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }
}