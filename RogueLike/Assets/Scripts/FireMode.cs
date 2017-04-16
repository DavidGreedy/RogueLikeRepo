using System.Collections;
using UnityEngine;

public class FireMode : MonoBehaviour
{
    private bool isReady = true;
    private bool isTriggerDown = false;

    public IEnumerator CooldownRoutine(float cooldown)
    {
        isReady = false;
        //print(isReady);
        while (!isReady)
        {
            yield return new WaitForSeconds(cooldown);
            isReady = true;
            //print(isReady);
        }
    }

    public virtual bool IsReady
    {
        get { return isReady; }
    }

    public void PullTrigger()
    {
        isTriggerDown = true;
    }

    public void ReleaseTrigger()
    {
        isTriggerDown = false;
    }
}