#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace CataFury
{
    [CustomEditor(typeof(CF_CurrencyManager))]
    public class CF_CurrencyManagerEditor : Editor
    {
        // ── Serialized Properties ──────────────────────────────────────────
        private SerializedProperty _defaultCoinsProp;

        // ── Foldout States ─────────────────────────────────────────────────
        private bool _showConfig = true;
        private bool _showDebug = true;

        // ── Editor State ───────────────────────────────────────────────────
        private int _addAmount = 100;
        private int _spendAmount = 50;
        private int _setAmount = 0;
        private bool _setInit = false;

        // ── Styles ─────────────────────────────────────────────────────────
        private GUIStyle _headerStyle;
        private GUIStyle _sectionLabelStyle;
        private GUIStyle _bigCoinStyle;
        private bool _stylesReady;

        // ── Colours ────────────────────────────────────────────────────────
        private static readonly Color ColBg = new Color(0.14f, 0.13f, 0.10f);
        private static readonly Color ColSection = new Color(0.20f, 0.18f, 0.13f);
        private static readonly Color ColCard = new Color(0.22f, 0.20f, 0.14f);
        private static readonly Color ColGreen = new Color(0.16f, 0.55f, 0.26f);
        private static readonly Color ColDanger = new Color(0.75f, 0.20f, 0.20f);
        private static readonly Color ColAccent = new Color(0.20f, 0.46f, 0.86f);
        private static readonly Color ColGold = new Color(0.95f, 0.78f, 0.18f);
        private static readonly Color ColGoldDark = new Color(0.60f, 0.46f, 0.05f);
        private static readonly Color ColText = new Color(0.92f, 0.90f, 0.86f);
        private static readonly Color ColMuted = new Color(0.55f, 0.53f, 0.48f);

        // ──────────────────────────────────────────────────────────────────
        private void OnEnable()
        {
            _defaultCoinsProp = serializedObject.FindProperty("defaultCoins");
        }

        public override void OnInspectorGUI()
        {
            InitStyles();
            serializedObject.Update();

            DrawHeader();
            GUILayout.Space(6);

            // ── Coin Display ──────────────────────────────────────────────
            DrawCoinDisplay();
            GUILayout.Space(6);

            // ── Config ────────────────────────────────────────────────────
            _showConfig = DrawFoldout("⚙️  Config", _showConfig);
            if (_showConfig)
            {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    GUILayout.Space(4);
                    DrawConfig();
                    GUILayout.Space(2);
                }
            }

            GUILayout.Space(6);

            // ── Debug ─────────────────────────────────────────────────────
            _showDebug = DrawFoldout("🛠  Debug & Tools", _showDebug);
            if (_showDebug)
            {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    GUILayout.Space(4);
                    DrawDebug();
                    GUILayout.Space(2);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        // ══════════════════════════════════════════════════════════════════
        //  COIN DISPLAY
        // ══════════════════════════════════════════════════════════════════

        private void DrawCoinDisplay()
        {
            int coins = ReadCoinsFromPrefs();

            // Big coin card
            var cardRect = GUILayoutUtility.GetRect(0, 72, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(cardRect, ColCard);

            // Gold left bar
            EditorGUI.DrawRect(new Rect(cardRect.x, cardRect.y, 5, cardRect.height), ColGold);

            // Coin emoji
            GUI.Label(new Rect(cardRect.x + 14, cardRect.y + 10, 48, 48),
                "🪙", new GUIStyle(GUI.skin.label) { fontSize = 36 });

            // Amount
            GUI.Label(new Rect(cardRect.x + 66, cardRect.y + 8, cardRect.width - 80, 38),
                coins.ToString("N0"),
                new GUIStyle(GUI.skin.label)
                {
                    fontSize = 30,
                    fontStyle = FontStyle.Bold,
                    normal = { textColor = ColGold },
                    alignment = TextAnchor.MiddleLeft
                });

            // Sub label
            string subLabel = Application.isPlaying ? "▶ Live" : "✏️ Edit Mode";
            Color subColor = Application.isPlaying ? new Color(0.4f, 1f, 0.5f) : new Color(0.9f, 0.8f, 0.3f);
            GUI.Label(new Rect(cardRect.x + 66, cardRect.y + 50, cardRect.width - 80, 16),
                $"coins  •  {subLabel}",
                new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = subColor } });
        }

        // ══════════════════════════════════════════════════════════════════
        //  CONFIG
        // ══════════════════════════════════════════════════════════════════

        private void DrawConfig()
        {
            EditorGUILayout.LabelField("Default Coins", _sectionLabelStyle);
            GUILayout.Space(2);
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Starting amount:", GUILayout.Width(120));
                var prevCC = GUI.contentColor;
                GUI.contentColor = ColGold;
                EditorGUILayout.PropertyField(_defaultCoinsProp, GUIContent.none);
                GUI.contentColor = prevCC;
            }
            EditorGUILayout.HelpBox("This value is only applied when Init() is called and no saved data exists.", MessageType.Info);
        }

        // ══════════════════════════════════════════════════════════════════
        //  DEBUG & TOOLS
        // ══════════════════════════════════════════════════════════════════

        private void DrawDebug()
        {
            var manager = (CF_CurrencyManager)target;

            // ── Add Coins ─────────────────────────────────────────────────
            EditorGUILayout.LabelField("➕  Add Coins", _sectionLabelStyle);
            GUILayout.Space(3);

            using (new EditorGUILayout.HorizontalScope())
            {
                var prevCC = GUI.contentColor;
                GUI.contentColor = ColGold;
                _addAmount = EditorGUILayout.IntField(_addAmount, GUILayout.Width(80));
                GUI.contentColor = prevCC;

                // Quick presets
                GUI.backgroundColor = new Color(0.28f, 0.28f, 0.18f);
                foreach (int preset in new[] { 50, 100, 500, 1000 })
                {
                    if (GUILayout.Button($"+{preset}", GUILayout.Height(22)))
                        _addAmount = preset;
                }
                GUI.backgroundColor = Color.white;
            }

            GUILayout.Space(3);
            GUI.backgroundColor = ColGreen;
            if (GUILayout.Button($"➕  Add {_addAmount:N0} Coins", GUILayout.Height(30)))
            {
                ModifyCoins(manager, _addAmount, add: true);
                Repaint();
            }
            GUI.backgroundColor = Color.white;

            GUILayout.Space(8);

            // ── Spend Coins ───────────────────────────────────────────────
            EditorGUILayout.LabelField("➖  Spend Coins", _sectionLabelStyle);
            GUILayout.Space(3);

            using (new EditorGUILayout.HorizontalScope())
            {
                var prevCC = GUI.contentColor;
                GUI.contentColor = new Color(1f, 0.6f, 0.6f);
                _spendAmount = EditorGUILayout.IntField(_spendAmount, GUILayout.Width(80));
                GUI.contentColor = prevCC;

                GUI.backgroundColor = new Color(0.30f, 0.20f, 0.20f);
                foreach (int preset in new[] { 10, 50, 100, 500 })
                {
                    if (GUILayout.Button($"-{preset}", GUILayout.Height(22)))
                        _spendAmount = preset;
                }
                GUI.backgroundColor = Color.white;
            }

            GUILayout.Space(3);
            int currentCoins = ReadCoinsFromPrefs();
            bool canSpend = currentCoins >= _spendAmount;
            GUI.enabled = canSpend;
            GUI.backgroundColor = canSpend ? ColDanger : new Color(0.4f, 0.25f, 0.25f);
            if (GUILayout.Button($"➖  Spend {_spendAmount:N0} Coins"
                + (canSpend ? "" : "  (not enough)"), GUILayout.Height(30)))
            {
                ModifyCoins(manager, _spendAmount, add: false);
                Repaint();
            }
            GUI.enabled = true;
            GUI.backgroundColor = Color.white;

            GUILayout.Space(8);

            // ── Set Exact Amount ──────────────────────────────────────────
            EditorGUILayout.LabelField("✏️  Set Exact Amount", _sectionLabelStyle);
            GUILayout.Space(3);

            if (!_setInit) { _setAmount = currentCoins; _setInit = true; }

            using (new EditorGUILayout.HorizontalScope())
            {
                var prevCC = GUI.contentColor;
                GUI.contentColor = ColGold;
                _setAmount = EditorGUILayout.IntField(_setAmount);
                GUI.contentColor = prevCC;

                GUI.backgroundColor = ColAccent;
                if (GUILayout.Button("Set", GUILayout.Width(48), GUILayout.Height(22)))
                {
                    SetCoins(manager, Mathf.Max(0, _setAmount));
                    Repaint();
                }
                GUI.backgroundColor = Color.white;
            }

            GUILayout.Space(8);

            // ── Reset ─────────────────────────────────────────────────────
            EditorGUILayout.LabelField("⚠️  Danger Zone", _sectionLabelStyle);
            GUILayout.Space(3);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUI.backgroundColor = ColDanger;
                if (GUILayout.Button("🗑  Reset to Default (" + _defaultCoinsProp.intValue + ")", GUILayout.Height(30)))
                {
                    if (EditorUtility.DisplayDialog("Reset Currency",
                        $"Reset coins to default value ({_defaultCoinsProp.intValue})?",
                        "Yes, Reset", "Cancel"))
                    {
                        SetCoins(manager, _defaultCoinsProp.intValue);
                        _setInit = false;
                        if (Application.isPlaying) manager.Init();
                        Debug.Log("[Currency] Coins reset to default.");
                        Repaint();
                    }
                }

                if (GUILayout.Button("🗑  Reset to 0", GUILayout.Height(30)))
                {
                    if (EditorUtility.DisplayDialog("Reset Currency",
                        "Set coins to 0?", "Yes", "Cancel"))
                    {
                        SetCoins(manager, 0);
                        _setInit = false;
                        if (Application.isPlaying) manager.Init();
                        Repaint();
                    }
                }
                GUI.backgroundColor = Color.white;
            }
        }

        // ══════════════════════════════════════════════════════════════════
        //  PLAYERPREFS HELPERS
        // ══════════════════════════════════════════════════════════════════

        private int ReadCoinsFromPrefs()
        {
            if (Application.isPlaying)
                return ((CF_CurrencyManager)target).GetCoins();

            string json = PlayerPrefs.GetString(CF_SafetyKey.Data.COIN_SAVE_KEY, "");
            if (string.IsNullOrEmpty(json)) return 0;
            return JsonUtility.FromJson<CF_CurrencyManager.CurrencyRuntimeData>(json).coins;
        }

        private void WriteCoinsToPrefs(int coins)
        {
            var data = new CF_CurrencyManager.CurrencyRuntimeData { coins = coins };
            PlayerPrefs.SetString(CF_SafetyKey.Data.COIN_SAVE_KEY, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }

        private void ModifyCoins(CF_CurrencyManager manager, int amount, bool add)
        {
            if (Application.isPlaying)
            {
                if (add) manager.AddCoins(amount);
                else manager.SpendCoins(amount);
            }
            else
            {
                int current = ReadCoinsFromPrefs();
                int next = add ? current + amount : Mathf.Max(0, current - amount);
                WriteCoinsToPrefs(next);
                Debug.Log($"[Currency] {(add ? "+" : "-")}{amount} coins → {next}");
            }
        }

        private void SetCoins(CF_CurrencyManager manager, int value)
        {
            if (Application.isPlaying)
            {
                int current = manager.GetCoins();
                int diff = value - current;
                if (diff > 0) manager.AddCoins(diff);
                else if (diff < 0) manager.SpendCoins(-diff);
            }
            else
            {
                WriteCoinsToPrefs(value);
                Debug.Log($"[Currency] Coins set to {value}");
            }
        }

        // ══════════════════════════════════════════════════════════════════
        //  HEADER + UTILITIES
        // ══════════════════════════════════════════════════════════════════

        private new void DrawHeader()
        {
            var rect = GUILayoutUtility.GetRect(0, 44, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, ColBg);
            GUI.Label(new Rect(rect.x + 8, rect.y + 6, 30, 30), "🪙",
                new GUIStyle(GUI.skin.label) { fontSize = 22 });
            GUI.Label(new Rect(rect.x + 44, rect.y + 5, rect.width - 50, 20),
                "Currency Manager", _headerStyle);
            GUI.Label(new Rect(rect.x + 44, rect.y + 26, rect.width - 50, 14),
                "CataFury  •  Coin System",
                new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = ColMuted } });
        }

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
            new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = new Color(0.55f, 0.53f, 0.48f) } };

        private void InitStyles()
        {
            if (_stylesReady) return;
            _headerStyle = new GUIStyle(EditorStyles.boldLabel)
            { fontSize = 14, normal = { textColor = ColText } };
            _sectionLabelStyle = new GUIStyle(EditorStyles.boldLabel)
            { fontSize = 11, normal = { textColor = new Color(0.75f, 0.85f, 1f) } };
            _stylesReady = true;
        }
    }
}
#endif