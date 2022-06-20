using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine;
using System;
using Mirror;

public class NetworkManagerPercorra : NetworkManager
{
    public static event Action<NetworkConnectionToClient> OnServerReadied;

    [Header("Min number players")]
    [SerializeField] public int minPlayers;

    [Header("Loading Panel")]
    [SerializeField] private LoadingManager loadingPanel;

    [Header("Lobby prefab")]
    [SerializeField] private LobbyPlayer lobbyPlayerPrefab;

    [Header("Player spawn system")]
    [SerializeField] private GameObject playerSpawnSystem;

    public List<LobbyPlayer> LobbyPlayers { get; } = new List<LobbyPlayer>();
    public List<GamePlayer> GamePlayers { get; } = new List<GamePlayer>();

    public void StartGame()
    {
        if (CanStartGame() && SceneManager.GetActiveScene().name == "Menu")
        {
            ServerChangeScene("Game");
        }
    }
    public void StartLoadingPanel()
    {
        loadingPanel.ActiveLoadingPanel();
    }

    private bool CanStartGame()
    {
        if (numPlayers < minPlayers)
            return false;

        foreach (var player in LobbyPlayers)
        {
            if (!player.IsReady)
                return false;
        }

        return true;
    }

    public override void OnStartClient()
    {
        Debug.Log("Starting client...");

    }
    public override void OnClientConnect()
    {
        Debug.Log("Client connected.");
        base.OnClientConnect();
    }
    public override void OnClientDisconnect()
    {
        Debug.Log("Client disconnected.");
        base.OnClientDisconnect();
    }
    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        Debug.Log("On Server Connect");
        if (numPlayers >= maxConnections) // prevents players joining if the game is full
        {
            Debug.Log("Too many players. Disconnecting user.");
            conn.Disconnect();
            return;
        }
        if (SceneManager.GetActiveScene().name != "Menu") // prevents players from joining a game that has already started. When the game starts, the scene will no longer be the "TitleScreen"
        {
            Debug.Log("Player did not load from correct scene. Disconnecting user. Player loaded from scene: " + SceneManager.GetActiveScene().name);
            conn.Disconnect();
            return;
        }

        Debug.Log("Server Connected");
    }
    public override void OnServerError(NetworkConnectionToClient conn, Exception exception)
    {
        Debug.Log("Aconteceu um erro no server: " + exception);
        base.OnServerError(conn, exception);
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        try
        {
            Debug.Log("Adicionando jogadores pelo script do networkmanager");

            if (SceneManager.GetActiveScene().name == "Menu")
            {
                bool isGameLeader = LobbyPlayers.Count == 0;

                LobbyPlayer lobbyPlayerInstance = Instantiate(lobbyPlayerPrefab);

                lobbyPlayerInstance.IsGameLeader = isGameLeader;
                lobbyPlayerInstance.ConnectionId = conn.connectionId;

                NetworkServer.AddPlayerForConnection(conn, lobbyPlayerInstance.gameObject);
            }
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
            throw;

        }
    }
    public override void ServerChangeScene(string newSceneName)
    {
        try
        {
            Debug.Log("Mudando de cena");

            if (SceneManager.GetActiveScene().name == "Menu" && newSceneName == "Game")
            {
                for (int i = LobbyPlayers.Count - 1; i >= 0; i--)
                {
                    var conn = LobbyPlayers[i].connectionToClient;

                    NetworkServer.Destroy(conn.identity.gameObject);
                }

                GameObject playerSpawnSystemInstance = Instantiate(playerSpawnSystem);
                NetworkServer.Spawn(playerSpawnSystemInstance);
            }

            base.ServerChangeScene(newSceneName);

        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
            throw;
        }
    }
    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        try
        {
            if (SceneManager.GetActiveScene().name == "Menu")
            {
                print(conn.connectionId + " saiu do server na cena do menu");
                if (conn.identity != null)
                {
                    LobbyPlayer player = conn.identity.GetComponent<LobbyPlayer>();
                    LobbyPlayers.Remove(player);
                }
            }
            base.OnServerDisconnect(conn);
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
            throw;
        }
    }
    public override void OnStopServer()
    {
        try
        {
            Debug.Log("Limpou o lobby");
            LobbyPlayers.Clear();
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
            throw;
        }
    }
    public override void OnServerReady(NetworkConnectionToClient conn)
    {
        try
        {
            if (SceneManager.GetActiveScene().name == "Game")
            {

                print("Cliente pronto no servidor: " + conn.connectionId);
                OnServerReadied?.Invoke(conn);
                base.OnServerReady(conn);
            }

        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
            throw;
        }
    }
}
