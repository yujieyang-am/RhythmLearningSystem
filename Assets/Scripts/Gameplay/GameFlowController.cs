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

        // 自由編譜傳入的 ScoreModel 優先
        if (FreeComposeSessionHolder.PendingScore != null)
        {
            var score = FreeComposeSessionHolder.PendingScore;
            FreeComposeSessionHolder.PendingScore = null;
            StartWithScore(score);
        }
        else
        {
            LoadAndRenderScore();
        }
    }

    // ── 自由編譜進入點 ──────────────────────────────
    public void StartWithScore(ScoreModel score)
    {
        currentScoreModel  = score;
        allGroups          = RenderGroupBuilder.Build(score);
        currentMeasureIndex = 1;
        allHitResults.Clear();

        scoreRenderer.RenderMeasure(
            currentScoreModel,
            currentMeasureIndex,
            GetGroupsForCurrentMeasure()
        );

        // 自由編譜沒有 BPM Slider，直接用 score 的 BPM
        selectedBpm = Mathf.RoundToInt(score.Bpm);
        startOverlay.SetActive(false);
        StartCoroutine(CountdownRoutine());
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
            Debug.Log($"Miss: Delta {miss.DeltaTime:F3}s");
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

        scoreRenderer.RenderMeasure(
            currentScoreModel,
            currentMeasureIndex,
            GetGroupsForCurrentMeasure()
        );
    }

    public void StartGameFlow()
    {
        selectedBpm = bpmSliderController.SelectedBpm;
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
        npcController.ShowGiraffeNod();
        StartCoroutine(DemoPhaseRoutine());
    }

    private IEnumerator DemoPhaseRoutine()
    {
        inputPhaseActive = false;

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

        float secondsPerBeat = 60f / selectedBpm;
        float measureDuration = currentScoreModel.TimeSignature.BeatsPerMeasure * secondsPerBeat;

        double inputStartDspTime = rhythmAudioPlayer.LastDemoStartDspTime + measureDuration;

        List<RenderGroupModel> groups = GetGroupsForCurrentMeasure();
        currentJudgeTargets = JudgeTargetBuilder.BuildFromRenderGroups(groups, selectedBpm);
        rhythmJudge.StartMeasure(currentJudgeTargets, inputStartDspTime);

        yield return new WaitForSeconds(measureDuration);

        inputPhaseActive = false;

        List<HitResult> finalMisses =
            rhythmJudge.CheckMissedTargets(AudioSettings.dspTime);

        foreach (HitResult miss in finalMisses)
        {
            allHitResults.Add(miss);
            scoreRenderer.ApplyHitResult(miss);
        }

        EvaluateMeasure();
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

            SceneManager.LoadScene("SC_Result");
            return;
        }

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
                result.Add(group);
        }

        return result;
    }

    private void EvaluateMeasure()
    {
        if (missCount == 0)
            npcController.ShowPigGood();
        else if (missCount <= 2)
            npcController.ShowPigNormal();
        else
            npcController.ShowPigBad();
    }
}