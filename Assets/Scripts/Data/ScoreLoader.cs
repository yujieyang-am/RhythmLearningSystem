using System.IO;
using UnityEngine;

public class ScoreLoader
{
    public static ScoreJsonDto LoadScore(string fileName)
    {
        string path = Path.Combine(Application.streamingAssetsPath, "Scores", fileName);

        if (!File.Exists(path))
        {
            Debug.LogError("Score file not found: " + path);
            return null;
        }

        string json = File.ReadAllText(path);
        ScoreJsonDto dto = JsonUtility.FromJson<ScoreJsonDto>(json);

        return dto;
    }
}