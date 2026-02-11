using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace bJakGZQ3_Outer_World
{
    public class bJakGZQ3_GamePlay : bJakGZQ3_UIPage
    {
        [Header("Panels")]
        [SerializeField] Panels RightPanel;
        [SerializeField] Panels LeftPanel;
        [SerializeField] Panels TopPanel;

        [Header("Text")]
        [SerializeField] TextMeshProUGUI Time;
        [SerializeField] TextMeshProUGUI Steps;
        [SerializeField] TextMeshProUGUI Round;

        [Header("Oxygen Slider")]
        [SerializeField] Slider oxygen;
        [SerializeField] TextMeshProUGUI oxygenText;

        [Header("Gun")]
        [SerializeField] bJakGZQ3_Gun[] gunUI;

        [Header("Missions")]
        [SerializeField] bJakGZQ3_MissionItems missionItemPrefab;
        [SerializeField] RectTransform spawnMissionContainer;

        List<bJakGZQ3_MissionItems> _spawnedMissionCells = new();

        bJakGZQ3_DataManager _dataManager;

        Vector2 _rightStart;
        Vector2 _leftStart;
        Vector2 _topStart;

        bool _initializedPos;

        bJakGZQ3_LevelManager _levelManager;
        bJakGZQ3_Oxygen _playerOxygen;

        public override void Init(bJakGZQ3_BaseUI parent)
        {
            base.Init(parent);
            CacheStartPositions();

            _levelManager = bJakGZQ3_LevelManager.Instance;

            // oxygen lấy từ player
            if (_playerOxygen == null)
            {
                var player = FindFirstObjectByType<bJakGZQ3_Player>();
                if (player != null)
                    _playerOxygen = player.GetComponent<bJakGZQ3_Oxygen>();
            }

            // missionMgr
            if (_dataManager == null)
            {
                _dataManager = bJakGZQ3_DataManager.Instance;
            }

            gunUI = GetComponentsInChildren<bJakGZQ3_Gun>();
        }

        void OnEnable()
        {
            // level events
            if (_levelManager == null)
                _levelManager = bJakGZQ3_LevelManager.Instance;

            if (_levelManager != null)
            {
                _levelManager.OnTimeChanged += HandleTimeChanged;
                _levelManager.OnStepsChanged += HandleStepsChanged;
                _levelManager.OnRoundChanged += HandleRoundChanged;

                HandleTimeChanged(_levelManager.TimeStr);
                HandleStepsChanged(_levelManager.StepsCount);
                HandleRoundChanged(_levelManager.RoundCount);
            }

            // oxygen events
            if (_playerOxygen == null)
            {
                var player = FindFirstObjectByType<bJakGZQ3_Player>();
                if (player != null)
                    _playerOxygen = player.GetComponent<bJakGZQ3_Oxygen>();
            }

            if (_playerOxygen != null)
            {
                _playerOxygen.OnOxygenChanged += HandleOxygenChanged;
                HandleOxygenChanged(_playerOxygen.CurrentOxygen, _playerOxygen.MaxOxygen);
            }

            // mission events
            if (_dataManager == null)
                _dataManager = bJakGZQ3_DataManager.Instance;

            if (_dataManager != null)
            {
                _dataManager.OnMissionListChanged += RebuildMissionList;
                _dataManager.OnMissionSlotUpdated += HandleMissionSlotUpdated;
                _dataManager.OnGunChanged += HandleGunChanged;

                RebuildMissionList();
            }
        }

        void OnDisable()
        {
            if (_levelManager != null)
            {
                _levelManager.OnTimeChanged -= HandleTimeChanged;
                _levelManager.OnStepsChanged -= HandleStepsChanged;
                _levelManager.OnRoundChanged -= HandleRoundChanged;
            }

            if (_playerOxygen != null)
            {
                _playerOxygen.OnOxygenChanged -= HandleOxygenChanged;
            }

            if (_dataManager != null)
            {
                _dataManager.OnMissionListChanged -= RebuildMissionList;
                _dataManager.OnMissionSlotUpdated -= HandleMissionSlotUpdated;
                _dataManager.OnGunChanged -= HandleGunChanged;
            }
        }

        void HandleTimeChanged(string val)
        {
            if (Time != null) Time.text = val;
        }

        void HandleStepsChanged(int val)
        {
            if (Steps != null) Steps.text = val.ToString();
        }

        void HandleRoundChanged(int val)
        {
            if (Round != null) Round.text = "ROUND " + val.ToString();
        }

        void HandleOxygenChanged(float cur, float max)
        {
            float norm = (max <= 0f) ? 0f : Mathf.Clamp01(cur / max);

            if (oxygen != null)
                oxygen.value = norm;

            if (oxygenText != null)
            {
                int secLeft = Mathf.CeilToInt(cur);
                if (secLeft < 0) secLeft = 0;
                oxygenText.text = secLeft.ToString();
            }
        }

        protected override void CacheStartPositions()
        {
            if (_initializedPos) return;
            if (RightPanel.panel) _rightStart = RightPanel.panel.anchoredPosition;
            if (LeftPanel.panel) _leftStart = LeftPanel.panel.anchoredPosition;
            _initializedPos = true;
        }

        public override IEnumerator Show(object ctx = null)
        {
            CacheStartPositions();

            Vector2 rFrom = GetOffscreenPos(RightPanel.panel, RightPanel.slideDir, _rightStart, offscreenPadding);
            Vector2 lFrom = GetOffscreenPos(LeftPanel.panel, LeftPanel.slideDir, _leftStart, offscreenPadding);
            Vector2 tFrom = GetOffscreenPos(TopPanel.panel, TopPanel.slideDir, _topStart, offscreenPadding);

            yield return ShowMovePanels(
                duration, showEase, 1f, 1f,
                (RightPanel.panel, rFrom, _rightStart),
                (LeftPanel.panel, lFrom, _leftStart),
                (TopPanel.panel, tFrom, _topStart)
            );
        }

        public override IEnumerator Hide()
        {
            CacheStartPositions();

            Vector2 rTo = GetOffscreenPos(RightPanel.panel, RightPanel.slideDir, _rightStart, offscreenPadding);
            Vector2 lTo = GetOffscreenPos(LeftPanel.panel, LeftPanel.slideDir, _leftStart, offscreenPadding);
            Vector2 tTo = GetOffscreenPos(TopPanel.panel, TopPanel.slideDir, _topStart, offscreenPadding);

            yield return HideMovePanels(
                duration, hideEase, 1f, 1f,
                (RightPanel.panel, _rightStart, rTo),
                (LeftPanel.panel, _leftStart, lTo),
                (TopPanel.panel, _topStart, tTo)
            );
        }

        void RebuildMissionList()
        {
            foreach (var cell in _spawnedMissionCells)
            {
                if (cell != null)
                    Destroy(cell.gameObject);
            }
            _spawnedMissionCells.Clear();

            if (_dataManager == null || _dataManager.Slots == null) return;
            if (missionItemPrefab == null || spawnMissionContainer == null) return;

            var slots = _dataManager.Slots;
            for (int i = 0; i < slots.Count; i++)
            {
                bJakGZQ3_Item slot = slots[i];
                var cell = Instantiate(missionItemPrefab, spawnMissionContainer);
                cell.Init(slot);
                _spawnedMissionCells.Add(cell);
            }
        }

        void HandleMissionSlotUpdated(int index, bJakGZQ3_Item slot)
        {
            if (index < 0 || index >= _spawnedMissionCells.Count) return;
            var uiCell = _spawnedMissionCells[index];
            if (uiCell != null)
            {
                uiCell.RefreshProgress(slot);
            }
        }

        public void HandleGunChanged(int gunAvailable)
        {
            if (gunUI == null || gunUI.Length == 0) return;

            for (int i = 0; i < gunUI.Length; i++)
            {
                if (gunUI[i] == null) continue;

                if (i < gunAvailable)
                {
                    gunUI[i].GunAvalable();
                }
                else
                {
                    gunUI[i].GunNotAvalable();
                }
            }
        }
    }
}
