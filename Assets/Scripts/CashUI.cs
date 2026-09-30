using UnityEngine;
using TMPro;
using System.Collections;

public class CashUI : MonoBehaviour
{
    public TextMeshProUGUI cashText;

    private int displayedCash;
    private Coroutine countRoutine;

    void Start()
    {
        CashManager.Instance.OnCashChanged += OnCashChanged;
        displayedCash = CashManager.Instance.cash;
        cashText.text = displayedCash.ToString("N0") + " G";
    }

    void OnDestroy()
    {
        if (CashManager.Instance != null)
            CashManager.Instance.OnCashChanged -= OnCashChanged;
    }

    void OnCashChanged(int newCash)
    {
        if (countRoutine != null) StopCoroutine(countRoutine);
        countRoutine = StartCoroutine(CountTo(newCash));
    }

    IEnumerator CountTo(int target)
    {
        int start = displayedCash;
        float duration = 0.4f;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            displayedCash = (int)Mathf.Lerp(start, target, t / duration);
            cashText.text = displayedCash.ToString("N0") + " G";
            yield return null;
        }

        displayedCash = target;
        cashText.text = displayedCash.ToString("N0") + " G";
    }
}