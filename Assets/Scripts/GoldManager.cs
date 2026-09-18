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
            Debug.Log(
                "GoldManager → Not enough Gold for Melee! " +
                "Need: " +
                currentMeleePrice +
                " | Current: " +
                currentGold
            );

            return false;
        }

        int price = currentMeleePrice;

        currentGold -= price;
        currentMeleePrice += meleePriceIncrease;

        Debug.Log(
            "BUY MELEE → Spent: " +
            price +
            " | Remaining Gold: " +
            currentGold +
            " | Next Price: " +
            currentMeleePrice
        );

        UpdateUI();

        return true;
    }

    public bool BuyRanged()
    {
        if (!CanAfford(currentRangedPrice))
        {
            Debug.Log(
                "GoldManager → Not enough Gold for Ranged! " +
                "Need: " +
                currentRangedPrice +
                " | Current: " +
                currentGold
            );

            return false;
        }

        int price = currentRangedPrice;

        currentGold -= price;
        currentRangedPrice += rangedPriceIncrease;

        Debug.Log(
            "BUY RANGED → Spent: " +
            price +
            " | Remaining Gold: " +
            currentGold +
            " | Next Price: " +
            currentRangedPrice
        );

        UpdateUI();

        return true;
    }

    public void AddGold(int amount)
    {
        if (amount <= 0)
            return;

        currentGold += amount;

        Debug.Log(
            "GoldManager → Added " +
            amount +
            " Gold | Current: " +
            currentGold
        );

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (meleePriceText != null)
        {
            meleePriceText.text =
                "Melee: " +
                FormatGold(currentMeleePrice);
        }

        if (rangedPriceText != null)
        {
            rangedPriceText.text =
                "Range: " +
                FormatGold(currentRangedPrice);
        }

        if (goldText != null)
        {
            goldText.text =
                FormatGold(currentGold);
        }
    }

    private string FormatGold(int amount)
    {
        if (amount >= 1000000)
        {
            return (amount / 1000000f)
                .ToString("0.#") + "M";
        }

        if (amount >= 1000)
        {
            return (amount / 1000f)
                .ToString("0.#") + "k";
        }

        return amount.ToString();
    }
}