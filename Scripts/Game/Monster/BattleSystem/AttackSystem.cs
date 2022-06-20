using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using Mirror;

public class AttackSystem : NetworkBehaviour
{
    [Header("Audio Player")]
    public SoundManager sound;

    [Header("Animator")]
    [SerializeField] private Animator monsterAnimator;
    [SerializeField] private Animator panelDieAnimator;

    [Header("Canvas monster")]
    [SerializeField] private GameObject monsterCanvas;

    [Header("Life bar")]
    [SerializeField] private Image lifeBar;

    [Header("Feedback when attacked")]
    [SerializeField] private Image blurFlashlightEffect;

    [Header("Life")]
    [SerializeField] private float currentLife;

    [Header("Damage taken from ligth")]
    [SerializeField] private float lightDamage;

    private float maxLife;
    private bool dead;

    private void Start()
    {
        if (hasAuthority)
        {
            monsterCanvas.SetActive(true);
            maxLife = currentLife;
        }
        else
            monsterCanvas.SetActive(false);
    }
    private void Update()
    {
        if (hasAuthority)
        {
            string animationName = monsterAnimator.GetCurrentAnimatorClipInfo(0)[0].clip.name;
            if (animationName != "MonsterUsingSkill" && animationName != "MonsterAttack" && animationName != "MonsterDying")
            {
                if (Application.isFocused)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        CmdChangeAnimationAttackSystem("Attack");
                        CmdPlaySounds("Attack");
                    }
                }
            }
        }
    }

    private void TakeDamage(float damage)
    {
        if (!dead)
        {
            currentLife -= damage;
            UpdateUI(currentLife);

            if (currentLife <= 0)
            {
                CmdPlaySounds("Die");

                dead = true;
                StartCoroutine(MonsterReborn());
                Debug.Log("Morreu");
            }
            else
            {
                if (!sound.IsPlaying("Damage"))
                {
                    CmdPlaySounds("Damage");
                }
            }
        }
    }
    private void UpdateUI(float value)
    {
        lifeBar.fillAmount = value / maxLife;
    }
    private void TeleportMonster()
    {
        var local = PlayerSpawnSystem.spawnMonsterPoints[Random.Range(0, PlayerSpawnSystem.spawnMonsterPoints.Count)];
        this.gameObject.transform.position = local.position;
        this.gameObject.transform.rotation = local.rotation;
    }

    private IEnumerator MonsterReborn()
    {
        CmdChangeAnimationAttackSystem("Dead");
        panelDieAnimator.SetTrigger("Dead");

        yield return new WaitForSeconds(monsterAnimator.GetCurrentAnimatorClipInfo(0)[0].clip.length);

        TeleportMonster();
        UpdateUI(maxLife);


        CmdChangeAnimationAttackSystem("Reborn");
        panelDieAnimator.SetTrigger("Reborn");

        currentLife = maxLife;
        dead = false;

        Debug.Log("Renasceu");
    }

    private void OnTriggerStay(Collider other)
    {
        if (hasAuthority)
        {
            if (other.CompareTag("Flashlight"))
            {
                blurFlashlightEffect.enabled = true;
                TakeDamage(lightDamage);
            }
            else
            {

                blurFlashlightEffect.enabled = false;
            }
        }
    }

    [Command]
    private void CmdChangeAnimationAttackSystem(string animatiomName)
    {
        RpcChangeAnimationAttackSystem(animatiomName);
    }
    [Command]
    private void CmdPlaySounds(string soundName)
    {
        RpcPlaySounds(soundName);
    }

    [ClientRpc]
    private void RpcChangeAnimationAttackSystem(string animatiomName)
    {
        monsterAnimator.SetTrigger(animatiomName);
    }
    [ClientRpc]
    private void RpcPlaySounds(string soundName)
    {
        sound.Play(soundName);
    }
}
