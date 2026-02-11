using System;
using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public enum CellDirection
{
    LEFT = 0,
    RIGHT = 1,
    UP = 2,
    DOWN = 3
}

[Serializable]
public struct CellStruct
{
    public GameObject prefab;
    [Range(1, 2)] public int objectSize;

}

namespace bJakGZQ3_Outer_World
{
    public class bJakGZQ3_GridSystem : MonoBehaviour
    {
        [Header("Grid Settings")]
        [SerializeField][Range(7, 10)] private int _gridSize = 7;
        [SerializeField][Range(1, 2)] private int _cellSize = 1;
        [SerializeField] private CellStruct _cell;

        [Space(8)]
        [Header("Grid Container")]
        [SerializeField] private Transform _gridContainer;

        [Space(8)]
        [Header("Grid Visualize")]
        [SerializeField] private Transform _gridVisual_7x7;
        [SerializeField] private Transform _gridVisual_8x8;
        [SerializeField] private Transform _gridVisual_9x9;
        [SerializeField] private Transform _gridVisual_10x10;

        [Header("Grid Spawn Tween")]
        [SerializeField] float spawnOffsetY = 1.25f;
        [SerializeField] float spawnDuration = 0.5f;
        [SerializeField] Ease spawnEase = Ease.OutBack;
        [SerializeField, Range(0f, 0.5f)] float scaleOvershoot = 0.12f;

        [Header("Ripple Tween")]
        [SerializeField] bool rippleFromCenter = true;
        [SerializeField] Vector2Int rippleOrigin = new Vector2Int(0, 0);
        [SerializeField, Range(0f, 0.2f)] float delayPerTile = 0.03f;

        [Header("Spawn helpers")]
        [Tooltip("Layers considered as blocking when checking free cell (set to Player/Enemy/Item, exclude Ground/Tiles)")]
        public LayerMask blockingLayers = ~0; // set in inspector (default all) — change to only blocking layers

        private int maxSize = 10;
        bool _isInitialized = false;
        public bool IsInitialized => _isInitialized;

        Vector3 _originBL;

        public void Inititalize(Action OnComplete)
        {
            float half = (_gridSize - 1) * 0.5f;
            _originBL = transform.position + new Vector3(-half * _cellSize, 0f, -half * _cellSize);
            BoardInitialized(OnComplete);
            _isInitialized = true;
        }

        private void BoardInitialized(Action OnComplete)
        {
            ResetContainer();
            float maxFinish = 0f;
            int spawned = 0;

            for (int i = 0; i < _gridSize; i++)
            {
                for (int j = 0; j < _gridSize; j++)
                {
                    GameObject prefab = _cell.prefab;
                    if (prefab == null) continue;

                    Vector3 pos = IndexToWorld(i, j);
                    GameObject go = Instantiate(prefab, pos, Quaternion.identity, _gridContainer);

                    float delay = GetDelayForTween(i, j, _gridSize);
                    float finish = delay + spawnDuration;
                    if (finish > maxFinish) maxFinish = finish;

                    spawned++;
                    PlaySpawnTween(go.transform, delay, null);
                }
            }

            if (spawned > 0)
            {
                DOVirtual.DelayedCall(maxFinish, () =>
                {
                    ActivateVisualForSize(_gridSize);
                    OnComplete?.Invoke();
                });
            }
        }

        private void ResetContainer()
        {
            if (_gridContainer == null)
            {
                GameObject go = new("Grid Container");
                go.transform.SetParent(transform);
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = Vector3.one;
                _gridContainer = go.transform;
            }

            if (_gridContainer.childCount > 0)
            {
                for (int k = _gridContainer.childCount - 1; k >= 0; k--)
                {
                    DestroyImmediate(_gridContainer.GetChild(k).gameObject);
                }
            }
        }

        private float GetDelayForTween(int i, int j, int sizeNow)
        {
            Vector2 originIdx = rippleFromCenter
                ? new Vector2((sizeNow - 1) * 0.5f, (sizeNow - 1) * 0.5f)
                : (Vector2)rippleOrigin;

            return Vector2.Distance(new Vector2(i, j), originIdx) * delayPerTile;
        }

        public void AddSize(int amount = 1)
        {
            if (amount <= 0) return;

            if (_gridSize >= maxSize) return;
            bJakGZQ3_GameManager.Instance?.SetState(GameState.LevelSetup);

            int increase = Mathf.Min(amount, maxSize - _gridSize);
            int oldSize = _gridSize;

            for (int step = 1; step <= increase; step++)
            {
                int newSize = oldSize + step;
                Vector2 originIdx = GetRippleOriginIndices(newSize);

                int ringRemaining = newSize + (newSize - 1);

                Action onTweenDone = () =>
                {
                    ringRemaining--;
                    if (ringRemaining == 0)
                    {
                        ActivateVisualForSize(newSize);
                        _gridSize += increase;
                        bJakGZQ3_GameManager.Instance?.SetState(GameState.Play);
                    }
                };

                int topJ = newSize - 1;
                for (int i = 0; i < newSize; i++)
                {
                    SpawnCells(i, topJ, onTweenDone);
                }

                int rightI = newSize - 1;
                for (int j = 0; j < newSize - 1; j++)
                {
                    SpawnCells(rightI, j, onTweenDone);
                }
            }
        }

        private Vector2 GetRippleOriginIndices(int sizeNow)
        {
            return rippleFromCenter
                ? new Vector2((sizeNow - 1) * 0.5f, (sizeNow - 1) * 0.5f)
                : (Vector2)rippleOrigin;
        }

        private Vector3 IndexToWorld(int i, int j)
        {
            return _originBL + new Vector3(i * _cellSize, 0f, j * _cellSize);
        }

        private void SpawnCells(int i, int j, Action onTweenComplete)
        {
            GameObject prefab = _cell.prefab;
            if (prefab == null) { onTweenComplete?.Invoke(); return; }

            Vector3 pos = IndexToWorld(i, j);
            GameObject go = Instantiate(prefab, pos, Quaternion.identity, _gridContainer);

            float delay = GetDelayForTween(i, j, _gridSize);
            PlaySpawnTween(go.transform, delay, onTweenComplete);
        }

        private Sequence PlaySpawnTween(Transform t, float delay, Action onComplete)
        {
            Vector3 target = t.position;

            t.position = target + Vector3.down * spawnOffsetY;
            t.localScale = Vector3.one * (1f - scaleOvershoot);

            Sequence seq = DOTween.Sequence();
            seq.AppendInterval(delay);
            seq.Append(t.DOMoveY(target.y, spawnDuration).SetEase(spawnEase));
            seq.Join(t.DOScale(_cell.objectSize, spawnDuration * 0.9f).SetEase(Ease.OutBack));

            if (onComplete != null) seq.OnComplete(() => onComplete.Invoke());
            return seq;
        }

        private void ActivateVisualForSize(int size)
        {
            DeactivateAllVisuals();
            Transform t = GetVisualForSize(size);
            if (t != null) t.gameObject.SetActive(true);
        }

        private void DeactivateAllVisuals()
        {
            if (_gridVisual_7x7) _gridVisual_7x7.gameObject.SetActive(false);
            if (_gridVisual_8x8) _gridVisual_8x8.gameObject.SetActive(false);
            if (_gridVisual_9x9) _gridVisual_9x9.gameObject.SetActive(false);
            if (_gridVisual_10x10) _gridVisual_10x10.gameObject.SetActive(false);
        }

        private Transform GetVisualForSize(int size)
        {
            switch (size)
            {
                case 7: return _gridVisual_7x7;
                case 8: return _gridVisual_8x8;
                case 9: return _gridVisual_9x9;
                case 10: return _gridVisual_10x10;
                default: return null;
            }
        }

        private static Vector2Int DirToDelta(CellDirection dir)
        {
            switch (dir)
            {
                case CellDirection.LEFT: return new Vector2Int(-1, 0);
                case CellDirection.RIGHT: return new Vector2Int(1, 0);
                case CellDirection.UP: return new Vector2Int(0, 1);
                case CellDirection.DOWN: return new Vector2Int(0, -1);
                default: return Vector2Int.zero;
            }
        }

        private bool IsInside(int i, int j) => i >= 0 && j >= 0 && i < _gridSize && j < _gridSize;

        private bool WorldToIndex(Vector3 world, out int i, out int j)
        {
            float fx = (world.x - _originBL.x) / _cellSize;
            float fz = (world.z - _originBL.z) / _cellSize;
            i = Mathf.RoundToInt(fx);
            j = Mathf.RoundToInt(fz);
            return IsInside(i, j);
        }

        public bool TryGetNextCellCenter(Vector3 fromWorldPos, CellDirection dir, out Vector3 center)
        {
            center = default;
            if (!_isInitialized) return false;
            if (!WorldToIndex(fromWorldPos, out int ci, out int cj)) return false;

            Vector2Int d = DirToDelta(dir);
            int ni = ci + d.x;
            int nj = cj + d.y;
            if (!IsInside(ni, nj)) return false;

            center = IndexToWorld(ni, nj);
            return true;
        }

        // new public helpers ------------------------------------------------
        public int GridSize => _gridSize;
        public int CellSize => _cellSize;

        /// <summary>
        /// Try get world center for given grid indices (i,j).
        /// </summary>
        public bool TryGetCellCenter(int i, int j, out Vector3 center)
        {
            center = default;
            if (!IsInside(i, j)) return false;
            center = IndexToWorld(i, j);
            return true;
        }

        /// <summary>
        /// Try find a random free cell center on the board.
        /// Uses Physics.OverlapSphere with configurable layer mask.
        /// </summary>
        public bool TryGetRandomFreeCell(out Vector3 center, float checkRadius = 0.12f)
        {
            center = default;
            if (!_isInitialized) return false;

            int size = _gridSize;
            var indices = new System.Collections.Generic.List<(int i, int j)>(size * size);
            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                    indices.Add((i, j));

            // shuffle
            for (int k = 0; k < indices.Count; k++)
            {
                int r = UnityEngine.Random.Range(k, indices.Count);
                var tmp = indices[k];
                indices[k] = indices[r];
                indices[r] = tmp;
            }

            // find first free
            foreach (var idx in indices)
            {
                Vector3 c = IndexToWorld(idx.i, idx.j);
                // small upward offset if your colliders are at different heights (optional)
                // c.y += 0.1f;

                Collider[] hits = Physics.OverlapSphere(c, checkRadius, blockingLayers, QueryTriggerInteraction.Ignore);
                bool occupied = false;
                foreach (var h in hits)
                {
                    if (h == null) continue;
                    occupied = true;
                    break;
                }
                if (!occupied)
                {
#if UNITY_EDITOR
                    // debug visualize briefly in editor
                    Debug.DrawLine(c, c + Vector3.up * 0.2f, Color.green, 1.0f);
#endif
                    center = c;
                    return true;
                }
            }

            return false;
        }

        public bool TryGetRandomFreeCellExclusive(
    HashSet<Vector2Int> bannedCells,
    out Vector3 center,
    float checkRadius = 0.12f,
    float yOverride = -999f
)
        {
            center = default;
            if (!_isInitialized) return false;

            int size = _gridSize;
            var indices = new System.Collections.Generic.List<Vector2Int>(size * size);

            // gom hết index
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    indices.Add(new Vector2Int(i, j));
                }
            }

            // shuffle
            for (int k = 0; k < indices.Count; k++)
            {
                int r = UnityEngine.Random.Range(k, indices.Count);
                (indices[k], indices[r]) = (indices[r], indices[k]);
            }

            foreach (var idx in indices)
            {
                // tile này đã pick trong cùng đợt spawn => skip
                if (bannedCells != null && bannedCells.Contains(idx))
                    continue;

                Vector3 c = IndexToWorld(idx.x, idx.y);

                Vector3 checkPos = c;
                if (yOverride != -999f)
                    checkPos.y = yOverride;

                // *** FIX: detect cả trigger collider ***
                Collider[] hits = Physics.OverlapSphere(
                    checkPos,
                    checkRadius,
                    blockingLayers,
                    QueryTriggerInteraction.Collide // <--- đổi từ Ignore thành Collide
                );

                bool occupied = false;
                foreach (var h in hits)
                {
                    if (h == null) continue;
                    occupied = true;
                    break;
                }

                if (!occupied)
                {
                    center = c;

#if UNITY_EDITOR
                    Debug.DrawLine(checkPos, checkPos + Vector3.up * 0.4f, Color.green, 1f);
#endif
                    return true;
                }
            }

            return false;
        }

        // expose helper để world -> (i,j) nếu cần
        public bool WorldToCellIndices(Vector3 worldPos, out Vector2Int ij)
        {
            ij = default;
            if (!WorldToIndex(worldPos, out int i, out int j))
                return false;
            ij = new Vector2Int(i, j);
            return true;
        }

        // ------------------------------------------------------------------
    }
}
