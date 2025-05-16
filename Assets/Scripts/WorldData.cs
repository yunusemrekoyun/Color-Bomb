using UnityEngine;

[System.Serializable]
public class WorldData
{
    public string worldName;
    public Sprite worldPreview;
    public Sprite backgroundSprite; // <-- Yeni eklendi
    public int earnedStars;
    public int totalStars;
}
