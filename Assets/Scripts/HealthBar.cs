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
        unitHealth =
            GetComponentInParent<UnitHealth>();

        canvasGroup =
            GetComponentInParent<CanvasGroup>();

        Refresh();
    }

    // =========================================================
    // SETUP
    // =========================================================

    public void Setup()
    {
        if (unitHealth == null)
        {
            unitHealth =
                GetComponentInParent<UnitHealth>();
        }

        if (canvasGroup == null)
        {
            canvasGroup =
                GetComponentInParent<CanvasGroup>();
        }

        Show();

        Refresh();
    }

    // =========================================================
    // REFRESH
    // =========================================================

    public void Refresh()
    {
        if (unitHealth == null)
        {
            unitHealth =
                GetComponentInParent<UnitHealth>();
        }

        if (fillImage == null)
            return;

        if (unitHealth == null)
            return;

        // -----------------------------------------------------
        // UPDATE FILL
        // -----------------------------------------------------

        fillImage.fillAmount =
            unitHealth.GetHealthPercent();

        // -----------------------------------------------------
        // HIDE WHEN FULL
        // -----------------------------------------------------

        if (hideWhenFull)
        {
            if (unitHealth.GetCurrentHealth() <
                unitHealth.GetMaxHealth())
            {
                Show();
            }
            else
            {
                Hide();
            }
        }

        // -----------------------------------------------------
        // HIDE WHEN DEAD
        // -----------------------------------------------------

        if (hideWhenDead &&
            unitHealth.IsDead())
        {
            Hide();
        }
    }

    // =========================================================
    // SHOW
    // =========================================================

    public void Show()
    {
        if (canvasGroup == null)
        {
            canvasGroup =
                GetComponentInParent<CanvasGroup>();
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    // =========================================================
    // HIDE
    // =========================================================

    public void Hide()
    {
        if (canvasGroup == null)
        {
            canvasGroup =
                GetComponentInParent<CanvasGroup>();
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }
}