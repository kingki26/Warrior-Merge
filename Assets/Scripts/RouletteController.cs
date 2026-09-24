using System.Collections;
using UnityEngine;

public class RouletteController : MonoBehaviour
{
    [Header("Roulette")]
    [SerializeField] private RectTransform rouletteSpin;
    [SerializeField] private RectTransform[] slots;

    [Header("Spin Settings")]
    [SerializeField] private int spinRounds = 5;
    [SerializeField] private float spinDuration = 3f;

    [Header("Reward Multipliers")]
    [SerializeField] private int[] multipliers;

    private bool isSpinning;
    private int currentRewardMultiplier = 1;

    public void AutoSpin()
    {
        if (isSpinning)
        {
            return;
        }

        if (rouletteSpin == null)
        {
            Debug.LogError("RouletteController → Roulette Spin is NULL!");
            return;
        }

        if (slots == null || slots.Length == 0)
        {
            Debug.LogError("RouletteController → Slots are NULL or empty!");
            return;
        }

        int resultIndex = Random.Range(0, slots.Length);

        if (multipliers != null && resultIndex < multipliers.Length)
        {
            currentRewardMultiplier = multipliers[resultIndex];
        }
        else
        {
            currentRewardMultiplier = 1;
        }

        StartCoroutine(SpinToSlot(resultIndex));
    }

    private IEnumerator SpinToSlot(int resultIndex)
    {
        isSpinning = true;

        RectTransform targetSlot = slots[resultIndex];

        if (targetSlot == null)
        {
            isSpinning = false;
            yield break;
        }

        Vector2 slotPosition = targetSlot.anchoredPosition;
        float slotAngle = Mathf.Atan2(slotPosition.x, slotPosition.y) * Mathf.Rad2Deg;

        float startRotation = rouletteSpin.localEulerAngles.z;
        float currentRotation = Mathf.DeltaAngle(0f, startRotation);
        float rotationDifference = slotAngle - currentRotation;
        float totalRotation = rotationDifference + 360f * spinRounds;

        float elapsed = 0f;

        while (elapsed < spinDuration)
        {
            elapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(elapsed / spinDuration);
            float easedProgress = 1f - Mathf.Pow(1f - progress, 3f);

            float rotation = startRotation + totalRotation * easedProgress;
            rouletteSpin.localRotation = Quaternion.Euler(0f, 0f, rotation);

            yield return null;
        }

        rouletteSpin.localRotation = Quaternion.Euler(0f, 0f, startRotation + totalRotation);

        isSpinning = false;

        Debug.Log("Roulette Result → Slot: " + resultIndex + " | Multiplier: x" + currentRewardMultiplier);
    }

    public int GetRewardMultiplier()
    {
        return currentRewardMultiplier;
    }

    public bool IsSpinning()
    {
        return isSpinning;
    }
}