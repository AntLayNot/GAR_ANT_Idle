using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    [Header("Références UI")]
    public GameObject panel;
    public Button resumeButton;
    public Button resetButton;
    public GameObject confirmResetPanel;
    public Button confirmYesButton;
    public Button confirmNoButton;

    private bool isPaused = false;

    private void Start()
    {
        panel.SetActive(false);
        confirmResetPanel.SetActive(false);

        resumeButton.onClick.AddListener(OnResumeClicked);
        resetButton.onClick.AddListener(OnResetClicked);

        confirmYesButton.onClick.AddListener(OnConfirmResetYes);
        confirmNoButton.onClick.AddListener(OnConfirmResetNo);
    }

    public void ToggleMenu()
    {
        bool shouldOpen = !panel.activeSelf;

        panel.SetActive(shouldOpen);

        if (shouldOpen)
            PauseGame();
        else
            ResumeGame();
    }

    private void OnResumeClicked()
    {
        ResumeGame();
        panel.SetActive(false);
    }

    private void OnResetClicked()
    {
        confirmResetPanel.SetActive(true);
    }

    private void OnConfirmResetYes()
    {
        // 1) Remet le temps à la normale
        Time.timeScale = 1f;

        // 2) Reset logique du GameManager
        if (GameManager.Instance != null)
            GameManager.Instance.ResetGame();

        // 3) Recharge la scène principale
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnConfirmResetNo()
    {
        confirmResetPanel.SetActive(false);
    }

    private void PauseGame()
    {
        Time.timeScale = 0f;
        isPaused = true;
    }

    private void ResumeGame()
    {
        Time.timeScale = 1f;
        isPaused = false;
    }
}
