using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using Mirror;

public class Flashlight : NetworkBehaviour
{
    [Header("Audio Player")]
    public SoundManager sound;

    [Header("Light")]
    [SerializeField] private GameObject _light;

    [Header("Canvas light feedback")]
    [SerializeField] private GameObject canvas;

    [Header("Light bar image")]
    [SerializeField] private Image lightBar;

    [Header("Value to decrease battery")]
    [SerializeField] private float quantityBatteryToSpend;

    [Header("Current battery energy")]
    [SerializeField] private float currentBatteryEnergy;

    [Header("Time monster skill effect")]
    [SerializeField] private float timeEffectMonsterSkill;

    private bool isAffectedByMonsterSkill;

    private void Start()
    {
        if (hasAuthority)
            currentBatteryEnergy = lightBar.fillAmount;
        else
            canvas.SetActive(false);
    }
    private void Update()
    {
        if (hasAuthority)
        {
            if (!PlayerHpManager.isDead || !GamePlayer.escaped)
            {
                if (Input.GetKeyDown(KeyCode.F) && currentBatteryEnergy > 0f)
                {
                    if (!isAffectedByMonsterSkill)
                    {
                        if (!_light.activeSelf)
                            StartCoroutine(SpendBatteryPerTime());
                        else
                            StopAllCoroutines();

                        CmdLight(!_light.activeSelf);

                    }
                }
            }
        }
    }

    public void ChangeBatteryEnergy(float quantity)
    {
        currentBatteryEnergy += quantity;

        if (currentBatteryEnergy < 0)
            currentBatteryEnergy = 0f;

        if (currentBatteryEnergy > 1f)
            currentBatteryEnergy = 1f;

        lightBar.fillAmount = currentBatteryEnergy;
    }

    private IEnumerator SpendBatteryPerTime()
    {
        do
        {
            ChangeBatteryEnergy(-quantityBatteryToSpend);
            yield return new WaitForFixedUpdate();
        } while (currentBatteryEnergy > 0f);

        currentBatteryEnergy = 0f;
        CmdLight(false);
    }
    private IEnumerator CountdownFlashlightWhenAffectedByMonsterSkill()
    {
        isAffectedByMonsterSkill = true;
        yield return new WaitForSeconds(timeEffectMonsterSkill);
        isAffectedByMonsterSkill = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasAuthority)
        {
            if (other.CompareTag("MonsterSkill"))
            {
                CmdLight(false);
                StopAllCoroutines();
                StartCoroutine(CountdownFlashlightWhenAffectedByMonsterSkill());
            }
        }
    }

    [Command]
    public void CmdLight(bool state)
    {
        RpcSyncFlashlight(state);
    }
    [ClientRpc]
    private void RpcSyncFlashlight(bool state)
    {
        _light.SetActive(state);
        sound.Play("Flashlight");
    }
}
