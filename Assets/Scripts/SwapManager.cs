using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(GameBoard), typeof(HintManager))]
public class SwapManager : MonoBehaviour
{
    private GameBoard board;
    private HintManager hintManager;

    private void Awake()
    {
        board = GetComponent<GameBoard>();
        hintManager = GetComponent<HintManager>();
    }

    public void SwapBalloons(int x1, int y1, int x2, int y2)
    {
        hintManager.ResetIdleTimer();

        var b1 = board.allBalloons[x1, y1];
        var b2 = board.allBalloons[x2, y2];
        if (b1 == null || b2 == null) return;

        // ❄️ Freeze check: eğer biri donuksa, swap yapma
        var bi1 = b1.GetComponent<BalloonItem>();
        var bi2 = b2.GetComponent<BalloonItem>();
        if ((bi1 != null && bi1.isFrozen) || (bi2 != null && bi2.isFrozen))
        {
            Debug.Log($"🚫 Swap iptal: Balonlardan biri frozen → ({x1},{y1}) or ({x2},{y2})");
            return;
        }

        // Swap işlemi
        board.allBalloons[x1, y1] = b2;
        board.allBalloons[x2, y2] = b1;

        bi1.x = x2; bi1.y = y2;
        bi2.x = x1; bi2.y = y1;

        bi1.MoveTo(new Vector3(x2 * board.spacing + board.offsetX,
                               y2 * board.spacing + board.offsetY, 0));
        bi2.MoveTo(new Vector3(x1 * board.spacing + board.offsetX,
                               y1 * board.spacing + board.offsetY, 0));

        StartCoroutine(HandlePostSwap(b1, b2, x1, y1, x2, y2));
    }

    private IEnumerator HandlePostSwap(GameObject b1, GameObject b2, int x1, int y1, int x2, int y2)
    {
        yield return new WaitForSeconds(0.25f); // Swap animasyonu süresi

        SpecialItem s1 = b1.GetComponent<SpecialItem>();
        SpecialItem s2 = b2.GetComponent<SpecialItem>();

        bool specialTriggered = false;

        if (s1 != null)
        {
            board.allBalloons[x2, y2] = null;
            StartCoroutine(TriggerSpecial(s1, x2, y2, b2.tag)); // önce patlat
            Destroy(b1); // sonra yok et
            specialTriggered = true;
        }

        if (s2 != null)
        {
            board.allBalloons[x1, y1] = null;
            StartCoroutine(TriggerSpecial(s2, x1, y1, b1.tag));
            Destroy(b2); // sonra yok et
            specialTriggered = true;
        }

        if (!specialTriggered)
        {
            StartCoroutine(CheckMatchAfterSwap(x1, y1, x2, y2));
        }
    }



    private IEnumerator TriggerSpecial(SpecialItem item, int x, int y, string targetTag = null)
    {
        var toDestroy = new List<GameObject>();

        // Yardımcı: pozisyona göre camı kırma/düşürme kontrolü
        bool TryHandleGlassAt(int i, int j)
        {
            var balloon = board.allBalloons[i, j];
            if (balloon == null) return false;

            var bi = balloon.GetComponent<BalloonItem>();
            if (bi != null && bi.isFrozen)
            {
                var gridPos = new Vector2Int(i, j);
                if (board.breakableManager.glassHealthDict.TryGetValue(gridPos, out int health))
                {
                    health -= 1;
                    board.breakableManager.glassHealthDict[gridPos] = health;
                    Debug.Log($"🔨 Cam kırılıyor at {gridPos}, kalan can: {health}");

                    if (health <= 0)
                    {
                        // 1) Sözlükten çıkar
                        board.breakableManager.glassHealthDict.Remove(gridPos);

                        // 2) Sahnedeki Glass objesini bulup yok et
                        var glassObj = GameObject.Find($"Glass_{i}_{j}");
                        if (glassObj != null)
                        {
                            Destroy(glassObj);
                            Debug.Log($"🗑️ Glass_{i}_{j} sahneden silindi.");
                        }

                        // 3) Balonu serbest bırak
                        bi.isFrozen = false;
                        Debug.Log($"✅ Cam yok! Balon ({i},{j}) artık serbest.");
                    }
                }
                // Cam olan hücre için patlatmayı atla
                return true;
            }

            return false;
        }

        // Horizontal-4 special
        if (item.state == SpecialItem.SpecialState.Horizontal4)
        {
            for (int i = 0; i < board.width; i++)
            {
                if (TryHandleGlassAt(i, y)) continue;
                var b = board.allBalloons[i, y];
                if (b != null)
                {
                    toDestroy.Add(b);
                    board.allBalloons[i, y] = null;
                }
            }
        }
        // Vertical-4 special
        else if (item.state == SpecialItem.SpecialState.Vertical4)
        {
            for (int j = 0; j < board.height; j++)
            {
                if (TryHandleGlassAt(x, j)) continue;
                var b = board.allBalloons[x, j];
                if (b != null)
                {
                    toDestroy.Add(b);
                    board.allBalloons[x, j] = null;
                }
            }
        }
        // Bomb (Vertical5) special
        else if (item.state == SpecialItem.SpecialState.Vertical5)
        {
            int radius = 1;
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    int tx = x + dx, ty = y + dy;
                    if (tx < 0 || tx >= board.width || ty < 0 || ty >= board.height)
                        continue;
                    if (TryHandleGlassAt(tx, ty)) continue;
                    var b = board.allBalloons[tx, ty];
                    if (b != null)
                    {
                        toDestroy.Add(b);
                        board.allBalloons[tx, ty] = null;
                    }
                }
            }
        }
        // Color-clear (Horizontal5) special
        else if (item.state == SpecialItem.SpecialState.Horizontal5 && targetTag != null)
        {
            for (int i = 0; i < board.width; i++)
            {
                for (int j = 0; j < board.height; j++)
                {
                    if (TryHandleGlassAt(i, j)) continue;
                    var b = board.allBalloons[i, j];
                    if (b != null && b.tag == targetTag)
                    {
                        toDestroy.Add(b);
                        board.allBalloons[i, j] = null;
                    }
                }
            }
        }

        // Klasik 3’lük/4’lük/5’lik match temizlemeleri bu metotta değil
        // Yalnızca special ile yok edilecekler
        foreach (var obj in toDestroy)
            Destroy(obj);

        yield return new WaitForSeconds(0.4f);

        GetComponent<DropManager>().DropBalloons();
    }



    private IEnumerator CheckMatchAfterSwap(int x1, int y1, int x2, int y2)
    {
        yield return new WaitForSeconds(0.3f);
        var matchManager = GetComponent<MatchManager>();
        if (!matchManager.CheckAndClearMatches())
            SwapWithoutCheck(x1, y1, x2, y2);
    }

    private void SwapWithoutCheck(int x1, int y1, int x2, int y2)
    {
        var b1 = board.allBalloons[x1, y1];
        var b2 = board.allBalloons[x2, y2];
        if (b1 == null || b2 == null) return;

        var i1 = b1.GetComponent<BalloonItem>();
        var i2 = b2.GetComponent<BalloonItem>();

        board.allBalloons[x1, y1] = b2;
        board.allBalloons[x2, y2] = b1;
        i1.x = x2; i1.y = y2;
        i2.x = x1; i2.y = y1;

        i1.MoveTo(new Vector3(x2 * board.spacing + board.offsetX,
                               y2 * board.spacing + board.offsetY, 0));
        i2.MoveTo(new Vector3(x1 * board.spacing + board.offsetX,
                               y1 * board.spacing + board.offsetY, 0));
    }
}
