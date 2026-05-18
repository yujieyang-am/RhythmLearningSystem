using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class RhythmAudioPlayer : MonoBehaviour
{
    [Header("Audio Clips")]
    [SerializeField] private AudioClip demoClickClip;
    [SerializeField] private AudioClip userHitClip;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource hitAudioSource;

    [Header("Demo Audio Pool")]
    [SerializeField] private int poolSize = 8;

    [Header("Timing")]
    [SerializeField] private double scheduleDelay = 0.2;

    [SerializeField] private NpcController npcController;
    [SerializeField] private ScoreRenderer scoreRenderer;

    private AudioSource[] demoSources;
    private int poolIndex = 0;

    private void Awake()
    {
        demoSources = new AudioSource[poolSize];

        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = new GameObject("DemoAudioSource_" + i);
            obj.transform.SetParent(transform);

            AudioSource source = obj.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.clip = demoClickClip;

            demoSources[i] = source;
        }
    }

    public void PlayUserHitSound()
    {
        if (userHitClip == null)
        {
            Debug.LogWarning("User Hit Clip is not assigned.");
            return;
        }

        if (hitAudioSource == null)
        {
            Debug.LogWarning("Hit AudioSource is not assigned.");
            return;
        }

        hitAudioSource.PlayOneShot(userHitClip);
    }

    public IEnumerator PlayDemoMeasure(
        ScoreModel scoreModel,
        int measureIndex,
        int selectedBpm,
        List<RenderGroupModel> groups)
    {
        if (scoreModel == null)
        {
            Debug.LogError("ScoreModel is null. Cannot play demo.");
            yield break;
        }

        if (demoClickClip == null)
        {
            Debug.LogError("Demo Click Clip is not assigned.");
            yield break;
        }

        MeasureModel measure = scoreModel.Measures.Find(m => m.MeasureIndex == measureIndex);

        if (measure == null)
        {
            Debug.LogError("Measure not found: " + measureIndex);
            yield break;
        }

        float secondsPerBeat = 60f / selectedBpm;
        double demoStartDspTime = AudioSettings.dspTime + scheduleDelay;

        Debug.Log("Demo audio start. Measure: " + measureIndex + ", Notes: " + measure.Notes.Count);

        foreach (var note in measure.Notes)
        {
            AudioSource source = GetNextDemoSource();
            double targetDspTime = demoStartDspTime + note.BeatInMeasure * secondsPerBeat;
            source.clip = demoClickClip;
            source.PlayScheduled(targetDspTime);

            // 找到這顆 note 對應的 group
            RenderGroupModel matchedGroup = groups.Find(g =>
                g.Notes.Contains(note)
            );

            StartCoroutine(TriggerAtDspTime(targetDspTime, matchedGroup));
        }

        double demoLengthSeconds =
            scoreModel.TimeSignature.BeatsPerMeasure * secondsPerBeat;

        yield return new WaitForSeconds((float)(scheduleDelay + demoLengthSeconds));
    }

    private AudioSource GetNextDemoSource()
    {
        AudioSource source = demoSources[poolIndex];
        poolIndex = (poolIndex + 1) % demoSources.Length;
        return source;
    }

    private IEnumerator TriggerAtDspTime(double targetDspTime, RenderGroupModel group)
    {
        while (AudioSettings.dspTime < targetDspTime)
            yield return null;

        npcController?.TriggerGiraffeNod();

        if (group != null)
            scoreRenderer?.ApplyDemoPlayed(group);
    }
}