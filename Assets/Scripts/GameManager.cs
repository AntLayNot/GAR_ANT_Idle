using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Currency")]
    public double stardust = 0;              // Poussière d'étoile actuelle
    public double totalStardustEarned = 0;   // Total de poussière gagnée depuis le début
    public double  totalPerSecond = 0;       // Poussières générée par /s

    [Header("All Generators in the Scene")]
    public List<Generator> generators = new List<Generator>();

    // Keys de sauvegarde
    private const string StardustKey = "ASTRAL_STARDUST";
    private const string TotalStardustEarnedKey = "ASTRAL_TOTAL_STARDUST";
    private const string LastSaveKey = "ASTRAL_LAST_SAVE_TIME";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        LoadGame();
    }

    private void Update()
    {

        foreach (var gen in generators)
        {
            if (gen != null)
                totalPerSecond += gen.GetProductionPerSecond();
        }

        double globalProdMult = 1.0;
        if (GlobalUpgradeManager.Instance != null)
            globalProdMult = GlobalUpgradeManager.Instance.globalProductionMultiplier;

        double effectivePerSecond = totalPerSecond * globalProdMult;
        double deltaGain = effectivePerSecond * Time.deltaTime;

        if (deltaGain > 0)
        {
            AddStardust(deltaGain);
        }
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }

    public void AddStardust(double amount)
    {
        if (amount <= 0) return; // on ne compte que le gain positif dans le total
        
        stardust += amount;
        totalStardustEarned += amount;

        if (stardust < 0) stardust = 0;
    }

    public bool SpendStardust(double amount)
    {
        if (stardust >= amount)
        {
            stardust -= amount;
            return true;
        }

        return false;
    }

    public void SaveGame()
    {
        // Sauvegarde de la monnaie actuelle
        PlayerPrefs.SetString(StardustKey, stardust.ToString("R"));
        // Sauvegarde du total gagné
        PlayerPrefs.SetString(TotalStardustEarnedKey, totalStardustEarned.ToString("R"));

        // Sauvegarde du niveau de chaque générateur
        foreach (var gen in generators)
        {
            if (gen == null) continue;
            string levelKey = $"ASTRAL_GEN_{gen.id}_LEVEL";
            PlayerPrefs.SetInt(levelKey, gen.level);
        }

        // Sauvegarde du timestamp pour l'offline
        long ticks = DateTime.UtcNow.Ticks;
        PlayerPrefs.SetString(LastSaveKey, ticks.ToString());

        PlayerPrefs.Save();
        if (GlobalUpgradeManager.Instance != null)
        {
            GlobalUpgradeManager.Instance.SaveUpgrades();
        }

        Debug.Log("Game Saved.");
    }

    public void ResetGame()
    {
        stardust = 10;
        totalStardustEarned = 0;
        totalPerSecond = 0;

        foreach (var gen in generators)
        {
            if (gen == null) continue;
            gen.level = 0;

            string levelKey = $"ASTRAL_GEN_{gen.id}_LEVEL";
            PlayerPrefs.DeleteKey(levelKey);
        }

        if (GlobalUpgradeManager.Instance != null)
        {
            foreach (var upg in GlobalUpgradeManager.Instance.upgrades)
            {
                if (upg == null) continue;
                upg.level = 0;

                string upgradeKey = $"ASTRAL_UPG_{upg.id}_LEVEL";
                PlayerPrefs.DeleteKey(upgradeKey);
            }

            GlobalUpgradeManager.Instance.RecomputeMultipliers();
        }

        PlayerPrefs.DeleteKey(StardustKey);
        PlayerPrefs.DeleteKey(TotalStardustEarnedKey);
        PlayerPrefs.DeleteKey(LastSaveKey);
        PlayerPrefs.Save();
    }



    private void LoadGame()
    {
        // Chargement de la currency actuelle
        if (PlayerPrefs.HasKey(StardustKey))
        {
            string value = PlayerPrefs.GetString(StardustKey, "0");
            double parsed;
            if (double.TryParse(value, out parsed))
            {
                stardust = parsed;
            }
        }

        // Chargement du total gagné
        if (PlayerPrefs.HasKey(TotalStardustEarnedKey))
        {
            string value = PlayerPrefs.GetString(TotalStardustEarnedKey, "0");
            double parsed;
            if (double.TryParse(value, out parsed))
            {
                totalStardustEarned = parsed;
            }
        }

        // Chargement des niveaux des générateurs
        foreach (var gen in generators)
        {
            if (gen == null) continue;

            string levelKey = $"ASTRAL_GEN_{gen.id}_LEVEL";
            if (PlayerPrefs.HasKey(levelKey))
            {
                gen.level = PlayerPrefs.GetInt(levelKey, 0);
            }
            else
            {
                // s'il n'y a PAS de sauvegarde pour ce générateur, niveau 0.
                gen.level = 0;
            }
        }




        // Offline progress
        if (PlayerPrefs.HasKey(LastSaveKey))
        {
            string ticksStr = PlayerPrefs.GetString(LastSaveKey, "0");
            long ticks;
            if (long.TryParse(ticksStr, out ticks))
            {
                DateTime lastSave = new DateTime(ticks, DateTimeKind.Utc);
                TimeSpan delta = DateTime.UtcNow - lastSave;

                double secondsAway = Math.Max(0, delta.TotalSeconds);

                double totalPerSecond = 0;
                foreach (var gen in generators)
                {
                    if (gen != null)
                        totalPerSecond += gen.GetProductionPerSecond();
                }

                double offlineGain = totalPerSecond * secondsAway;

                // applique le multiplicateur d'upgrade offline
                double offlineMult = 1.0;
                if (GlobalUpgradeManager.Instance != null)
                    offlineMult = GlobalUpgradeManager.Instance.offlineGainMultiplier;

                offlineGain *= offlineMult;

                if (offlineGain > 0)
                {
                    AddStardust(offlineGain);
                }

                Debug.Log($"Offline gain: +{offlineGain} stardust (away {secondsAway} s)");
            }
        }
        if (GlobalUpgradeManager.Instance != null)
        {
            GlobalUpgradeManager.Instance.LoadUpgrades();
        }

    }
}
