using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine;

public class LoadingManager : MonoBehaviour
{
    [Header("Loading obj")]
    [SerializeField] private GameObject loadingObj;

    public void ActiveLoadingPanel()
    {
        loadingObj.SetActive(true);
        StartCoroutine(StartLoadingAnimation());
    }

    private void ResetLoad()
    {
        StopAllCoroutines();
        loadingObj.SetActive(false);
    }
    private IEnumerator StartLoadingAnimation()
    {
        yield return new WaitUntil(() => SceneManager.GetActiveScene().name == "Game");

        yield return new WaitForSeconds(1f);
        ResetLoad();
    }
}
