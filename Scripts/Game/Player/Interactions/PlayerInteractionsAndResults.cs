using UnityEngine.UI;
using UnityEngine;
using Mirror;

public class PlayerInteractionsAndResults : NetworkBehaviour
{
    [SyncVar(hook = nameof(HandleOpenStorageDoor))] private bool isOpenDoor;
    public static bool escaped;

    [Header("Audio Player")]
    public SoundManager sound;

    [Header("Distance raycast")]
    [SerializeField] private float raycastDistance;

    [Header("Raycast origin")]
    [SerializeField] private GameObject raycastOrigin;

    [Header("Feedback message")]
    [SerializeField] private Text feedbackText;

    [Header("Menu inside game")]
    [SerializeField] private GameObject menuInsideGame;

    [Header("Final place transform")]
    [SerializeField] private Transform finalPlaceTransform;

    private bool hasKey;
    private Flashlight _flashlight;

    private void Start()
    {
        if (hasAuthority)
        {
            _flashlight = this.GetComponent<Flashlight>();
        }
    }
    private void Update()
    {
        if (hasAuthority)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                GameManager.instance.ChangeGameMenu(menuInsideGame);
            }

            if (!PlayerHpManager.isDead)
            {
                RaycastHit hit;
                if (Physics.Raycast(raycastOrigin.transform.position, raycastOrigin.transform.TransformDirection(Vector3.forward), out hit, raycastDistance))
                {
                    Debug.DrawRay(raycastOrigin.transform.position, raycastOrigin.transform.TransformDirection(Vector3.forward) * hit.distance, Color.green);
                    ChangeFeedbackText(hit);


                    if (Input.GetMouseButtonDown(0))
                    {
                        DoActionsByTagHit(hit);
                    }
                }
                else
                {
                    feedbackText.text = "";
                }
            }
        }
    }

    private void DoActionsByTagHit(RaycastHit hit)
    {
        switch (hit.transform.tag)
        {
            case "Battery":
                GetBattery(hit);
                break;

            case "PrimaryLever":
                CmdActivePrimaryLever(hit.transform.GetComponent<PrimaryLever>());
                break;

            case "SecundaryLever":
                CmdActiveSecundaryLever(hit.transform.GetComponent<SecundaryLever>());
                break;

            case "Pc":
                CmdAtiveCountdownPc(hit.transform.parent.GetComponent<Pc>());
                break;

            case "Key":
                GetKey(hit);
                break;

            case "StorageDoor":
                if (hasKey)
                    CmdSetIsOpenDoorToTrue();
                else
                    CmdPlaySong("Wrong");

                break;

            case "ExitDoor":
                if (GameManager.instance.finalDoorAreOpen)
                    ClickInExitDoorWhenIsOpen();
                break;

            default:
                break;
        }
    }
    private void ChangeFeedbackText(RaycastHit hit)
    {
        switch (hit.transform.tag)
        {
            case "Battery":
                feedbackText.text = "Pegar pilha";
                break;

            case "PrimaryLever":
                if (!hit.transform.GetComponent<PrimaryLever>().canActive)
                    feedbackText.text = "Acione três alavancas";
                else
                {
                    if (hit.transform.GetComponent<Animator>().GetCurrentAnimatorClipInfo(0)[0].clip.name != "IsActivate")
                        feedbackText.text = "Ativar alavanca";
                    else
                        feedbackText.text = "Computador ligado";
                }
                break;

            case "SecundaryLever":
                if (!hit.transform.GetComponent<SecundaryLever>().canActive)
                    feedbackText.text = "Alavanca bloqueada";
                else
                {
                    if (hit.transform.GetComponent<Animator>().GetCurrentAnimatorClipInfo(0)[0].clip.name != "IsActivate")
                        feedbackText.text = "Ativar alavanca";
                    else
                        feedbackText.text = "Alavanca ativada";
                }
                break;

            case "Pc":
                if (!hit.transform.parent.GetComponent<Pc>().CheckIfHasEnergy())
                    feedbackText.text = "Computador desligado";
                else
                {
                    if (!hit.transform.parent.GetComponent<Pc>().CheckIfIsInCountdown())
                        feedbackText.text = "Iniciar reinicialização";
                    else
                        feedbackText.text = "Reinicialização iniciada";
                }

                break;

            case "Key":
                feedbackText.text = "Pegar chave";
                break;

            case "StorageDoor":
                if (hit.transform.GetComponent<Animator>().GetCurrentAnimatorClipInfo(0)[0].clip.name != "OpenDoor")
                {
                    if (!hasKey)
                        feedbackText.text = "Precisa de chave";
                    else
                        feedbackText.text = "Abrir porta";
                }
                break;

            case "ExitDoor":
                if (GameManager.instance.finalDoorAreOpen)
                    feedbackText.text = "Sair";
                else
                    feedbackText.text = "Porta bloqueada";
                break;

            default:
                feedbackText.text = "";
                break;
        }
    }
    private void GetKey(RaycastHit hit)
    {
        hasKey = true;
        CmdDestroyObj(hit.transform.gameObject);
    }

    private void GetBattery(RaycastHit hit)
    {
        _flashlight.ChangeBatteryEnergy(hit.transform.GetComponent<Battery>().valueEnergy);
        CmdDestroyObj(hit.transform.gameObject);
    }
    private void ClickInExitDoorWhenIsOpen()
    {
        GamePlayer.escaped = true;
        this.gameObject.transform.position = finalPlaceTransform.position;

        CmdAddOneToPlayersExitedCount();
    }

    private void HandleOpenStorageDoor(bool oldValue, bool newValue)
    {
        var doors = GameObject.FindGameObjectsWithTag("StorageDoor");
        foreach (var door in doors)
        {
            door.GetComponent<Animator>().SetTrigger("Open");
        }

        var secundaryLevers = GameObject.FindGameObjectsWithTag("SecundaryLever");
        foreach (var lever in secundaryLevers)
        {
            lever.GetComponent<SecundaryLever>().canActive = true;
        }
    }

    [Command]
    private void CmdDestroyObj(GameObject obj)
    {
        RpcPlaySong("Collect");
        Destroy(obj);
    }
    [Command]
    private void CmdSetIsOpenDoorToTrue()
    {
        if (!isOpenDoor)
        {
            RpcPlaySong("Correct");
            isOpenDoor = true;
        }
        else
        {
            RpcPlaySong("Wrong");
        }
    }
    [Command]
    private void CmdActiveSecundaryLever(SecundaryLever lever)
    {
        if (lever.CheckIfCanActiveLever())
        {
            RpcPlaySong("Correct");
            lever.isActive = true;
            lever.CmdAddOneToSecundaryLeverActivadeCount();
        }
        else
        {
            RpcPlaySong("Wrong");
        }
    }
    [Command]
    private void CmdActivePrimaryLever(PrimaryLever lever)
    {
        if (lever.CheckIfCanActiveLever())
        {
            RpcPlaySong("Correct");
            lever.isActive = true;
        }
        else
        {
            RpcPlaySong("Wrong");
        }
    }
    [Command]
    private void CmdAtiveCountdownPc(Pc pc)
    {
        if (pc.CheckIfHasEnergy() && !pc.CheckIfIsInCountdown())
        {
            RpcPlaySong("Correct");
            pc.startCountdown = true;
        }
        else
        {
            RpcPlaySong("Wrong");
        }
    }
    [Command]
    private void CmdAddOneToPlayersExitedCount()
    {
        GameManager.instance.playersExited++;
    }

    [Command]
    private void CmdPlaySong(string soundName)
    {
        RpcPlaySong(soundName);
    }
    [ClientRpc]
    private void RpcPlaySong(string soundName)
    {
        sound.Play(soundName);
    }
}
