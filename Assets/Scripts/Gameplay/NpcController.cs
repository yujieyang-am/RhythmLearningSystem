using UnityEngine;
using System.Collections;

public class NpcController : MonoBehaviour
{
    [Header("Left NPC (Giraffe)")]
    [SerializeField] private GameObject leftIdle;
    [SerializeField] private GameObject leftNod;
    [SerializeField] private GameObject leftSwing;

    [Header("Right NPC (Pig Doctor)")]
    [SerializeField] private GameObject rightIdle;
    [SerializeField] private GameObject rightGood;
    [SerializeField] private GameObject rightBad;

    // ---------- Left ----------
    public void ShowGiraffeIdle()
    {
        SetLeftState(leftIdle);
    }

    public void ShowGiraffeNod()
    {
        SetLeftState(leftNod);
    }

    public void ShowGiraffeSwing()
    {
        SetLeftState(leftSwing);
    }

    private void SetLeftState(GameObject target)
    {
        leftIdle.SetActive(false);
        leftNod.SetActive(false);
        leftSwing.SetActive(false);

        target.SetActive(true);
    }

    // ---------- Right ----------
    public void ShowPigNormal()
    {
        SetRightState(rightIdle);
    }

    public void ShowPigGood()
    {
        SetRightState(rightGood);
    }

    public void ShowPigBad()
    {
        SetRightState(rightBad);
    }

    private void SetRightState(GameObject target)
    {
        rightIdle.SetActive(false);
        rightGood.SetActive(false);
        rightBad.SetActive(false);

        target.SetActive(true);
    }


    public void TriggerGiraffeNod(float nodDuration = 0.15f)
    {
        StartCoroutine(NodRoutine(nodDuration));
    }

    private IEnumerator NodRoutine(float duration)
    {
        ShowGiraffeNod();
        yield return new WaitForSeconds(duration);
        ShowGiraffeIdle();
    }

    public void TriggerGiraffeSwing(float swingDuration = 0.2f)
    {
        StartCoroutine(SwingRoutine(swingDuration));
    }

    private IEnumerator SwingRoutine(float duration)
    {
        ShowGiraffeSwing();
        yield return new WaitForSeconds(duration);
        ShowGiraffeIdle();
    }

}