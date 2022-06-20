using System.Collections;
using UnityEngine;
using TMPro;

public class UIScreenManager : MonoBehaviour
{
    private const string PlayerPrefsNameKey = "PlayerName";

    public static UIScreenManager instance;

    [Header("NetworkManager prefab")]
    [SerializeField] private GameObject networkManagerPrefab;

    [Header("InputFields")]
    [SerializeField] private TMP_InputField namePlayerInputField;
    [SerializeField] private TMP_InputField ipAddressInputField;

    [Header("UI Painels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject playerNamePanel;
    [SerializeField] private GameObject hostOrJoinPanel;
    [SerializeField] private GameObject creditsPanel;

    [Header("Camera Animator")]
    [SerializeField] private Animator camAnimator;

    private static NetworkManagerPercorra game;
    private NetworkManagerPercorra Game
    {
        get
        {
            if (game != null)
            {
                return game;
            }

            return game = NetworkManagerPercorra.singleton as NetworkManagerPercorra;
        }
    }

    private void Awake()
    {
        if (instance == null)
            instance = this;

        Cursor.lockState = CursorLockMode.None;
    }

    public void StartHost()
    {
        hostOrJoinPanel.SetActive(false);
        Game.StartHost();
    }
    public void StartClient()
    {
        if (string.IsNullOrEmpty(ipAddressInputField.text))
            return;

        Game.networkAddress = ipAddressInputField.text;
        Game.StartClient();
    }

    public void SavePlayerNameAndChangePainel()
    {
        if (string.IsNullOrEmpty(namePlayerInputField.text)) return;

        PlayerPrefs.SetString(PlayerPrefsNameKey, namePlayerInputField.text);
        playerNamePanel.SetActive(false);

        StartCoroutine(GoToHostOrJoinPanel());
    }
    public void GoFromMenuToCredits()
    {
        StartCoroutine(GoToCredits());
    }

    public void ReturnFromLobbyToMainMenu()
    {
        hostOrJoinPanel.SetActive(false);
        playerNamePanel.SetActive(false);
        StartCoroutine(ReturnFromPanelToMenuCoroutine("FromHostOrClientToMenu"));
    }
    public void ReturnFromCreditsToMenu()
    {
        creditsPanel.SetActive(false);
        StartCoroutine(ReturnFromPanelToMenuCoroutine("FromCreditsToMenu"));
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public GameObject GetHostOrJoinPanel() => hostOrJoinPanel;

    private IEnumerator GoToHostOrJoinPanel()
    {
        yield return new WaitForSeconds(ReturnTimeAndSetAnimationCam("FromMenuToHostOrClient"));
        hostOrJoinPanel.SetActive(true);
    }
    private IEnumerator GoToCredits()
    {
        mainMenuPanel.SetActive(false);
        yield return new WaitForSeconds(ReturnTimeAndSetAnimationCam("FromMenuToCredits"));
        creditsPanel.SetActive(true);
    }

    private IEnumerator ReturnFromPanelToMenuCoroutine(string panelName)
    {
        yield return new WaitForSeconds(ReturnTimeAndSetAnimationCam(panelName));
        mainMenuPanel.SetActive(true);
    }

    private float ReturnTimeAndSetAnimationCam(string animationName)
    {
        camAnimator.SetTrigger(animationName);
        return camAnimator.GetCurrentAnimatorStateInfo(0).length + 0.5f;
    }
}
