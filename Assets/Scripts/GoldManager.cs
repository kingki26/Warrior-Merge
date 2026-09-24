using TMPro;
using UnityEngine;

public class GoldManager : MonoBehaviour
{
    [Header("Gold")]
    [SerializeField] private int startingGold = 1000;

    [Header("Melee Price")]
    [SerializeField] private int meleeStartingPrice = 20;
    [SerializeField] private int meleePriceIncrease = 5;

    [Header("Ranged Price")]
    [SerializeField] private int rangedStartingPrice = 30;
    [SerializeField] private int rangedPriceIncrease = 5;

    [Header("UI")]
    [SerializeField] private TMP_Text meleePriceText;
    [SerializeField] private TMP_Text rangedPriceText;
    [SerializeField] private TMP_Text goldText;

    [SerializeField] private GameObject meleeAdIcon;
    [SerializeField] private GameObject rangedAdIcon;


    private int currentGold;

    private int currentMeleePrice;
    private int currentRangedPrice;

    private void Awake()
    {
        currentGold = startingGold;

        currentMeleePrice = meleeStartingPrice;
        currentRangedPrice = rangedStartingPrice;

        UpdateUI();

        Debug.Log(
            "GoldManager → Starting Gold: " +
            currentGold
        );
    }

    public int GetGold()
    {
        return currentGold;
    }

    public int GetMeleePrice()
    {
        return currentMeleePrice;
    }

    public int GetRangedPrice()
    {
        return currentRangedPrice;
    }

    public bool CanAfford(int amount)
    {
        return currentGold >= amount;
    }

    public bool BuyMelee()
    {
        if (!CanAfford(currentMeleePrice))
        {
            return false;
        }

        int price = currentMeleePrice;

        currentGold -= price;
        currentMeleePrice += meleePriceIncrease;

        UpdateUI();

        return true;
    }

    public bool BuyRanged()
    {
        if (!CanAfford(currentRangedPrice))
        {
            return false;
        }

        int price = currentRangedPrice;

        currentGold -= price;
        currentRangedPrice += rangedPriceIncrease;

        UpdateUI();

        return true;
    }

    public void AddGold(int amount)
    {
        if (amount <= 0)
            return;

        currentGold += amount;

        UpdateUI();
    }

    private void UpdateUI()
    {
        bool canBuyMelee = CanAfford(currentMeleePrice);
        bool canBuyRanged = CanAfford(currentRangedPrice);

        if (meleePriceText != null)
        {
            meleePriceText.gameObject.SetActive(canBuyMelee);

            if (canBuyMelee)
            {
                meleePriceText.text = "Melee: " + FormatGold(currentMeleePrice);
            }
        }

        if (rangedPriceText != null)
        {
            rangedPriceText.gameObject.SetActive(canBuyRanged);

            if (canBuyRanged)
            {
                rangedPriceText.text = "Range: " + FormatGold(currentRangedPrice);
            }
        }

        if (meleeAdIcon != null)
        {
            meleeAdIcon.SetActive(!canBuyMelee);
        }

        if (rangedAdIcon != null)
        {
            rangedAdIcon.SetActive(!canBuyRanged);
        }

        if (goldText != null)
        {
            goldText.text = FormatGold(currentGold);
        }
    }

    private string FormatGold(int amount)
    {
        if (amount >= 1000000)
        {
            return (amount / 1000000f).ToString("0.#") + "M";
        }

        if (amount >= 1000)
        {
            return (amount / 1000f).ToString("0.#") + "k";
        }

        return amount.ToString();
    }

    public void ClearData()
    {
        currentGold = 100;
        currentMeleePrice = meleeStartingPrice;
        currentRangedPrice = rangedStartingPrice;

        UpdateUI();
    }
}