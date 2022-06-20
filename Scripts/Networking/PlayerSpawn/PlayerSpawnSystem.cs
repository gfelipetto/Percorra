using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Mirror;

public class PlayerSpawnSystem : NetworkBehaviour
{
    [SerializeField] private GameObject[] playersPrefab = null;

    public static List<Transform> spawnMonsterPoints = new List<Transform>();
    private static List<Transform> spawnPlayerPoints = new List<Transform>();
    public static List<Camera> playersSpecCameras = new List<Camera>();

    private readonly List<int> playerPrefabIndexList = new List<int>();

    [SyncVar] private int nextIndexSpawnPoints = 0;

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

    private void Start()
    {
        Game.StartLoadingPanel();
        DontDestroyOnLoad(gameObject);
    }

    public static void AddSpawnPoint(Transform transform, bool isSpawnMonster)
    {
        try
        {
            if (isSpawnMonster)
            {
                spawnMonsterPoints.Add(transform);
                spawnMonsterPoints = spawnMonsterPoints.OrderBy(s => s.GetSiblingIndex()).ToList();
            }
            else
            {
                spawnPlayerPoints.Add(transform);
                spawnPlayerPoints = spawnPlayerPoints.OrderBy(s => s.GetSiblingIndex()).ToList();
            }
        }
        catch (System.Exception ex)
        {
            Debug.Log(ex.Message);
            throw;
        }
    }
    public static void RemoveSpawnPoint(Transform transform, bool isSpawnMonster)
    {
        try
        {
            if (isSpawnMonster)
                spawnMonsterPoints.Remove(transform);
            else
                spawnPlayerPoints.Remove(transform);
        }
        catch (System.Exception ex)
        {
            Debug.Log(ex.Message);
            throw;
        }
    }

    public override void OnStartServer()
    {
        try
        {
            Debug.Log("On Start Server do PlayerSpawn");
            NetworkManagerPercorra.OnServerReadied += SpawnPlayer;
        }
        catch (System.Exception ex)
        {
            Debug.Log(ex.Message);
            throw;
        }
    }
    public override void OnStopServer()
    {
        Debug.Log("On stop Server do playerSpawn");
        base.OnStopServer();
    }

    private void ReturnAndIncrementIndexSpawnPoints()
    {
        nextIndexSpawnPoints++;
    }
    private int ReturnNewIndexPlayerPrefab()
    {
        int index;
        bool contains = true;

        do
        {
            index = Random.Range(0, playersPrefab.Length);

            if (!playerPrefabIndexList.Contains(index))
            {
                playerPrefabIndexList.Add(index);
                contains = false;
            }
        } while (contains);

        return index;
    }
    private int ReturnNewIndexMonsterSpawnPoint()
    {
        return Random.Range(0, spawnMonsterPoints.Count);
    }

    [ServerCallback]
    private void OnDestroy()
    {
        try
        {
            Debug.Log("OnDestroy do playerSpawn");
            NetworkManagerPercorra.OnServerReadied -= SpawnPlayer;
        }
        catch (System.Exception ex)
        {
            Debug.Log(ex.Message);
            throw;
        }
    }

    [Server]
    public void SpawnPlayer(NetworkConnectionToClient conn)
    {
        try
        {
            Debug.Log("Spawn Player");

            var playerPrefab = playersPrefab[ReturnNewIndexPlayerPrefab()];
            GameObject playerInstance;

            if (playerPrefab.transform.CompareTag("Player"))
            {
                playerInstance = Instantiate(playerPrefab,
                    spawnPlayerPoints[nextIndexSpawnPoints].position,
                    spawnPlayerPoints[nextIndexSpawnPoints].rotation);
                playersSpecCameras.Add(playerInstance.GetComponent<SpectatorSystem>().specCam);

                ReturnAndIncrementIndexSpawnPoints();
            }
            else
            {
                playerInstance = Instantiate(playerPrefab,
                    spawnMonsterPoints[ReturnNewIndexMonsterSpawnPoint()].position,
                    spawnMonsterPoints[ReturnNewIndexMonsterSpawnPoint()].rotation);
                playerInstance.GetComponent<GamePlayer>().SetEscaped(true);
            }

            playerInstance.GetComponent<GamePlayer>().SetPlayerName(PlayerPrefs.GetString("PlayerName"));
            playerInstance.GetComponent<GamePlayer>().SetConnectionId(conn.connectionId);

            NetworkServer.ReplacePlayerForConnection(conn, playerInstance, true);
        }
        catch (System.Exception ex)
        {
            Debug.Log(ex.Message);
            throw;
        }
    }
}
