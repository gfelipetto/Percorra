using UnityEngine;
using Mirror;

public class SecundaryLever : NetworkBehaviour
{
    [SyncVar(hook = nameof(HandleActiveLever))] public bool isActive;

    [Header("Animator")]
    public Animator leverAnimator;

    [Header("Primary lever")]
    public PrimaryLever primaryLever;

    public bool canActive;

    public bool CheckIfCanActiveLever()
    {
        if (canActive)
        {
            Debug.Log("CanActice true");
            if (leverAnimator.GetCurrentAnimatorClipInfo(0)[0].clip.name != "IsActivate")
            {
                Debug.Log("Animação não é a ativa");
                return true;
            }
        }
        return false;
    }

    private void HandleActiveLever(bool oldValue, bool newValue)
    {
        Debug.Log("Chamou o hook da alanva secundária");
        leverAnimator.SetTrigger("Activate");
    }

    [Command(requiresAuthority = false)]
    public void CmdAddOneToSecundaryLeverActivadeCount()
    {
        primaryLever.quantityOfSecundaryLeverActivade++;
    }
}
