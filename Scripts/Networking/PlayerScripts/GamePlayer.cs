using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class GamePlayer : NetworkBehaviour
{
    [Header("Animator of all dead players feedback")]
    [SerializeField] private Animator whenPlayersDeadAnimator;
    [SerializeField] private Animator whenPlayersExitedAnimator;

    public static bool escaped;

    [SyncVar] public string PlayerName;
    [SyncVar] public int ConnectionId;

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
    public override void OnStartLocalPlayer()
    {
        try
        {
            gameObject.name = "LocalGamePlayer";
            base.OnStartLocalPlayer();
        }
        catch (System.Exception)
        {

            throw;
        }
    }
    public override void OnStartClient()
    {
        try
        {
            DontDestroyOnLoad(gameObject);
            Game.GamePlayers.Add(this);
        }
        catch (System.Exception)
        {

            throw;
        }
    }
    public override void OnStopServer()
    {
        Debug.Log(PlayerName + " Esta saindo do jogo pelo on stop server do gameplayer");
        Game.GamePlayers.Remove(this);

        base.OnStopServer();
    }
    public override void OnStopClient()
    {
        try
        {
            Debug.Log(PlayerName + " Esta saindo do jogo pelo on stop client do gameplayer.");
            if (hasAuthority)
            {
                CmdResquestCheckToAllClientsWhenOneQuit();
            }
            Game.GamePlayers.Remove(this);
            Game.ServerChangeScene("Menu");
        }
        catch (System.Exception ex)
        {
            Debug.Log(ex.Message);
            throw;
        }
    }

    public void QuitGame()
    {
        if (hasAuthority)
        {
            NetworkServer.Destroy(FindObjectOfType<PlayerSpawnSystem>().gameObject);

            if (isServer)
            {
                Debug.Log("Parou o host");
                Game.StopHost();
            }
            else
            {
                Debug.Log("Parou o client");
                Game.StopClient();
            }

        }
    }
    public void FinishGameWhenHavePlayersDefeat()
    {
        if (hasAuthority)
        {
            if (!escaped)
            {
                whenPlayersDeadAnimator.SetTrigger("Start");
            }
            else
            {
                whenPlayersExitedAnimator.SetTrigger("Start");
            }
        }
    }
    public void FinishGameWhenAllPlayersExited()
    {
        if (hasAuthority)
        {
            whenPlayersExitedAnimator.SetTrigger("Start");
        }
    }
    public void SetEscaped(bool state)
    {
        escaped = state;
    }

    [Command]
    private void CmdResquestCheckToAllClientsWhenOneQuit()
    {
        RpcResquestCheckToAllClientsWhenOneQuit();
    }

    [ClientRpc]
    private void RpcResquestCheckToAllClientsWhenOneQuit()
    {
        GameManager.instance.CheckGamePlayerFinishedCount();
    }

    [Server]
    public void SetPlayerName(string playerName)
    {
        this.PlayerName = playerName;
    }
    [Server]
    public void SetConnectionId(int connId)
    {
        this.ConnectionId = connId;
    }
}
