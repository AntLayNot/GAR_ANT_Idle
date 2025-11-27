using UnityEngine;

public class GlobalUpgradeListUI : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("RectTransform du Content du ScrollView (Content_Upgrades).")]
    public RectTransform contentParent;

    [Tooltip("Template d'item d'upgrade placé dans Content_Upgrades (désactivé).")]
    public GlobalUpgradeUI upgradeItemTemplate;

    private void Start()
    {
        BuildList();
    }

    /// <summary>
    /// Construit la liste d'upgrades globales dans le ScrollView.
    /// </summary>
    private void BuildList()
    {
        if (GlobalUpgradeManager.Instance == null)
        {
            Debug.LogError("GlobalUpgradeListUI : GlobalUpgradeManager.Instance est null.");
            return;
        }

        if (contentParent == null)
        {
            Debug.LogError("GlobalUpgradeListUI : contentParent n'est pas assigné.");
            return;
        }

        if (upgradeItemTemplate == null)
        {
            Debug.LogError("GlobalUpgradeListUI : upgradeItemTemplate n'est pas assigné.");
            return;
        }

        // Nettoie les anciens items (mais garde le template)
        foreach (Transform child in contentParent)
        {
            if (child == upgradeItemTemplate.transform) continue;
            Destroy(child.gameObject);
        }

        // Crée un item pour chaque upgrade globale
        foreach (var upg in GlobalUpgradeManager.Instance.upgrades)
        {
            if (upg == null) continue;

            // 1) Duplique le template sous Content_Upgrades
            GlobalUpgradeUI ui = Instantiate(upgradeItemTemplate, contentParent);
            ui.gameObject.SetActive(true); // le clone doit être actif

            // 2) Assigne l'upgrade
            ui.upgrade = upg;
        }
    }
}
