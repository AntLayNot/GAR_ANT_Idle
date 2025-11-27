using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GlobalUpgradeUI : MonoBehaviour
{
    public GlobalUpgrade upgrade;

    [Header("UI")]
    public TMP_Text nameText;
    public TMP_Text descriptionText;
    public TMP_Text levelText;
    public TMP_Text effectText;
    public TMP_Text costText;
    public Button buyButton;

    private void Awake()
    {

    }

    private void Start()
    {
        if (buyButton != null)
            buyButton.onClick.AddListener(OnBuyClicked);

        Refresh();
    }

    private void Update()
    {
        Refresh();
    }

    private void Refresh()
    {
        if (upgrade == null)
        {
            Debug.LogWarning($"{name} : upgrade n'est pas assigné sur GlobalUpgradeUI.");
            return;
        }

        if (nameText != null)
            nameText.text = upgrade.displayName;

        if (descriptionText != null)
            descriptionText.text = upgrade.description;

        if (levelText != null)
            levelText.text =
                $"Niveau : {upgrade.level}/{(upgrade.maxLevel <= 0 ? "∞" : upgrade.maxLevel.ToString())}";

        if (effectText != null)
        {
            double totalValue = upgrade.GetCurrentValue() * 100.0;
            effectText.text = $"Bonus total : +{totalValue:0.#}%";
        }

        if (costText != null)
        {
            if (upgrade.IsMaxed)
            {
                costText.text = "Max";
            }
            else
            {
                double cost = upgrade.GetNextCost();
                costText.text = $"Coût : {NumberFormatter.Format(cost)}";
            }
        }

        if (buyButton != null)
        {
            buyButton.interactable = !upgrade.IsMaxed;
        }
    }

    private void OnBuyClicked()
    {
        if (upgrade == null) return;

        bool bought = upgrade.TryBuy();
        if (!bought)
        {
            Debug.Log("Pas assez de poussière d'étoile pour acheter cette upgrade.");
        }

        Refresh();
    }
}
