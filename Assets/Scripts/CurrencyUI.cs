using UnityEngine;
using TMPro;

public class CurrencyUI : MonoBehaviour
{
    public TMP_Text stardustText;
    public TMP_Text stardustWorshipText;
    public TMP_Text stardustPerSecondText;

    private void Update()
    {
        if (GameManager.Instance == null || stardustText == null || stardustWorshipText == null || stardustPerSecondText == null) return;

        double amount = GameManager.Instance.stardust;
        stardustText.text = $"Poussière d'étoile : {NumberFormatter.Format(amount)}";

        double Totalamount = GameManager.Instance.totalStardustEarned;
        stardustWorshipText.text = $"Poussière d'étoile total : {NumberFormatter.Format(Totalamount)}";

        double PerSecondamount = GameManager.Instance.totalPerSecond;
        stardustPerSecondText.text = $"Poussière d'étoile : {NumberFormatter.Format(PerSecondamount)}/s";

    }
}
