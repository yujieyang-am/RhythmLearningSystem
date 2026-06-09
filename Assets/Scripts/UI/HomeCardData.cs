using UnityEngine;

[System.Serializable]
public class HomeCardData
{
    public string titleText;        // 全大寫功能名稱，例如 "CHALLENGE"
    public string subtitleText;     // 副標題，只有中間卡片顯示
    public Sprite backgroundSprite; // 場景截圖，null 則用純色
    public Color placeholderColor;  // 沒有截圖時的佔位顏色
    public string sceneName;        // 點擊進入的場景名稱，空字串 = 待實作
}