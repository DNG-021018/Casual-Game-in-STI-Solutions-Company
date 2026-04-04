#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace CataFury
{
    [CustomEditor(typeof(CF_DailyRewardManager))]
    public class CF_DailyRewardManagerEditor : Editor
    {
        // ── Serialized Properties ──────────────────────────────────────────
        private SerializedProperty _currencyManagerProp;
        private SerializedProperty _dailyRewardProp;

        // ── ReorderableList ───────────────────────────────────────────────
        private ReorderableList _rewardList;

        // ── Foldout States ─────────────────────────────────────────────────
        private bool _showRewardList = true;
        private bool _showSetup = true;
        private bool _showSaveData = false;

        // ── Styles (lazy-init) ─────────────────────────────────────────────
        private GUIStyle _headerStyle;
        private GUIStyle _sectionLabelStyle;
        private bool _stylesReady;

        // ── Colours ────────────────────────────────────────────────────────
        private static readonly Color ColBg = new Color(0.16f, 0.16f, 0.20f);
        private static readonly Color ColSection = new Color(0.20f, 0.20f, 0.26f);
        private static readonly Color ColCard = new Color(0.23f, 0.23f, 0.30f);
        private static readonly Color ColCardAlt = new Color(0.20f, 0.20f, 0.27f);
        private static readonly Color ColActive = new Color(0.18f, 0.62f, 0.32f);
        private static readonly Color ColClaimed = new Color(0.36f, 0.36f, 0.40f);
        private static readonly Color ColDanger = new Color(0.78f, 0.22f, 0.22f);
        private static readonly Color ColAccent = new Color(0.20f, 0.48f, 0.88f);
        private static readonly Color ColGold = new Color(0.95f, 0.78f, 0.20f);
        private static readonly Color ColText = new Color(0.90f, 0.90f, 0.90f);
        private static readonly Color ColMuted = new Color(0.55f, 0.55f, 0.60f);

        // ── Constants ──────────────────────────────────────────────────────
        private const float CARD_HEIGHT = 64f;
        private const float ICON_SIZE = 48f;

        // ──────────────────────────────────────────────────────────────────
        private void OnEnable()
        {
            _currencyManagerProp = serializedObject.FindProperty("_currencyManager");
            _dailyRewardProp = serializedObject.FindProperty("dailyReward");
            BuildReorderableList();
        }

        // ──────────────────────────────────────────────────────────────────
        public override void OnInspectorGUI()
        {
            InitStyles();
            serializedObject.Update();

            DrawHeader();
            GUILayout.Space(6);

            DrawReferences();
            GUILayout.Space(6);

            // ── Reward List ───────────────────────────────────────────────
            _showRewardList = DrawFoldout("🎁  Daily Rewards", _showRewardList);
            if (_showRewardList)
            {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    GUILayout.Space(4);
                    DrawDayPreviewStrip();
                    GUILayout.Space(6);
                    _rewardList.DoLayoutList();
                    GUILayout.Space(2);
                }
            }

            GUILayout.Space(6);

            // ── Setup & Debug ─────────────────────────────────────────────
            _showSetup = DrawFoldout("🛠  Setup & Debug", _showSetup);
            if (_showSetup)
            {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    GUILayout.Space(4);
                    DrawSetupAndDebug();
                    GUILayout.Space(2);
                }
            }

            GUILayout.Space(6);

            // ── Raw Save Data ─────────────────────────────────────────────
            _showSaveData = DrawFoldout("💾  Raw Save Data", _showSaveData);
            if (_showSaveData)
            {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    GUILayout.Space(4);
                    DrawRawSaveData();
                    GUILayout.Space(2);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        // ══════════════════════════════════════════════════════════════════
        //  REWARD LIST
        // ══════════════════════════════════════════════════════════════════

        private void BuildReorderableList()
        {
            _rewardList = new ReorderableList(serializedObject, _dailyRewardProp,
                draggable: true, displayHeader: true,
                displayAddButton: true, displayRemoveButton: false)
            {
                headerHeight = 24f,
                elementHeight = CARD_HEIGHT + 6f,
            };

            // Header
            _rewardList.drawHeaderCallback = rect =>
            {
                EditorGUI.DrawRect(rect, ColSection);
                GUI.Label(new Rect(rect.x + 6, rect.y + 4, 200, 18), "Reward Entries", EditorStyles.boldLabel);
                GUI.Label(new Rect(rect.xMax - 60, rect.y + 4, 56, 18),
                    $"{_dailyRewardProp.arraySize} / 7",
                    new GUIStyle(EditorStyles.miniLabel)
                    { alignment = TextAnchor.MiddleRight, normal = { textColor = ColMuted } });
            };

            // Element (card)
            _rewardList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                var prop = _dailyRewardProp.GetArrayElementAtIndex(index);
                var iconProp = prop.FindPropertyRelative("icon");
                var dayProp = prop.FindPropertyRelative("day");
                var amountProp = prop.FindPropertyRelative("rewardAmount");

                // Card background
                var cardRect = new Rect(rect.x, rect.y + 3, rect.width, CARD_HEIGHT);
                EditorGUI.DrawRect(cardRect, index % 2 == 0 ? ColCard : ColCardAlt);

                // Left accent bar  (gold on day 7, blue otherwise)
                EditorGUI.DrawRect(new Rect(cardRect.x, cardRect.y, 4, cardRect.height),
                    index == 6 ? ColGold : ColAccent);

                // ── Icon area ─────────────────────────────────────────────
                var iconFieldRect = new Rect(cardRect.x + 10, cardRect.y + 8, ICON_SIZE, ICON_SIZE);

                var sprite = iconProp.objectReferenceValue as Sprite;
                if (sprite != null)
                {
                    var tex = AssetPreview.GetAssetPreview(sprite);
                    if (tex != null)
                        EditorGUI.DrawPreviewTexture(iconFieldRect, tex, null, ScaleMode.ScaleToFit);
                    else
                        EditorGUI.DrawRect(iconFieldRect, new Color(0.3f, 0.3f, 0.3f));
                }
                else
                {
                    EditorGUI.DrawRect(iconFieldRect, new Color(0.26f, 0.26f, 0.32f));
                    GUI.Label(new Rect(iconFieldRect.x + 13, iconFieldRect.y + 13, 22, 22),
                        "🖼", new GUIStyle(GUI.skin.label) { fontSize = 16 });
                }

                // Invisible object-field overlay for drag&drop onto the icon preview
                iconProp.objectReferenceValue = EditorGUI.ObjectField(
                    iconFieldRect, GUIContent.none,
                    iconProp.objectReferenceValue, typeof(Sprite), false);

                // ── Fields (right of icon) ────────────────────────────────
                float fx = cardRect.x + ICON_SIZE + 18;
                float fw = cardRect.width - ICON_SIZE - 90;

                // Day field
                EditorGUI.LabelField(new Rect(fx, cardRect.y + 8, 48, 16), "Day", MutedLabel());
                dayProp.intValue = EditorGUI.IntField(new Rect(fx + 50, cardRect.y + 8, fw - 50, 16), dayProp.intValue);

                // Reward amount
                EditorGUI.LabelField(new Rect(fx, cardRect.y + 30, 48, 16), "Coins", MutedLabel());
                var prev = GUI.contentColor;
                GUI.contentColor = ColGold;
                amountProp.intValue = EditorGUI.IntField(new Rect(fx + 50, cardRect.y + 30, fw - 50, 16), amountProp.intValue);
                GUI.contentColor = prev;

                // Hint
                GUI.Label(new Rect(fx, cardRect.y + 50, fw, 12),
                    "↑ drag sprite onto icon to assign",
                    new GUIStyle(EditorStyles.miniLabel) { fontSize = 8, normal = { textColor = new Color(0.38f, 0.38f, 0.42f) } });

                // ── Delete button ─────────────────────────────────────────
                var prevBg = GUI.backgroundColor;
                GUI.backgroundColor = isActive ? ColDanger : new Color(0.50f, 0.18f, 0.18f, 0.75f);
                if (GUI.Button(new Rect(cardRect.xMax - 26, cardRect.y + (CARD_HEIGHT / 2f) - 11, 22, 22),
                    new GUIContent("✕", "Remove this reward")))
                {
                    if (EditorUtility.DisplayDialog("Remove Reward",
                        $"Remove the reward for Day {index + 1}?", "Remove", "Cancel"))
                    {
                        _dailyRewardProp.DeleteArrayElementAtIndex(index);
                        serializedObject.ApplyModifiedProperties();
                    }
                }
                GUI.backgroundColor = prevBg;
            };

            // Add button
            _rewardList.onAddCallback = list =>
            {
                int ni = _dailyRewardProp.arraySize;
                _dailyRewardProp.arraySize++;
                var np = _dailyRewardProp.GetArrayElementAtIndex(ni);
                np.FindPropertyRelative("day").intValue = ni + 1;
                np.FindPropertyRelative("rewardAmount").intValue = 0;
                np.FindPropertyRelative("icon").objectReferenceValue = null;
                serializedObject.ApplyModifiedProperties();
            };
        }

        // ── 7-Day Preview Strip ────────────────────────────────────────────
        private void DrawDayPreviewStrip()
        {
            var manager = (CF_DailyRewardManager)target;
            var rewards = manager.GetDailyRewards();
            var data = LoadSaveData();

            EditorGUILayout.LabelField("Quick Preview", MutedLabel());
            GUILayout.Space(2);

            using (new EditorGUILayout.HorizontalScope())
            {
                for (int i = 0; i < 7; i++)
                {
                    bool hasCfg = rewards != null && i < rewards.Count;
                    int amount = hasCfg ? rewards[i].rewardAmount : 0;
                    bool current = data != null && data.currentDay == i + 1;
                    bool claimed = data != null && data.claimedDays != null
                                   && i < data.claimedDays.Length && data.claimedDays[i];

                    Color bg = claimed ? ColClaimed : current ? ColActive : ColCard;
                    var rect = GUILayoutUtility.GetRect(0, 56, GUILayout.ExpandWidth(true));
                    EditorGUI.DrawRect(rect, bg);

                    // top accent
                    EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, 3),
                        current ? ColGold : claimed ? ColMuted : (i == 6 ? ColGold : ColAccent));

                    // icon or emoji
                    if (hasCfg && rewards[i].icon != null)
                    {
                        var tex = AssetPreview.GetAssetPreview(rewards[i].icon);
                        if (tex != null)
                        {
                            float s = Mathf.Min(rect.width - 4, 26);
                            float ix = rect.x + (rect.width - s) / 2f;
                            EditorGUI.DrawPreviewTexture(new Rect(ix, rect.y + 5, s, s), tex, null, ScaleMode.ScaleToFit);
                        }
                    }
                    else
                    {
                        string emoji = claimed ? "✅" : i == 6 ? "⭐" : "🪙";
                        GUI.Label(new Rect(rect.x, rect.y + 3, rect.width, 22), emoji,
                            new GUIStyle(GUI.skin.label) { fontSize = 15, alignment = TextAnchor.MiddleCenter });
                    }

                    GUI.Label(new Rect(rect.x, rect.y + 32, rect.width, 12), $"D{i + 1}",
                        new GUIStyle(EditorStyles.centeredGreyMiniLabel)
                        {
                            normal = { textColor = current ? Color.white : ColMuted },
                            fontStyle = current ? FontStyle.Bold : FontStyle.Normal
                        });

                    GUI.Label(new Rect(rect.x, rect.y + 43, rect.width, 12),
                        amount > 0 ? $"+{amount}" : "—",
                        new GUIStyle(EditorStyles.centeredGreyMiniLabel)
                        { normal = { textColor = current ? ColGold : ColMuted }, fontStyle = FontStyle.Bold });

                    if (i < 6) GUILayout.Space(2);
                }
            }
        }

        // ══════════════════════════════════════════════════════════════════
        //  SETUP & DEBUG  (edit mode + play mode)
        // ══════════════════════════════════════════════════════════════════

        private void DrawSetupAndDebug()
        {
            var manager = (CF_DailyRewardManager)target;
            var data = LoadSaveData();

            // ── Status banner ─────────────────────────────────────────────
            bool canClaim = data?.canClaimToday ?? false;
            var bannerBg = canClaim ? new Color(0.12f, 0.38f, 0.18f) : new Color(0.32f, 0.18f, 0.18f);
            var bannerRect = GUILayoutUtility.GetRect(0, data != null ? 58 : 30, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(bannerRect, bannerBg);

            if (data != null)
            {
                string lastLogin = data.lastLoginTimestamp == 0 ? "Never"
                    : DateTime.FromBinary(data.lastLoginTimestamp).ToString("yyyy-MM-dd HH:mm");

                GUI.Label(new Rect(bannerRect.x + 8, bannerRect.y + 5, bannerRect.width - 12, 18),
                    $"{(canClaim ? "🟢" : "🔴")}  {(canClaim ? "Can claim today" : "Already claimed / not available")}",
                    new GUIStyle(EditorStyles.boldLabel) { normal = { textColor = Color.white } });
                GUI.Label(new Rect(bannerRect.x + 8, bannerRect.y + 24, bannerRect.width - 12, 14),
                    $"Current Day: {data.currentDay}   •   Total Logins: {data.loginDayCount}   •   Last Login: {lastLogin}",
                    new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = ColMuted } });
                GUI.Label(new Rect(bannerRect.x + 8, bannerRect.y + 40, bannerRect.width - 12, 14),
                    Application.isPlaying ? "▶  Play Mode — live data" : "✏️  Edit Mode — writes directly to PlayerPrefs",
                    new GUIStyle(EditorStyles.miniLabel)
                    { normal = { textColor = Application.isPlaying ? new Color(0.5f, 1f, 0.5f) : new Color(0.9f, 0.8f, 0.3f) } });
            }
            else
            {
                GUI.Label(new Rect(bannerRect.x + 8, bannerRect.y + 6, bannerRect.width - 12, 18),
                    "⚪  No save data found",
                    new GUIStyle(EditorStyles.boldLabel) { normal = { textColor = Color.white } });
            }

            GUILayout.Space(10);

            // ── Set Current Day ───────────────────────────────────────────
            EditorGUILayout.LabelField("Set Current Day", _sectionLabelStyle);
            GUILayout.Space(3);
            using (new EditorGUILayout.HorizontalScope())
            {
                for (int d = 1; d <= 7; d++)
                {
                    bool isThis = data != null && data.currentDay == d;
                    GUI.backgroundColor = isThis ? ColActive : ColCard;
                    if (GUILayout.Button(new GUIContent($"Day {d}", $"Jump save data to Day {d}"),
                        GUILayout.Height(30)))
                    {
                        Undo.RecordObject(manager, $"Set Day {d}");
                        WriteDay(d);
                        if (Application.isPlaying) manager.Init();
                        Repaint();
                    }
                }
                GUI.backgroundColor = Color.white;
            }

            GUILayout.Space(8);

            // ── Claim State Override ──────────────────────────────────────
            EditorGUILayout.LabelField("Claim State Override", _sectionLabelStyle);
            GUILayout.Space(3);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUI.backgroundColor = canClaim ? ColActive : new Color(0.25f, 0.45f, 0.28f);
                if (GUILayout.Button("✅  Enable Claim", GUILayout.Height(28)))
                {
                    WriteCanClaim(true);
                    if (Application.isPlaying) manager.Init();
                    Repaint();
                }

                GUI.backgroundColor = !canClaim ? ColDanger : new Color(0.45f, 0.22f, 0.22f);
                if (GUILayout.Button("🔒  Disable Claim", GUILayout.Height(28)))
                {
                    WriteCanClaim(false);
                    if (Application.isPlaying) manager.Init();
                    Repaint();
                }
                GUI.backgroundColor = Color.white;
            }

            GUILayout.Space(8);

            // ── Simulate Day Skip ─────────────────────────────────────────
            EditorGUILayout.LabelField("Simulate Day Skip", _sectionLabelStyle);
            GUILayout.Space(3);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUI.backgroundColor = ColAccent;
                if (GUILayout.Button("⏩  +1 Day", GUILayout.Height(28))) SimulateDaySkip(manager, 1);
                if (GUILayout.Button("⏩  +2 Days", GUILayout.Height(28))) SimulateDaySkip(manager, 2);
                if (GUILayout.Button("⏩  +3 Days", GUILayout.Height(28))) SimulateDaySkip(manager, 3);

                GUI.backgroundColor = ColDanger;
                if (GUILayout.Button("💀  +7 Days\n(streak break)", GUILayout.Height(36)))
                    SimulateDaySkip(manager, 7);

                GUI.backgroundColor = Color.white;
            }

            GUILayout.Space(8);

            // ── Play Mode: Claim ──────────────────────────────────────────
            if (Application.isPlaying)
            {
                EditorGUILayout.LabelField("Actions (Play Mode)", _sectionLabelStyle);
                GUILayout.Space(3);

                GUI.enabled = manager.CanClaimToday();
                GUI.backgroundColor = manager.CanClaimToday() ? ColActive : ColClaimed;
                if (GUILayout.Button("🎁  Claim Today's Reward", GUILayout.Height(30)))
                {
                    Debug.Log($"[DailyReward] Claim: {(manager.ClaimTodayReward() ? "SUCCESS" : "FAILED")}");
                    Repaint();
                }
                GUI.enabled = true;
                GUI.backgroundColor = Color.white;
                GUILayout.Space(6);
            }

            // ── Init / Reset ──────────────────────────────────────────────
            using (new EditorGUILayout.HorizontalScope())
            {
                GUI.backgroundColor = ColAccent;
                if (GUILayout.Button("🔁  Init / Reload", GUILayout.Height(30)))
                {
                    if (Application.isPlaying) manager.Init();
                    Repaint();
                }

                GUI.backgroundColor = ColDanger;
                if (GUILayout.Button("🗑  Reset All Progress", GUILayout.Height(30)))
                {
                    if (EditorUtility.DisplayDialog("Reset Progress",
                        "Delete ALL saved daily reward data?\n(claimed days, streak, last login)",
                        "Yes, Reset", "Cancel"))
                    {
                        PlayerPrefs.DeleteKey(CF_SafetyKey.Data.DAILY_REWARD_SAVE_KEY);
                        PlayerPrefs.Save();
                        if (Application.isPlaying) { manager.ResetAllData(); manager.Init(); }
                        Debug.Log("[DailyReward] Progress reset.");
                        Repaint();
                    }
                }
                GUI.backgroundColor = Color.white;
            }
        }

        // ══════════════════════════════════════════════════════════════════
        //  RAW SAVE DATA
        // ══════════════════════════════════════════════════════════════════

        private void DrawRawSaveData()
        {
            var data = LoadSaveData();
            if (data == null) { EditorGUILayout.HelpBox("No save data in PlayerPrefs.", MessageType.Warning); return; }

            GUI.enabled = false;
            EditorGUILayout.IntField("currentDay", data.currentDay);
            EditorGUILayout.IntField("loginDayCount", data.loginDayCount);
            EditorGUILayout.Toggle("canClaimToday", data.canClaimToday);
            EditorGUILayout.TextField("lastLogin",
                data.lastLoginTimestamp == 0 ? "Never"
                : DateTime.FromBinary(data.lastLoginTimestamp).ToString("yyyy-MM-dd HH:mm:ss"));

            EditorGUILayout.LabelField("claimedDays");
            EditorGUI.indentLevel++;
            for (int i = 0; i < (data.claimedDays?.Length ?? 0); i++)
                EditorGUILayout.Toggle($"Day {i + 1}", data.claimedDays[i]);
            EditorGUI.indentLevel--;
            GUI.enabled = true;
        }

        // ══════════════════════════════════════════════════════════════════
        //  HEADER + REFERENCES
        // ══════════════════════════════════════════════════════════════════

        private new void DrawHeader()
        {
            var rect = GUILayoutUtility.GetRect(0, 44, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, ColBg);
            GUI.Label(new Rect(rect.x + 8, rect.y + 6, 30, 30), "📅",
                new GUIStyle(GUI.skin.label) { fontSize = 22 });
            GUI.Label(new Rect(rect.x + 44, rect.y + 5, rect.width - 50, 20),
                "Daily Reward Manager", _headerStyle);
            GUI.Label(new Rect(rect.x + 44, rect.y + 26, rect.width - 50, 14),
                "CataFury  •  7-Day Streak System",
                new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = ColMuted } });
        }

        private void DrawReferences()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("References", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(_currencyManagerProp, new GUIContent("Currency Manager"));
            }
        }

        // ══════════════════════════════════════════════════════════════════
        //  PLAYERPREFS HELPERS
        // ══════════════════════════════════════════════════════════════════

        private DailyRewardData LoadSaveData()
        {
            if (Application.isPlaying)
            {
                var live = ((CF_DailyRewardManager)target).GetSaveData();
                if (live != null) return live;
            }
            string json = PlayerPrefs.GetString(CF_SafetyKey.Data.DAILY_REWARD_SAVE_KEY, "");
            return string.IsNullOrEmpty(json) ? null : JsonUtility.FromJson<DailyRewardData>(json);
        }

        private void WriteSaveData(DailyRewardData data)
        {
            PlayerPrefs.SetString(CF_SafetyKey.Data.DAILY_REWARD_SAVE_KEY, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
            if (Application.isPlaying)
            {
                var live = ((CF_DailyRewardManager)target).GetSaveData();
                if (live != null)
                {
                    live.currentDay = data.currentDay;
                    live.loginDayCount = data.loginDayCount;
                    live.canClaimToday = data.canClaimToday;
                    live.lastLoginTimestamp = data.lastLoginTimestamp;
                    live.claimedDays = data.claimedDays;
                }
            }
        }

        private void WriteDay(int day)
        {
            var data = LoadSaveData() ?? new DailyRewardData();
            data.currentDay = Mathf.Clamp(day, 1, 7);
            data.canClaimToday = data.claimedDays == null || data.currentDay - 1 >= data.claimedDays.Length
                ? true : !data.claimedDays[data.currentDay - 1];
            WriteSaveData(data);
        }

        private void WriteCanClaim(bool value)
        {
            var data = LoadSaveData() ?? new DailyRewardData();
            data.canClaimToday = value;
            WriteSaveData(data);
        }

        private void SimulateDaySkip(CF_DailyRewardManager manager, int days)
        {
            var data = LoadSaveData() ?? new DailyRewardData();
            data.lastLoginTimestamp = DateTime.Now.AddDays(-days).ToBinary();
            data.canClaimToday = true;
            WriteSaveData(data);
            if (Application.isPlaying) manager.Init();
            Debug.Log($"[DailyReward] Simulated -{days} day(s) on lastLoginTimestamp.");
            EditorUtility.SetDirty(manager);
            Repaint();
        }

        // ══════════════════════════════════════════════════════════════════
        //  UTILITIES
        // ══════════════════════════════════════════════════════════════════

        private bool DrawFoldout(string label, bool expanded)
        {
            var rect = GUILayoutUtility.GetRect(0, 24, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, ColSection);
            return EditorGUI.Foldout(
                new Rect(rect.x + 4, rect.y + 4, rect.width - 8, rect.height - 4),
                expanded, label, true,
                new GUIStyle(EditorStyles.foldout)
                {
                    fontStyle = FontStyle.Bold,
                    normal = { textColor = ColText },
                    onNormal = { textColor = ColText }
                });
        }

        private static GUIStyle MutedLabel() =>
            new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = ColMuted } };

        private void InitStyles()
        {
            if (_stylesReady) return;
            _headerStyle = new GUIStyle(EditorStyles.boldLabel)
            { fontSize = 14, normal = { textColor = Color.white } };
            _sectionLabelStyle = new GUIStyle(EditorStyles.boldLabel)
            { fontSize = 11, normal = { textColor = new Color(0.75f, 0.85f, 1f) } };
            _stylesReady = true;
        }
    }
}
#endif