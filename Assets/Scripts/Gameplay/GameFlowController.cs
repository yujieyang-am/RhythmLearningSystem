using System.Collections;
using TMPro;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameFlowController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject startOverlay;
    [SerializeField] private TMP_Text countdownText;

    [Header("Score")]
    [SerializeField] private ScoreRenderer scoreRenderer;

    [Header("BPM Slider")]
    [SerializeField] private BpmSliderController bpmSliderController;

    [Header("Audio")]
    [SerializeField] private RhythmAudioPlayer rhythmAudioPlayer;

    [Header("NPC")]
    [SerializeField] private NpcController npcController;

    [Header("Progress Bar")]
    [SerializeField] private UnityEngine.UI.Slider progressBar;

    private int selectedBpm;
    private ScoreModel currentScoreModel;
    private int currentMeasureIndex = 1;

    private bool inputPhaseActive = false;

    private RhythmJudge rhythmJudge = new RhythmJudge();
    private List<JudgeTarget> currentJudgeTargets;
    private List<HitResult> allHitResults = new List<HitResult>();
    private List<RenderGroupModel> allGroups;

    private int hitCount;
    private int missCount;

    private void Start()
    {
        countdownText.gameObject.SetActive(false);
        LoadAndRenderScore();
    }

    private void Update()
    {
        if (!inputPhaseActive)
            return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            rhythmAudioPlayer.PlayUserHitSound();

            npcController.TriggerGiraffeSwing();

            double tapDspTime = AudioSettings.dspTime;
            HitResult result = rhythmJudge.JudgeTap(tapDspTime);

            if (result != null)
            {
                allHitResults.Add(result);
                if (result.ResultType == HitResultType.Miss)
                    missCount++;
                else
                    hitCount++;

                scoreRenderer.ApplyHitResult(result);
            }
            else
            {
                Debug.Log("User Hit, but no target.");
            }
        }

        List<HitResult> missedResults =
            rhythmJudge.CheckMissedTargets(AudioSettings.dspTime);

        foreach (HitResult miss in missedResults)
        {
            allHitResults.Add(miss);
            Debug.Log(
                $"Miss: Delta {miss.DeltaTime:F3}s"
            );
            missCount++;
            scoreRenderer.ApplyHitResult(miss);
        }
    }

    private void LoadAndRenderScore()
    {
        var dto = ScoreLoader.LoadScore("level_001.json");
        currentScoreModel = ScoreConverter.Convert(dto);
        allGroups = RenderGroupBuilder.Build(currentScoreModel);

        if (currentScoreModel == null)
        {
            Debug.LogError("Failed to load score model.");
            return;
        }

        Debug.Log("ScoreModel: " + currentScoreModel.Title);
        Debug.Log("Time Signature: " +
                  currentScoreModel.TimeSignature.BeatsPerMeasure + "/" +
                  currentScoreModel.TimeSignature.BeatUnit);
        Debug.Log("All Notes: " + currentScoreModel.AllNotes.Count);

        scoreRenderer.RenderMeasure(
            currentScoreModel,
            currentMeasureIndex,
            GetGroupsForCurrentMeasure()
        );
    }

    public void StartGameFlow()
    {
        selectedBpm = bpmSliderController.SelectedBpm;

        Debug.Log("Start Game BPM: " + selectedBpm);

        startOverlay.SetActive(false);
        StartCoroutine(CountdownRoutine());
    }

    private IEnumerator CountdownRoutine()
    {
        countdownText.gameObject.SetActive(true);

        yield return ShowNumber("3");
        yield return ShowNumber("2");
        yield return ShowNumber("1");

        countdownText.text = "GO";
        yield return new WaitForSeconds(1f);

        countdownText.gameObject.SetActive(false);

        StartDemoPhase();
    }

    private IEnumerator ShowNumber(string num)
    {
        countdownText.text = num;
        yield return new WaitForSeconds(1f);
    }

    private void StartDemoPhase()
    {
        npcController.ShowGiraffeNod();  // ← 加這行

        Debug.Log("Demo Phase Start with BPM: " + selectedBpm);
        StartCoroutine(DemoPhaseRoutine());
    }

    private IEnumerator DemoPhaseRoutine()
    {
        inputPhaseActive = false;

        float secondsPerBeat = 60f / selectedBpm;
        float demoDuration = currentScoreModel.TimeSignature.BeatsPerMeasure * secondsPerBeat;

        // 示範音和進度條同時跑
        StartCoroutine(FillProgressBar(demoDuration));

        yield return rhythmAudioPlayer.PlayDemoMeasure(
            currentScoreModel,
            currentMeasureIndex,
            selectedBpm,
            GetGroupsForCurrentMeasure()
        );

        StartCoroutine(InputPhaseRoutine());
    }

    private IEnumerator InputPhaseRoutine()
    {
        inputPhaseActive = true;
        scoreRenderer.ResetAllNoteColors();
        npcController.ShowGiraffeIdle();

        hitCount = 0;
        missCount = 0;

        double inputStartDspTime = AudioSettings.dspTime;

        List<RenderGroupModel> groups = GetGroupsForCurrentMeasure();
        currentJudgeTargets = JudgeTargetBuilder.BuildFromRenderGroups(groups, selectedBpm);
        rhythmJudge.StartMeasure(currentJudgeTargets, inputStartDspTime);

        float secondsPerBeat = 60f / selectedBpm;
        float measureDuration = currentScoreModel.TimeSignature.BeatsPerMeasure * secondsPerBeat;

        StartCoroutine(FillProgressBar(measureDuration));  // ← 加在這行，用已有的 measureDuration

        yield return new WaitForSeconds(measureDuration);

        // 🔴 先關掉判定
        inputPhaseActive = false;

        // 🔴 再補最後的 miss
        List<HitResult> finalMisses =
            rhythmJudge.CheckMissedTargets(AudioSettings.dspTime);

        foreach (HitResult miss in finalMisses)
        {
            allHitResults.Add(miss);
            scoreRenderer.ApplyHitResult(miss);
        }

        EvaluateMeasure();
        // 🔴 最後才切下一小節
        GoToNextMeasureOrFinish();
    }

    private void GoToNextMeasureOrFinish()
    {
        currentMeasureIndex++;

        if (currentMeasureIndex > currentScoreModel.Measures.Count)
        {
            Debug.Log("Level Finished");

            ResultData resultData = ResultCalculator.Calculate(allHitResults);
            ResultDataHolder.Set(resultData);

            UnityEngine.SceneManagement.SceneManager.LoadScene("SC_Result");
            return;
        }

        Debug.Log("Go To Measure: " + currentMeasureIndex);

        scoreRenderer.RenderMeasure(
            currentScoreModel,
            currentMeasureIndex,
            GetGroupsForCurrentMeasure()
        );

        StartDemoPhase();
    }

    private List<RenderGroupModel> GetGroupsForCurrentMeasure()
    {
        List<RenderGroupModel> result = new List<RenderGroupModel>();

        foreach (var group in allGroups)
        {
            if (group.Notes == null || group.Notes.Count == 0)
                continue;

            if (group.Notes[0].MeasureIndex == currentMeasureIndex)
            {
                result.Add(group);
            }
        }

        return result;
    }

    private void EvaluateMeasure()
    {
        if (missCount == 0)
        {
            npcController.ShowPigGood();
        }
        else if (missCount <= 2)
        {
            npcController.ShowPigNormal();
        }
        else
        {
            npcController.ShowPigBad();
        }
    }

    private IEnumerator FillProgressBar(float duration)
    {
        float elapsed = 0f;
        progressBar.value = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            progressBar.value = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }

        progressBar.value = 1f;
    }

    private IEnumerator DrainProgressBar(float duration)
    {
        float elapsed = 0f;
        progressBar.value = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            progressBar.value = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }

        progressBar.value = 1f;
    }
}