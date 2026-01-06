using System.Collections.Generic;
using Unity.Android.Gradle;
using UnityEngine;

public class GeneratorListUI : MonoBehaviour
{
    [Header("Références UI")]
    public RectTransform contentParent;      // RectTransform de Content
    public GameObject generatorPanelPrefab;  // Prefab du panel étoile
    public GameObject lineSegmentPrefab;     // Prefab du trait (Image)

    [Header("Disposition zigzag")]
    public float verticalSpacing = 260f;     // distance en Y entre les étoiles
    public float topPadding = 150f;          // marge en haut
    public float xOffset = 250f;             // décalage gauche / droite
    public float xCenterOffset = 0f;         // pour recentrer l'ensemble
    public float lineThickness = 8f;         // épaisseur des traits
    public float bottomPadding = 300f;       // marge en bas

    private List<GeneratorUI> spawnedUIs = new List<GeneratorUI>();
    private readonly List<RectTransform> spawnedLines = new List<RectTransform>();

    private void Start()
    {
        BuildList();
    }

    private void Update()
    {
        RefreshLockStates();
    }

    private void BuildList()
    {
        // Nettoyage (panels + lignes déjà présents)
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
        spawnedUIs.Clear();
        spawnedLines.Clear();

        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager.Instance est null.");
            return;
        }

        var generators = GameManager.Instance.generators;
        int count = generators.Count;

        // Hauteur totale = marge haut + espace pour chaque étoile + marge bas
        float totalHeight = topPadding + count * verticalSpacing + bottomPadding;
        Vector2 size = contentParent.sizeDelta;
        size.y = totalHeight;
        contentParent.sizeDelta = size;

        // 1) On place les panels en zigzag
        List<RectTransform> panelRects = new List<RectTransform>();

        for (int i = 0; i < count; i++)
        {
            var gen = generators[i];
            if (gen == null) continue;

            GameObject panelObj = Instantiate(generatorPanelPrefab, contentParent);
            GeneratorUI genUI = panelObj.GetComponent<GeneratorUI>();

            genUI.Init(gen);       // configure visuel + glow pour CETTE étoile
            spawnedUIs.Add(genUI);


            RectTransform panelRT = panelObj.GetComponent<RectTransform>();

            float y = -topPadding - i * verticalSpacing;
            float side = (i % 2 == 0) ? -1f : 1f;
            float x = side * xOffset + xCenterOffset;

            panelRT.anchoredPosition = new Vector2(x, y);

            panelRects.Add(panelRT);
        }

        // 2) On trace les lignes entre les panels
        BuildLines(panelRects);
    }

    private void BuildLines(List<RectTransform> panelRects)
    {
        if (panelRects.Count < 2 || lineSegmentPrefab == null)
            return;

        for (int i = 0; i < panelRects.Count - 1; i++)
        {
            RectTransform a = panelRects[i];
            RectTransform b = panelRects[i + 1];

            GameObject lineObj = Instantiate(lineSegmentPrefab, contentParent);
            RectTransform lineRT = lineObj.GetComponent<RectTransform>();

            // On part de l'anchoredPosition (top-center)
            Vector2 posA = a.anchoredPosition;
            Vector2 posB = b.anchoredPosition;

            // On descend de la moitié de la hauteur pour viser le centre du panel
            posA.y -= a.rect.height * 0.5f;
            posB.y -= b.rect.height * 0.5f;

            Vector2 mid = (posA + posB) / 2f;
            float dist = Vector2.Distance(posA, posB);
            Vector2 dir = (posB - posA).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            lineRT.anchorMin = new Vector2(0.5f, 1f);
            lineRT.anchorMax = new Vector2(0.5f, 1f);
            lineRT.pivot = new Vector2(0.5f, 0.5f);

            lineRT.anchoredPosition = mid;
            lineRT.sizeDelta = new Vector2(dist, lineThickness);
            lineRT.localRotation = Quaternion.Euler(0f, 0f, angle);

            // Met la ligne tout en bas de la pile, donc derrière les panels
            lineRT.SetAsFirstSibling();

            spawnedLines.Add(lineRT);

        }
    }


    private void RefreshLockStates()
    {
        if (GameManager.Instance == null) return;

        foreach (var genUI in spawnedUIs)
        {
            if (genUI == null || genUI.generator == null) continue;

            bool unlocked = genUI.generator.IsUnlocked();
            genUI.SetLockedState(!unlocked);
        }
    }
}
