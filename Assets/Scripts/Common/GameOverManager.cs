using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Runtime.Script;

// Gerencia a interface de "Game Over".
// Anexe este componente a um GameObject na cena e atribua o painel de Game Over
// (gameOverPanel). Chame ShowGameOver() quando o jogador morrer.
public class GameOverManager : MonoBehaviour
{
    // Instância singleton para fácil acesso
    public static GameOverManager Instance { get; private set; }

    [Tooltip("Painel que contém a UI de Game Over. Deve estar desativado no início.")]
    public GameObject gameOverPanel;

    [Tooltip("Texto opcional do título (ex.: 'Game Over')")]
    public TextMeshProUGUI titleText;

    [Tooltip("Texto opcional para mostrar pontuação final ou mensagem")]
    public TextMeshProUGUI messageText;

    [Tooltip("Índice da cena do menu principal (usado por GoToMainMenu)")]
    public int mainMenuSceneIndex = 0;

    void Awake()
    {
        // Inicializa singleton, destrói duplicatas
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        // Assegura que o painel de Game Over comece invisível
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    // Exibe a tela de Game Over. message pode ser nulo.
    public void ShowGameOver(string title = "Game Over", string message = null)
    {
        // Atualiza textos e exibe o painel
        if (gameOverPanel != null)
        {
            if (titleText != null) titleText.text = title;
            if (messageText != null) messageText.text = message ?? string.Empty;
            gameOverPanel.SetActive(true);
        }

        // Pausa o jogo e libera o cursor para interação
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Tenta desabilitar controladores comuns do jogador para parar entrada/câmera
        var p3 = FindObjectOfType<PlayerThird>();
        if (p3 != null) p3.enabled = false;

        var fpChar = FindObjectOfType<PlayerCharacter>() as MonoBehaviour;
        if (fpChar != null) fpChar.enabled = false;

        var fp = FindObjectOfType<Player>() as MonoBehaviour;
        if (fp != null) fp.enabled = false;
    }

    // Callbacks dos botões da UI
    public void Retry()
    {
        // Retoma o tempo e recarrega a cena atual
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMainMenu()
    {
        // Retoma o tempo e carrega a cena do menu
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneIndex);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        // No Editor, para a execução
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // Em build, fecha a aplicação
        Application.Quit();
#endif
    }
}
