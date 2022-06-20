using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using Mirror;

public class Pc : NetworkBehaviour
{
    [SyncVar(hook = nameof(HandleActivePc))] public bool turnOnPc;
    [SyncVar(hook = nameof(HandleStartCountdown))] public bool startCountdown;

    [Header("Canvas countdown")]
    [SerializeField] private GameObject countdownStartCanvas;
    [SerializeField] private GameObject countdownFinishedCanvas;
    [SerializeField] private Text countdownText;

    private bool _isCountdown;
    private bool _hasEnergy;
    private float _countdownToDecrease = 30f;

    public bool CheckIfHasEnergy()
    {
        return _hasEnergy;
    }
    public bool CheckIfIsInCountdown()
    {
        return _isCountdown;
    }

    private void HandleActivePc(bool oldValue, bool newValue)
    {
        countdownStartCanvas.SetActive(true);
        _hasEnergy = true;

    }
    private void HandleStartCountdown(bool oldValue, bool newValue)
    {
        _isCountdown = true;
        StartCoroutine(CountdownCoroutine());
    }

    private IEnumerator CountdownCoroutine()
    {
        do
        {
            _countdownToDecrease -= Time.deltaTime;
            countdownText.text = _countdownToDecrease.ToString("F0");

            yield return new WaitForFixedUpdate();
        } while (_countdownToDecrease > 0f);

        countdownStartCanvas.SetActive(false);
        countdownFinishedCanvas.SetActive(true);

        GameManager.instance.ActiveFinalLights();
    }
}
