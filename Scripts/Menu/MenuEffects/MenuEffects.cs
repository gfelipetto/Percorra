using System.Collections;
using UnityEngine;

public class MenuEffects : MonoBehaviour
{
    public TurnLightPerSecond[] lightInfo;

    public MonsterWalkMenu[] monsters;

    private void Start()
    {
        foreach (var item in lightInfo)
        {
            StartCoroutine(StartTurnOnAndTurnOffLights(item.lightObj, item.turnLightTime));
        }
        foreach (var item in monsters)
        {
            StartCoroutine(StartMonsterWalk(item));
        }
    }

    private IEnumerator StartMonsterWalk(MonsterWalkMenu monster)
    {
        var obj = monster.monsterObj;
        do
        {
            yield return new WaitForSeconds(monster.timeToWalk);
            obj.GetComponent<Animator>().SetFloat("Speed", 1f);
            yield return new WaitForSeconds(5f);
            obj.GetComponent<Animator>().SetFloat("Speed", 0f);
            obj.transform.SetPositionAndRotation(monster.startWalkingLocal.position, monster.startWalkingLocal.rotation);
        } while (true);
    }

    private IEnumerator StartTurnOnAndTurnOffLights(Light light, float time)
    {
        do
        {
            yield return new WaitForSeconds(time + Random.Range(0, 3));
            light.enabled = !light.enabled;
            yield return new WaitForSeconds(time - Random.Range(0, time - 1));
            light.enabled = !light.enabled;
        } while (true);
    }
}
