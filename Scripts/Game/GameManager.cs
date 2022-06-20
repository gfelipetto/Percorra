using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Mirror;

public class GameManager : NetworkBehaviour
{
    [SyncVar(hook = nameof(HandleSetKeyActiveInList))] public int indexKeyList;
    [SyncVar(hook = nameof(HandleSetLeverActiveInList))] public List<int> indexLeverList;

    [SyncVar(hook = nameof(HandleCheckPlayersDead))] public int playersDead = 0;
    [SyncVar(hook = nameof(HandleCheckPlayersEscaped))] public int playersExited = 0;


    public static GameManager instance;

    [Header("List key objects")]
    [SerializeField] private List<GameObject> keys;

    [Header("List lever objects")]
    [SerializeField] private List<GameObject> levers;

    [Header("Final Lights obj")]
    [SerializeField] private GameObject finalLights;

    [Header("Final door open")]
    public bool finalDoorAreOpen;

    public bool gameMenuIsOpened;

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

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }
    private void Start()
    {
        if (isServer)
            CmdSetupScene();
    }

    public void ActiveFinalLights()
    {
        finalDoorAreOpen = true;
        StartCoroutine(StartFinalLightEffects());
    }
    public void ChangeGameMenu(GameObject menu)
    {
        bool visible = !gameMenuIsOpened;

        menu.SetActive(visible);
        gameMenuIsOpened = visible;
        Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
    }

    public void CheckGamePlayerFinishedCount()
    {
        var playersFinished = playersDead + playersExited;
        if (playersFinished >= Game.GamePlayers.Count - 1)
        {
            if (playersExited >= Game.GamePlayers.Count - 1)
            {
                foreach (var player in Game.GamePlayers)
                {
                    player.GetComponent<GamePlayer>().FinishGameWhenAllPlayersExited();
                }
            }
            else
            {
                foreach (var player in Game.GamePlayers)
                {
                    player.GetComponent<GamePlayer>().FinishGameWhenHavePlayersDefeat();
                }
            }
        }
    }

    private void HandleSetKeyActiveInList(int oldValue, int newValue)
    {
        foreach (var key in keys)
        {
            key.SetActive(false);
        }

        keys[indexKeyList].SetActive(true);
    }
    private void HandleSetLeverActiveInList(List<int> oldValue, List<int> newValue)
    {
        foreach (var lever in levers)
        {
            lever.SetActive(false);
        }

        foreach (var index in indexLeverList)
        {
            levers[index].SetActive(true);
        }
    }
    private void HandleCheckPlayersDead(int oldValue, int newValue)
    {
        Debug.Log("Chamou o hook do jogador morreu");

        CheckGamePlayerFinishedCount();
    }
    private void HandleCheckPlayersEscaped(int oldValue, int newValue)
    {
        Debug.Log("Chamou o hook do jogador escapou");

        CheckGamePlayerFinishedCount();
    }

    [Command(requiresAuthority = false)]
    private void CmdSetupScene()
    {
        indexKeyList = Random.Range(0, keys.Count);
        indexLeverList = RandomIndexOfLeverList();
    }

    private List<int> RandomIndexOfLeverList()
    {
        List<int> indexList = new List<int>();
        int value;
        var count = 0;

        do
        {
            value = Random.Range(0, levers.Count);
            if (!indexList.Contains(value))
            {
                indexList.Add(value);
                count++;
            }
        } while (count < 3);

        return indexList;
    }
    private IEnumerator StartFinalLightEffects()
    {
        bool on = true;
        do
        {
            yield return new WaitForSeconds(1f);
            finalLights.SetActive(on);
            on = !on;

        } while (true);
    }
}
