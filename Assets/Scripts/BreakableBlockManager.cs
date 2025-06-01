// Refactored BreakableBlockManager.cs
// - Kod düzenlendi ve okunabilirlik artırıldı
// - DamageRandomBlocks() metodu eklendi

using System.Collections.Generic;
using UnityEngine;

public class BreakableBlockManager : MonoBehaviour
{
    [Header("Damaged Sprites")]
    public Sprite damagedGlassSprite;
    public Sprite damagedBoxSprite;

    private TaskManager taskManager;
    private GameBoard board;

    public Dictionary<Vector2Int, int> glassHealthDict = new();
    public Dictionary<Vector2Int, int> boxHealthDict = new();

    private void Awake()
    {
        board = GetComponent<GameBoard>();
        taskManager = GetComponent<TaskManager>();

        if (board == null)
            Debug.LogError("GameBoard bileşeni bulunamadı!");
    }

    public bool TryDamageBlock(Vector2Int pos)
    {
        if (glassHealthDict.ContainsKey(pos))
        {
            return DamageGlass(pos);
        }
        else if (boxHealthDict.ContainsKey(pos))
        {
            return DamageBox(pos);
        }
        return false;
    }

    private bool DamageGlass(Vector2Int pos)
    {
        glassHealthDict[pos]--;
        int newHealth = glassHealthDict[pos];

        if (newHealth == 1)
            UpdateGlassSprite(pos);

        if (newHealth <= 0)
        {
            var glassObj = GameObject.Find($"Glass_{pos.x}_{pos.y}");
            if (glassObj != null)
            {
                var sr = glassObj.GetComponent<SpriteRenderer>();
                taskManager?.OnItemDestroyed(glassObj);
                if (sr != null)
                    sr.sprite = null;
                Destroy(glassObj);
            }

            glassHealthDict.Remove(pos);
            ReleaseBalloon(pos);
        }
        return true;
    }

    private bool DamageBox(Vector2Int pos)
    {
        boxHealthDict[pos]--;
        int newHealth = boxHealthDict[pos];

        if (newHealth == 1)
            UpdateBoxSprite(pos);

        if (newHealth <= 0)
        {
            var boxObj = GameObject.Find($"Box_{pos.x}_{pos.y}");
            if (boxObj != null)
            {
                taskManager?.OnItemDestroyed(boxObj);
                Destroy(boxObj);
            }

            boxHealthDict.Remove(pos);
            board.blockedPositions.RemoveAll(p => p.x == pos.x && p.y == pos.y);
            ReleaseBalloon(pos);
        }
        return true;
    }

    private void ReleaseBalloon(Vector2Int pos)
    {
        var balloon = board.allBalloons[pos.x, pos.y];
        if (balloon != null)
        {
            var script = balloon.GetComponent<BalloonItem>();
            if (script != null)
            {
                script.isFrozen = false;
                Debug.Log($"🔓 Balon serbest bırakıldı: {pos}");
            }
        }

        GetComponent<DropManager>()?.DropBalloons();
    }

    private void UpdateGlassSprite(Vector2Int pos)
    {
        var glass = GameObject.Find($"Glass_{pos.x}_{pos.y}");
        var sr = glass?.GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.sprite = damagedGlassSprite;
    }

    private void UpdateBoxSprite(Vector2Int pos)
    {
        var box = GameObject.Find($"Box_{pos.x}_{pos.y}");
        var sr = box?.GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.sprite = damagedBoxSprite;
    }

    // ✅ SwapManager için gerekli fonksiyon
    public void DamageRandomBlocks(int count)
    {
        List<Vector2Int> allBlocks = new();

        foreach (var entry in glassHealthDict)
            if (entry.Value > 0) allBlocks.Add(entry.Key);

        foreach (var entry in boxHealthDict)
            if (entry.Value > 0) allBlocks.Add(entry.Key);

        for (int i = 0; i < count && allBlocks.Count > 0; i++)
        {
            int index = Random.Range(0, allBlocks.Count);
            Vector2Int pos = allBlocks[index];
            TryDamageBlock(pos);
            allBlocks.RemoveAt(index);
        }
    }
    public bool HasBlock(int x, int y)
{
    Vector2Int pos = new Vector2Int(x, y);
    return glassHealthDict.ContainsKey(pos) || boxHealthDict.ContainsKey(pos);
}
}