using System.Collections.Generic;
using UnityEngine;

public class BreakableBlockManager : MonoBehaviour
{


    [Header("Damaged Sprites")]
    public Sprite damagedGlassSprite;
    public Sprite damagedBoxSprite;



    private GameBoard board;


    public Dictionary<Vector2Int, int> glassHealthDict = new Dictionary<Vector2Int, int>();
    public Dictionary<Vector2Int, int> boxHealthDict = new Dictionary<Vector2Int, int>();

    private void Awake()
    {
        board = GetComponent<GameBoard>();
        if (board == null)
            Debug.LogError("GameBoard bileşeni bulunamadı!");
    }

    public bool TryDamageBlock(Vector2Int pos)
    {
        // Eğer bu pozisyonda cam varsa
        if (glassHealthDict.ContainsKey(pos))
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
                    if (sr != null)
                        sr.sprite = null; // 🧹 Sprite'ı kaldır

                    Destroy(glassObj); // 🗑️ Obje yok et
                }

                glassHealthDict.Remove(pos);
                ReleaseBalloon(pos);
            }


            return true;
        }

        // Eğer bu pozisyonda box varsa
        if (boxHealthDict.ContainsKey(pos))
        {
            boxHealthDict[pos]--;
            int newHealth = boxHealthDict[pos];

            if (newHealth == 1)
                UpdateBoxSprite(pos);

            if (newHealth <= 0)
            {
                Destroy(GameObject.Find($"Box_{pos.x}_{pos.y}"));
                boxHealthDict.Remove(pos);
                board.blockedPositions.RemoveAll(p => p.x == pos.x && p.y == pos.y); // ✅ Temizle
                ReleaseBalloon(pos);
            }


            return true;
        }

        return false;
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
    }
    private void UpdateGlassSprite(Vector2Int pos)
    {
        var glass = GameObject.Find($"Glass_{pos.x}_{pos.y}");
        if (glass != null && damagedGlassSprite != null)
        {
            var sr = glass.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.sprite = damagedGlassSprite;
        }
    }

    private void UpdateBoxSprite(Vector2Int pos)
    {
        var box = GameObject.Find($"Box_{pos.x}_{pos.y}");
        if (box != null && damagedBoxSprite != null)
        {
            var sr = box.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.sprite = damagedBoxSprite;
        }
    }

}
