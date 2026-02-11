using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CB_CubeRunner
{
    public class CR_MapManager : MonoBehaviour
    {
        public static CR_MapManager Instance { get; private set; }

        [Header("Refs")]
        [SerializeField] Transform player;
        [SerializeField] CR_Tiles[] tilesPrefabs;
        [SerializeField] Transform tilesRoot;

        [Header("Grid Step")]
        public Vector2 gridStep = new Vector2(0, 1);
        public float cellSize = 1f;
        [SerializeField] Vector2 gridOriginOffset = Vector2.zero;

        [Header("Endless Settings")]
        [Min(1)]
        public int initialSegments = 8;
        [Min(1)] public int rowsBehindToDrop = 4;
        [Min(1)] public int rowsAhead = 6;
        public float fallDuration = 0.7f;
        public float fallDistance = 10f;

        [Header("Spawn Control")]
        [Tooltip("Số row đầu KHÔNG spawn trap/coin/lỗ")]
        [Min(0)]
        [SerializeField] private int safeRowsNoSpawn = 10;

        [Header("Auto Drop Settings")]
        [Tooltip("Thời gian giữa mỗi lần rơi (giây)")]
        [Min(0.1f)]
        public float autoDropInterval = 2f;

        readonly Dictionary<Vector2Int, CR_TileMap> _floorLookup = new();
        readonly Dictionary<Vector2Int, CR_TileMap> _wallLookup = new();

        readonly List<CR_Tiles> _segments = new();
        readonly List<CR_Tiles> _respawnQueue = new();
        bool _isProcessingRespawn = false;

        Vector3 _startWorldPos;
        Vector3 _stepWorld;

        int _minRow;
        int _maxRow;

        float _autoDropTimer;
        int _autoDropRowTarget;
        bool _isGameOver;

        bool _worldInitialized;

        Vector3 _playerStartPos;
        Quaternion _playerStartRot;
        CR_PlayerMovement _playerMovement;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (tilesRoot == null) tilesRoot = transform;

            if (player == null)
            {
                var controller = FindFirstObjectByType<CR_PlayerController>();
                if (controller != null)
                    player = controller.transform;
            }

            if (player != null)
            {
                _playerStartPos = player.position;
                _playerStartRot = player.rotation;
                _playerMovement = player.GetComponent<CR_PlayerMovement>();
            }

            _startWorldPos = tilesRoot.position;
            _stepWorld = new Vector3(gridStep.x, 0f, gridStep.y) * cellSize;

            if (CB_GameManager.Instance != null)
            {
                CB_GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
            }
        }

        void OnDestroy()
        {
            if (CB_GameManager.Instance != null)
            {
                CB_GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
            }
        }

        void Start()
        {
            if (tilesPrefabs == null || tilesPrefabs.Length == 0)
            {
                Debug.LogError("CR_MapManager: tilesPrefabs is empty!");
                return;
            }

            InitializeWorld();
        }

        void OnGameStateChanged(GameState state)
        {
            if (state == GameState.Initialize && _worldInitialized)
            {
                InitializeWorld();
            }
        }

        void InitializeWorld()
        {
            StopAllCoroutines();
            _isProcessingRespawn = false;

            if (tilesRoot != null)
            {
                var allSegments = tilesRoot.GetComponentsInChildren<CR_Tiles>(true);
                foreach (var seg in allSegments)
                {
                    if (seg != null)
                    {
                        Destroy(seg.gameObject);
                    }
                }
            }

            _segments.Clear();
            _respawnQueue.Clear();
            _floorLookup.Clear();
            _wallLookup.Clear();

            _minRow = 0;
            _maxRow = initialSegments - 1;

            for (int row = _minRow; row <= _maxRow; row++)
            {
                SpawnSegmentForRow(row);
            }

            ResetAutoDrop();
            _isGameOver = false;

            ResetPlayerAndCamera();

            _worldInitialized = true;
        }


        void ResetPlayerAndCamera()
        {
            if (player != null)
            {
                player.gameObject.SetActive(true);
                player.position = _playerStartPos;
                player.rotation = _playerStartRot;

                if (_playerMovement == null)
                    _playerMovement = player.GetComponent<CR_PlayerMovement>();

                if (_playerMovement != null)
                    _playerMovement.ResetForNewRun();
            }

            if (CB_CameraManager.Instance != null)
            {
                CR_PlayerController controller = null;
                if (player != null)
                    controller = player.GetComponent<CR_PlayerController>();

                CB_CameraManager.Instance.ResetCamera(controller);
            }
        }

        void FixedUpdate()
        {
            if (CB_GameManager.Instance == null || CB_GameManager.Instance.GetState() != GameState.Play)
                return;

            if (player == null || _segments.Count == 0) return;

            int playerRow = GetRowFromWorld(player.position);

            if (!_isGameOver)
            {
                _autoDropTimer -= Time.fixedDeltaTime;

                if (_autoDropTimer <= 0f)
                {
                    AutoDropNextRow();
                    _autoDropTimer = autoDropInterval;
                }
            }

            for (int i = _segments.Count - 1; i >= 0; i--)
            {
                var seg = _segments[i];
                int diff = playerRow - seg.RowIndex;

                if (diff >= rowsBehindToDrop)
                {
                    _segments.RemoveAt(i);
                    UnregisterSegment(seg);
                    StartCoroutine(DropThenMoveToFront(seg));
                }
            }

            while (_maxRow < playerRow + rowsAhead)
            {
                _maxRow++;
                SpawnSegmentForRow(_maxRow);
            }

            RecalcMinRow();

            if (!_isProcessingRespawn && _respawnQueue.Count > 0)
            {
                StartCoroutine(ProcessRespawnQueue());
            }
        }

        void AutoDropNextRow()
        {
            if (_segments.Count == 0) return;

            CR_Tiles targetSeg = null;
            int maxAttempts = _segments.Count + 10;
            int attempts = 0;

            while (targetSeg == null && attempts < maxAttempts)
            {
                foreach (var seg in _segments)
                {
                    if (seg.RowIndex == _autoDropRowTarget)
                    {
                        targetSeg = seg;
                        break;
                    }
                }

                if (targetSeg == null)
                {
                    _autoDropRowTarget++;
                    attempts++;
                }
                else
                {
                    break;
                }
            }

            if (targetSeg != null)
            {
                _segments.Remove(targetSeg);
                UnregisterSegment(targetSeg);
                StartCoroutine(DropThenMoveToFront(targetSeg));
            }

            _autoDropRowTarget++;
        }

        public Vector2Int WorldToGrid(Vector3 worldPos)
        {
            Vector3 local = worldPos - _startWorldPos;

            local.x -= gridOriginOffset.x * cellSize;
            local.z -= gridOriginOffset.y * cellSize;

            int gx = Mathf.RoundToInt(local.x / cellSize);
            int gz = Mathf.RoundToInt(local.z / cellSize);
            return new Vector2Int(gx, gz);
        }

        public Vector3 GridToWorld(Vector2Int gridPos)
        {
            Vector3 pos = _startWorldPos
                          + new Vector3(gridPos.x * cellSize, 0f, gridPos.y * cellSize)
                          + new Vector3(gridOriginOffset.x * cellSize, 0f, gridOriginOffset.y * cellSize);

            return pos;
        }

        int GridToRow(Vector2Int g)
        {
            Vector2 s = gridStep;
            float denom = s.x * s.x + s.y * s.y;
            if (denom == 0) return 0;

            float dot = g.x * s.x + g.y * s.y;
            return Mathf.RoundToInt(dot / denom);
        }

        int GetRowFromWorld(Vector3 worldPos)
        {
            Vector2Int g = WorldToGrid(worldPos);
            return GridToRow(g);
        }

        Vector3 GetWorldPosForRow(int row)
        {
            return _startWorldPos + _stepWorld * row;
        }

        void SpawnSegmentForRow(int row)
        {
            if (tilesPrefabs == null || tilesPrefabs.Length == 0)
            {
                Debug.LogError("CR_MapManager: tilesPrefabs is empty!");
                return;
            }

            int prefabIndex = ((row % tilesPrefabs.Length) + tilesPrefabs.Length) % tilesPrefabs.Length;
            var prefab = tilesPrefabs[prefabIndex];

            CR_Tiles seg = Instantiate(prefab, tilesRoot);
            seg.RowIndex = row;
            seg.transform.position = GetWorldPosForRow(row);

            bool allowSpawn = row >= safeRowsNoSpawn;
            seg.RespawnContent(allowSpawn);

            RegisterSegment(seg);
            _segments.Add(seg);
        }

        IEnumerator DropThenMoveToFront(CR_Tiles seg)
        {
            if (seg == null) yield break;

            seg.SetChildrenFall(true, fallDuration, fallDistance);

            Camera cam = Camera.main;
            if (cam != null)
            {
                yield return new WaitUntil(() => IsSegmentOutOfCamera(seg, cam));
            }
            else
            {
                yield return new WaitForSeconds(fallDuration);
            }

            lock (_respawnQueue)
            {
                _respawnQueue.Add(seg);
                _respawnQueue.Sort((a, b) => a.RowIndex.CompareTo(b.RowIndex));
            }
        }

        IEnumerator ProcessRespawnQueue()
        {
            _isProcessingRespawn = true;

            while (true)
            {
                CR_Tiles seg = null;

                lock (_respawnQueue)
                {
                    if (_respawnQueue.Count == 0)
                    {
                        _isProcessingRespawn = false;
                        yield break;
                    }

                    seg = _respawnQueue[0];
                    _respawnQueue.RemoveAt(0);
                }

                if (seg != null)
                {
                    _maxRow++;
                    int newRow = _maxRow;

                    seg.RowIndex = newRow;
                    seg.transform.position = GetWorldPosForRow(newRow);

                    bool allowSpawn = newRow >= safeRowsNoSpawn;
                    seg.RespawnContent(allowSpawn);

                    RegisterSegment(seg);
                    _segments.Add(seg);

                    RecalcMinRow();
                }

                yield return null;
            }
        }

        bool IsSegmentOutOfCamera(CR_Tiles seg, Camera cam)
        {
            var tiles = seg.GetAllChildren();
            if (tiles == null || tiles.Length == 0) return true;

            foreach (var tile in tiles)
            {
                if (tile == null) continue;

                var rend = tile.GetComponentInChildren<Renderer>();
                if (rend == null) continue;

                Vector3 vp = cam.WorldToViewportPoint(rend.bounds.center);

                if (vp.z > 0f &&
                    vp.x > -0.1f && vp.x < 1.1f &&
                    vp.y > -0.1f && vp.y < 1.1f)
                {
                    return false;
                }
            }

            return true;
        }

        void RecalcMinRow()
        {
            _minRow = int.MaxValue;
            foreach (var seg in _segments)
            {
                if (seg.RowIndex < _minRow)
                    _minRow = seg.RowIndex;
            }
        }

        public void StopAutoDrop()
        {
            _isGameOver = true;
        }

        public void ResetAutoDrop()
        {
            _autoDropTimer = autoDropInterval;
            _isGameOver = false;
            _respawnQueue.Clear();
            RecalcMinRow();
            _autoDropRowTarget = _minRow;
        }

        bool IsActiveFloor(CR_TileMap tile)
        {
            return tile != null && tile.gameObject.activeInHierarchy;
        }

        public bool HasFloorAtGrid(Vector2Int g)
        {
            if (_floorLookup.TryGetValue(g, out var tile))
                return IsActiveFloor(tile);

            return false;
        }

        public bool TryGetFloorAtGrid(Vector2Int g, out CR_TileMap tile)
        {
            if (_floorLookup.TryGetValue(g, out tile) && IsActiveFloor(tile))
                return true;

            tile = null;
            return false;
        }

        public bool HasWallAtGrid(Vector2Int g) => _wallLookup.ContainsKey(g);

        void RegisterSegment(CR_Tiles seg)
        {
            var tiles = seg.GetAllChildren();
            if (tiles == null) return;

            foreach (var t in tiles)
            {
                if (t == null) continue;
                Vector2Int g = WorldToGrid(t.transform.position);

                switch (t.tileType)
                {
                    case CR_TileType.Floor:
                        _floorLookup[g] = t;
                        break;
                    case CR_TileType.Wall:
                        _wallLookup[g] = t;
                        break;
                }
            }
        }

        void UnregisterSegment(CR_Tiles seg)
        {
            var tiles = seg.GetAllChildren();
            if (tiles == null) return;

            foreach (var t in tiles)
            {
                if (t == null) continue;
                Vector2Int g = WorldToGrid(t.transform.position);

                if (t.tileType == CR_TileType.Floor)
                {
                    if (_floorLookup.TryGetValue(g, out var cur) && cur == t)
                        _floorLookup.Remove(g);
                }
                else if (t.tileType == CR_TileType.Wall)
                {
                    if (_wallLookup.TryGetValue(g, out var cur) && cur == t)
                        _wallLookup.Remove(g);
                }
            }
        }

        //         void OnDrawGizmosSelected()
        //         {
        //             if (cellSize <= 0f) return;

        //             Vector3 origin = (tilesRoot != null ? tilesRoot.position : transform.position);
        //             Gizmos.color = Color.green;
        //             Gizmos.DrawSphere(origin, cellSize * 0.15f);

        //             Vector3 cellSizeVec = new Vector3(cellSize, 0.02f, cellSize);
        //             float yOffset = 0.01f;

        //             Gizmos.color = new Color(0f, 1f, 1f, 0.7f);
        //             foreach (var kvp in _floorLookup)
        //             {
        //                 var tile = kvp.Value;
        //                 if (tile == null || !tile.gameObject.activeInHierarchy) continue;

        //                 Vector2Int g = kvp.Key;
        //                 Vector3 center = GridToWorld(g) + Vector3.up * yOffset;
        //                 Gizmos.DrawWireCube(center, cellSizeVec);
        //             }

        //             Gizmos.color = new Color(1f, 0.4f, 0f, 0.8f);
        //             foreach (var kvp in _wallLookup)
        //             {
        //                 var tile = kvp.Value;
        //                 if (tile == null || !tile.gameObject.activeInHierarchy) continue;

        //                 Vector2Int g = kvp.Key;
        //                 Vector3 center = GridToWorld(g) + Vector3.up * (yOffset * 2f);
        //                 Gizmos.DrawWireCube(center, cellSizeVec);
        //             }

        // #if UNITY_EDITOR
        //             if (player != null)
        //             {
        //                 Vector2Int pg = WorldToGrid(player.position);
        //                 Vector3 pc = GridToWorld(pg) + Vector3.up * (yOffset * 3f);

        //                 Gizmos.color = Color.yellow;
        //                 Gizmos.DrawWireCube(pc, cellSizeVec * 1.05f);
        //             }
        // #endif
        //         }
    }
}