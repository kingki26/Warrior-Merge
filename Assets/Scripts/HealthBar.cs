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

    private void Awake()
    {
        unitHealth =
            GetComponentInParent<UnitHealth>();

        Refresh();
    }

    public void Refresh()
    {
        if (unitHealth == null)
        {
            unitHealth =
                GetComponentInParent<UnitHealth>();
        }

        if (unitHealth == null)
            return;

        if (fillImage == null)
            return;

        fillImage.fillAmount =
            unitHealth.GetHealthPercent();

        // -----------------------------------------------------
        // HIDE WHEN FULL
        // -----------------------------------------------------

        if (hideWhenFull)
        {
            gameObject.SetActive(
                unitHealth.GetCurrentHealth() <
                unitHealth.GetMaxHealth()
            );
        }

        // -----------------------------------------------------
        // HIDE WHEN DEAD
        // -----------------------------------------------------

        if (hideWhenDead &&
            unitHealth.IsDead())
        {
            gameObject.SetActive(false);
        }
    }
}