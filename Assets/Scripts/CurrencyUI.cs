using UnityEngine;
using TMPro;

public class CurrencyUI : MonoBehaviour
{
    public TMP_Text stardustText;

    private void Update()
    {
        if (GameManager.Instance == null || stardustText == null) return;

        double amount = GameManager.Instance.stardust;
        stardustText.text = $"Poussière d'étoile : {NumberFormatter.Format(amount)}";

    }
}
