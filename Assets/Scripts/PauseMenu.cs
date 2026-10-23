using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static PauseMenu Instance;

    [Header("설정 패널과 버튼")]
    public GameObject settingsPanel;
    public Button closeButton;
    public Button quitButton;

    [Header("게임 방법")]
    public GameObject howToPanel;
    public Button howToButton;
    public Button howToCloseButton;

    [Header("ESC가 동작하지 않을 씬")]
    public string titleSceneName = "TitleScene";

    private bool isOpen = false;

    void Awake()
    {
        // 씬을 옮겨도 하나만 남기기
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (howToPanel != null) howToPanel.SetActive(false);

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(ClosePanel);
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveAllListeners();
            quitButton.onClick.AddListener(QuitGame);
        }

        if (howToButton != null)
        {
            howToButton.onClick.RemoveAllListeners();
            howToButton.onClick.AddListener(OpenHowTo);
        }

        if (howToCloseButton != null)
        {
            howToCloseButton.onClick.RemoveAllListeners();
            howToCloseButton.onClick.AddListener(CloseHowTo);
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    void Update()
    {
        if (SceneManager.GetActiveScene().name == titleSceneName) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // 게임 방법 창이 열려 있으면 그것부터 닫기
            if (howToPanel != null && howToPanel.activeSelf)
            {
                CloseHowTo();
            }
            else if (isOpen)
            {
                ClosePanel();
            }
            else
            {
                OpenPanel();
            }
        }
    }

    // 씬이 바뀌면 설정창을 닫고 게임 진행 상태로 되돌리기
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ClosePanel();
    }

    public void OpenPanel()
    {
        isOpen = true;
        if (settingsPanel != null) settingsPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void ClosePanel()
    {
        isOpen = false;
        if (howToPanel != null) howToPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void OpenHowTo()
    {
        if (howToPanel != null) howToPanel.SetActive(true);
    }

    public void CloseHowTo()
    {
        if (howToPanel != null) howToPanel.SetActive(false);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        Debug.Log("게임 종료");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}