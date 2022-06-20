using UnityEngine.UI;
using UnityEngine;
using Mirror;

public class PlayerHpManager : NetworkBehaviour
{
    public static bool isDead;

    [Header("Audio Player")]
    public SoundManager sound;

    [Header("Hp total")]
    [SerializeField] private float hp;

    [Header("Damage taken from monster")]
    [SerializeField] private float damageTaken;

    [Header("Life bar")]
    [SerializeField] private Image hpBar;

    [Header("Player animator")]
    [SerializeField] private Animator playerAnimator;

    [Header("Screen damage animator")]
    [SerializeField] private Animator screenDamageAnimator;

    [Header("Screen die animator")]
    [SerializeField] private Animator screenDieAnimator;

    private float maxHp;
    private void Start()
    {
        if (hasAuthority)
            maxHp = hp;
    }
    private void TakeDamage()
    {
        if (!isDead)
        {

            hp -= damageTaken;

            UpdateUI();
            ScreenDamageEffect();

            if (hp <= 0)
            {
                Die();
                CmdPlayerDieAnimation();
            }
            else
                CmdSoundTakeDamage();
        }
    }
    private void UpdateUI()
    {
        hpBar.fillAmount = hp / maxHp;
    }
    private void ScreenDamageEffect()
    {
        screenDamageAnimator.SetTrigger("TakeDamage");
    }
    private void Die()
    {
        CmdRemoveThisCamFromSpecCameras();
        CmdAddOneToPlayersDeadCount();

        isDead = true;
        screenDieAnimator.SetTrigger("Die");

        GetComponent<SpectatorSystem>().SetSpecCameras();
        GetComponent<Flashlight>().CmdLight(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasAuthority)
        {
            if (other.CompareTag("MonsterAttack"))
            {
                Debug.Log("Apanhou");
                TakeDamage();
            }
        }
    }
    [Command]
    private void CmdAddOneToPlayersDeadCount()
    {
        GameManager.instance.playersDead++;
    }

    [Command]
    private void CmdRemoveThisCamFromSpecCameras()
    {
        RpcRemoveThisCamFromSpecCameras();
    }
    [Command]
    private void CmdPlayerDieAnimation()
    {
        RpcPlayerDieAnimation();
    }
    [Command]
    private void CmdSoundTakeDamage()
    {
        RpcSoundTakeDamage();
    }

    [ClientRpc]
    private void RpcSoundTakeDamage()
    {
        sound.Play("Damage");
    }
    [ClientRpc]
    private void RpcPlayerDieAnimation()
    {
        sound.Play("Die");
        playerAnimator.SetTrigger("Dead");
    }
    [ClientRpc]
    private void RpcRemoveThisCamFromSpecCameras()
    {
        GetComponent<SpectatorSystem>().RemoveCamFromList(GetComponent<SpectatorSystem>().specCam.name);
    }
}
