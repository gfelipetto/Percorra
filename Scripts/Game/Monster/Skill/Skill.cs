using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using Mirror;

public class Skill : NetworkBehaviour
{
    [Header("Audio Player")]
    public SoundManager sound;

    [Header("Countdown")]
    [SerializeField] private float countdown;

    [Header("Countdown image")]
    [SerializeField] private Image countdownImage;

    [Header("Animator")]
    [SerializeField] private Animator monsterAnimator;

    [Header("Feedback when use skill")]
    [SerializeField] private GameObject skillParticle;

    private bool _skillInCountdown;
    private void Update()
    {
        if (hasAuthority)
        {
            var animationName = monsterAnimator.GetCurrentAnimatorClipInfo(0)[0].clip.name;
            if (!_skillInCountdown && animationName != "MonsterAttack" && animationName != "MonsterDying")
            {
                if (Input.GetKeyDown(KeyCode.Q))
                {
                    CmdUseSkill();
                    StartCoroutine(StartSkillCountdown());
                }
            }
        }
    }
    private IEnumerator StartSkillAreaEffect()
    {
        yield return new WaitForSeconds(1f);
        skillParticle.GetComponentInChildren<ParticleSystem>().Play();


        skillParticle.GetComponent<SphereCollider>().enabled = true;
        do
        {
            skillParticle.GetComponent<SphereCollider>().radius += .6f;
            yield return new WaitForFixedUpdate();

        } while (skillParticle.GetComponent<SphereCollider>().radius <= 15);

        skillParticle.GetComponent<SphereCollider>().radius = 1;
        skillParticle.GetComponent<SphereCollider>().enabled = false;
    }
    private IEnumerator StartSkillCountdown()
    {
        _skillInCountdown = true;
        countdownImage.enabled = true;

        var countdownToDecrease = countdown;
        do
        {
            countdownImage.fillAmount = countdownToDecrease / 10;
            countdownToDecrease -= Time.deltaTime;
            yield return new WaitForFixedUpdate();
        } while (countdownToDecrease > 0);

        countdownImage.enabled = false;
        _skillInCountdown = false;
    }

    [Command]
    private void CmdUseSkill()
    {
        RpcUseSkill();
    }
    [ClientRpc]
    private void RpcUseSkill()
    {
        sound.Play("Skill");
        monsterAnimator.SetTrigger("UseSkill");
        StartCoroutine(StartSkillAreaEffect());
    }
}
