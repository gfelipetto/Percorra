using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;
using System.Linq;
using Mirror;

public class LobbyPlayer : NetworkBehaviour
{
    public const string PlayerPrefsNameKey = "PlayerName";

    [Header("UI")]
    [SerializeField] private GameObject playerLobyUI;
    [SerializeField] private GameObject[] playerPanels;
    [SerializeField] private GameObject startGameButton;
    [SerializeField] private Button readyButton;
    [SerializeField] private Button quitButton;

    [SyncVar(hook = nameof(HandlePlayerNameUpdate))] public string PlayerName;
    [SyncVar(hook = nameof(HandlePlayerReadyStatusUpdate))] public bool IsReady = false;
    [SyncVar] public int ConnectionId;

    [Header("Game Info")]
    public bool IsGameLeader = false;

    private NetworkManagerPercorra game;
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

    public override void OnStartClient()
    {
        if (isServer)
        {
            Debug.Log("Iniciou o cliente como server");
        }

        Debug.Log("Inicou o cliente");

        Game.LobbyPlayers.Add(this);
        base.OnStartClient();
    }
    public override void OnStopClient()
    {
        Debug.Log(PlayerName + " está saindo do jogo pelo script do lobby.");
        Game.LobbyPlayers.Remove(this);
    }
    public override void OnStartLocalPlayer()
    {
        UIScreenManager.instance.GetHostOrJoinPanel().SetActive(false);

        if (!playerLobyUI.activeInHierarchy)
            playerLobyUI.SetActive(true);

        gameObject.name = "LocalLobbyPlayer";

        CmdSetPlayerName(PlayerPrefs.GetString(PlayerPrefsNameKey));

        base.OnStartLocalPlayer();
    }
    public override void OnStopLocalPlayer()
    {
        Debug.Log("Parou pelo local player do lobby Player");
        CheckCurrentSceneAndReturnToMenuCoroutine();

        base.OnStopLocalPlayer();
    }
    public void UpdateLobbyUI()
    {
        CheckIfAllPlayersAreReady();

        GameObject localPlayer = GameObject.Find("LocalLobbyPlayer");
        if (localPlayer != null)
        {
            localPlayer.GetComponent<LobbyPlayer>().ActivateLobbyUI();
        }
    }
    public void ActivateLobbyUI()
    {
        if (!playerLobyUI.activeInHierarchy)
            playerLobyUI.SetActive(true);

        readyButton.gameObject.SetActive(true);

        quitButton.gameObject.SetActive(true);

        for (int i = 0; i < Game.LobbyPlayers.Count(); i++)
        {
            playerPanels[i].SetActive(true);

            UpdatePlayerReadyText(playerPanels[i], i);
        }
    }
    public void UpdatePlayerReadyText(GameObject panel, int index)
    {
        if (panel.activeInHierarchy)
        {
            foreach (Transform childText in panel.transform)
            {
                if (childText.name == "PlayerName")
                    childText.GetComponent<Text>().text = Game.LobbyPlayers[index].PlayerName;

                if (childText.name == "PlayerReady")
                {
                    childText.GetComponent<Text>().text = Game.LobbyPlayers[index].IsReady ? "Pronto" : "Não pronto";
                    childText.GetComponent<Text>().color = Game.LobbyPlayers[index].IsReady ? Color.green : Color.red;
                }
            }
        }

        readyButton.GetComponentInChildren<Text>().text = IsReady ? "Não pronto" : "Pronto";
    }

    public void HandlePlayerNameUpdate(string oldValue, string newValue)
    {
        UpdateLobbyUI();
    }
    public void HandlePlayerReadyStatusUpdate(bool oldValue, bool newValue)
    {
        UpdateLobbyUI();
    }

    public void CheckIfAllPlayersAreReady()
    {
        bool arePlayersReady = true;

        foreach (var player in Game.LobbyPlayers)
        {
            if (!player.IsReady)
            {
                arePlayersReady = false;
                break;
            }
        }

        if (arePlayersReady)
        {
            if (IsGameLeader && Game.LobbyPlayers.Count >= Game.minPlayers)
                startGameButton.SetActive(true);
        }
        else
            startGameButton.SetActive(false);
    }
    public void QuitLobby()
    {
        if (hasAuthority)
        {
            if (IsGameLeader)
                Game.StopHost();
            else
                Game.StopClient();
        }
    }


    public void CheckCurrentSceneAndReturnToMenuCoroutine()
    {
        try
        {
            if (SceneManager.GetActiveScene().name == "Menu")
            {
                Debug.Log("Está no menu e retornara para o início");
                UIScreenManager.instance.ReturnFromLobbyToMainMenu();
            }
        }
        catch (System.Exception ex)
        {
            Debug.Log(ex.Message);
            throw;
        }
    }

    [Command]
    public void CmdReadyUp()
    {
        IsReady = !IsReady;
    }
    [Command]
    private void CmdSetPlayerName(string playerName)
    {
        PlayerName = playerName;
    }
    [Command]
    public void CmdStartGame()
    {
        Game.StartGame();
    }
}
