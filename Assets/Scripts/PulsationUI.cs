using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PulsationUI : MonoBehaviour
{
    public Slider slider;
    public TMP_Text infoText;
    public TMP_Text hitText;

    private float hitTimer = 0f;

    private void Update()
    {
        var pm = PulsationManager.Instance;
        if (pm == null) return;

        if (slider != null)
            slider.value = pm.charge01;

        if (infoText != null)
        {
            double mult = pm.GetProductionMultiplier();
            string surge = pm.IsSurgeActive() ? $"  SURGE {pm.GetSurgeRemaining():0.0}s" : "";
            infoText.text = $"Combo {pm.GetCombo()}  |  x{mult:0.00}{surge}";
        }

        // Feedback du hit (Perfect/Good/Miss)
        if (hitText != null)
        {
            if (pm.lastHit != PulsationManager.HitQuality.None)
            {
                hitTimer = 0.35f;
                hitText.text = pm.lastHit.ToString();
                pm.lastHit = PulsationManager.HitQuality.None;
            }

            if (hitTimer > 0f)
            {
                hitTimer -= Time.unscaledDeltaTime;
                hitText.gameObject.SetActive(true);
            }
            else
            {
                hitText.gameObject.SetActive(false);
            }
        }
    }
}
