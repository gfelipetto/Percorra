using UnityEngine;
using Mirror;

public class PrimaryLever : NetworkBehaviour
{
    [SyncVar(hook = nameof(HandleCheckSecundaryLeversCount))] public int quantityOfSecundaryLeverActivade = 0;
    [SyncVar(hook = nameof(HandleActiveLever))] public bool isActive;

    [Header("Animator")]
    public Animator leverAnimator;

    [Header("Pc")]
    public Pc pcScript;

    public bool canActive = false;
    public bool CheckIfCanActiveLever()
    {
        if (canActive)
        {
            Debug.Log("CanActice true");
            if (leverAnimator.GetCurrentAnimatorClipInfo(0)[0].clip.name != "IsActivate")
            {
                return true;
            }
        }
        return false;
    }

    private void HandleCheckSecundaryLeversCount(int oldValue, int newValue)
    {
        Debug.Log("Chamou o hook da alavanca primária");
        if (quantityOfSecundaryLeverActivade >= 3)
        {
            canActive = true;
        }
    }
    private void HandleActiveLever(bool oldValue, bool newValue)
    {
        leverAnimator.SetTrigger("Activate");
        pcScript.turnOnPc = true;
        Debug.Log("Ativou animação da alavanca primária");
    }
}
