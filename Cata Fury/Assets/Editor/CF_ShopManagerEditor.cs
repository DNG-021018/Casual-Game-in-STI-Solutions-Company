#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace CataFury
{
    [CustomEditor(typeof(CF_ShopManager))]
    public class CF_ShopManagerEditor : Editor
    {
        // ── Serialized Properties ──────────────────────────────────────────
        private SerializedProperty _listItemsProp;

        // ── ReorderableList ────────────────────────────────────────────────
        private ReorderableList _itemList;

        // ── Foldout States ─────────────────────────────────────────────────
        private bool _showItemList = true;
        private bool _showDebug = true;
        private bool _showSaveData = false;

        // ── Styles ─────────────────────────────────────────────────────────
        private GUIStyle _headerStyle;
        private GUIStyle _sectionLabelStyle;
        private bool _stylesReady;

        // ── Colours ────────────────────────────────────────────────────────
        private static readonly Color ColBg = new Color(0.14f, 0.14f, 0.18f);
        private static readonly Color ColSection = new Color(0.18f, 0.18f, 0.24f);
        private static readonly Color ColCard = new Color(0.21f, 0.21f, 0.28f);
        private static readonly Color ColCardAlt = new Color(0.19f, 0.19f, 0.25f);
        private static readonly Color ColUnlocked = new Color(0.16f, 0.55f, 0.28f);
        private static readonly Color ColEquipped = new Color(0.15f, 0.40f, 0.75f);
        private static readonly Color ColLocked = new Color(0.36f, 0.36f, 0.40f);
        private static readonly Color ColDefault = new Color(0.45f, 0.28f, 0.65f);
        private static readonly Color ColDanger = new Color(0.78f, 0.22f, 0.22f);
        private static readonly Color ColAccent = new Color(0.20f, 0.48f, 0.88f);
        private static readonly Color ColGold = new Color(0.95f, 0.78f, 0.20f);
        private static readonly Color ColText = new Color(0.90f, 0.90f, 0.90f);
        private static readonly Color ColMuted = new Color(0.55f, 0.55f, 0.60f);

        // ── Constants ──────────────────────────────────────────────────────
        private const float CARD_H = 70f;
        private const float ICON_SZ = 52f;

        // ──────────────────────────────────────────────────────────────────
        private void OnEnable()
        {
            _listItemsProp = serializedObject.FindProperty("ListItems");
            BuildItemList();
        }

        // ──────────────────────────────────────────────────────────────────
        public override void OnInspectorGUI()
        {
            InitStyles();
            serializedObject.Update();

            DrawHeader();
            GUILayout.Space(6);

            // ── Item List ─────────────────────────────────────────────────
            _showItemList = DrawFoldout("🛒  Shop Items", _showItemList);
            if (_showItemList)
            {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    GUILayout.Space(4);
                    DrawItemGrid();
                    GUILayout.Space(6);
                    _itemList.DoLayoutList();
                    GUILayout.Space(2);
                }
            }

            GUILayout.Space(6);

            // ── Debug ─────────────────────────────────────────────────────
            _showDebug = DrawFoldout("🛠  Setup & Debug", _showDebug);
            if (_showDebug)
            {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    GUILayout.Space(4);
                    DrawDebug();
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
        //  ITEM LIST (ReorderableList)
        // ══════════════════════════════════════════════════════════════════

        private void BuildItemList()
        {
            _itemList = new ReorderableList(serializedObject, _listItemsProp,
                draggable: true, displayHeader: true,
                displayAddButton: true, displayRemoveButton: false)
            {
                headerHeight = 24f,
                elementHeight = CARD_H + 6f,
            };

            // Header
            _itemList.drawHeaderCallback = rect =>
            {
                EditorGUI.DrawRect(rect, ColSection);
                GUI.Label(new Rect(rect.x + 6, rect.y + 4, 200, 18), "Item Entries", EditorStyles.boldLabel);
                GUI.Label(new Rect(rect.xMax - 50, rect.y + 4, 46, 18),
                    $"{_listItemsProp.arraySize} items",
                    new GUIStyle(EditorStyles.miniLabel)
                    { alignment = TextAnchor.MiddleRight, normal = { textColor = ColMuted } });
            };

            // Element (card)
            _itemList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                var prop = _listItemsProp.GetArrayElementAtIndex(index);
                var idProp = prop.FindPropertyRelative("id");
                var iconProp = prop.FindPropertyRelative("itemIcon");
                var nameProp = prop.FindPropertyRelative("itemName");
                var costProp = prop.FindPropertyRelative("itemCost");
                var defaultProp = prop.FindPropertyRelative("isDefault");

                bool isDefault = defaultProp.boolValue;

                // ── Card background ───────────────────────────────────────
                var card = new Rect(rect.x, rect.y + 3, rect.width, CARD_H);
                EditorGUI.DrawRect(card, index % 2 == 0 ? ColCard : ColCardAlt);

                // Left accent: purple for default, blue otherwise
                EditorGUI.DrawRect(new Rect(card.x, card.y, 4, card.height),
                    isDefault ? ColDefault : ColAccent);

                // ── Icon area ─────────────────────────────────────────────
                var iconRect = new Rect(card.x + 10, card.y + (CARD_H - ICON_SZ) / 2f, ICON_SZ, ICON_SZ);
                var sprite = iconProp.objectReferenceValue as Sprite;
                if (sprite != null)
                {
                    var tex = AssetPreview.GetAssetPreview(sprite);
                    if (tex != null)
                        EditorGUI.DrawPreviewTexture(iconRect, tex, null, ScaleMode.ScaleToFit);
                    else
                        EditorGUI.DrawRect(iconRect, new Color(0.3f, 0.3f, 0.3f));
                }
                else
                {
                    EditorGUI.DrawRect(iconRect, new Color(0.24f, 0.24f, 0.30f));
                    GUI.Label(new Rect(iconRect.x + 14, iconRect.y + 14, 24, 24),
                        "🖼", new GUIStyle(GUI.skin.label) { fontSize = 18 });
                }
                // Drag & drop overlay
                iconProp.objectReferenceValue = EditorGUI.ObjectField(
                    iconRect, GUIContent.none, iconProp.objectReferenceValue, typeof(Sprite), false);

                // ── Fields ────────────────────────────────────────────────
                float fx = card.x + ICON_SZ + 18;
                float fw = card.width - ICON_SZ - 100;
                float row1 = card.y + 7;
                float row2 = card.y + 25;
                float row3 = card.y + 43;
                float row4 = card.y + 57;

                // ID  (enum)
                EditorGUI.LabelField(new Rect(fx, row1, 44, 16), "ID", MutedLabel());
                EditorGUI.PropertyField(new Rect(fx + 46, row1, fw - 46, 16), idProp, GUIContent.none);

                // Name
                EditorGUI.LabelField(new Rect(fx, row2, 44, 16), "Name", MutedLabel());
                nameProp.stringValue = EditorGUI.TextField(new Rect(fx + 46, row2, fw - 46, 16), nameProp.stringValue);

                // Cost  (gold colour)
                EditorGUI.LabelField(new Rect(fx, row3, 44, 16), "Cost", MutedLabel());
                var prevCC = GUI.contentColor;
                GUI.contentColor = isDefault ? ColMuted : ColGold;
                costProp.intValue = EditorGUI.IntField(new Rect(fx + 46, row3, fw - 46, 16), costProp.intValue);
                GUI.contentColor = prevCC;

                // Hint
                GUI.Label(new Rect(fx, row4, fw, 12), "↑ drag sprite onto icon",
                    new GUIStyle(EditorStyles.miniLabel)
                    { fontSize = 8, normal = { textColor = new Color(0.36f, 0.36f, 0.40f) } });

                // ── Default toggle + label ────────────────────────────────
                float toggleX = card.x + ICON_SZ + 18 + fw + 4;
                float toggleW = card.width - (ICON_SZ + 18 + fw + 4) - 32;

                var prevBg = GUI.backgroundColor;
                GUI.backgroundColor = isDefault ? ColDefault : new Color(0.3f, 0.3f, 0.38f);
                var toggleRect = new Rect(toggleX, card.y + 10, toggleW, 20);
                if (GUI.Button(toggleRect, new GUIContent(isDefault ? "✦ Default" : "Default",
                    "Mark this item as the default (free, pre-unlocked)")))
                {
                    // Unset all others if turning on
                    if (!isDefault)
                    {
                        for (int i = 0; i < _listItemsProp.arraySize; i++)
                        {
                            _listItemsProp.GetArrayElementAtIndex(i)
                                .FindPropertyRelative("isDefault").boolValue = false;
                        }
                    }
                    defaultProp.boolValue = !isDefault;
                    serializedObject.ApplyModifiedProperties();
                }
                GUI.backgroundColor = prevBg;

                // ── Delete button ─────────────────────────────────────────
                GUI.backgroundColor = isActive ? ColDanger : new Color(0.48f, 0.16f, 0.16f, 0.80f);
                if (GUI.Button(new Rect(card.xMax - 26, card.y + (CARD_H / 2f) - 11, 22, 22),
                    new GUIContent("✕", "Remove this item")))
                {
                    if (EditorUtility.DisplayDialog("Remove Item",
                        $"Remove item at index {index}?", "Remove", "Cancel"))
                    {
                        _listItemsProp.DeleteArrayElementAtIndex(index);
                        serializedObject.ApplyModifiedProperties();
                    }
                }
                GUI.backgroundColor = Color.white;
            };

            // Add button
            _itemList.onAddCallback = list =>
            {
                int ni = _listItemsProp.arraySize;
                _listItemsProp.arraySize++;
                var np = _listItemsProp.GetArrayElementAtIndex(ni);
                np.FindPropertyRelative("itemName").stringValue = $"Item {ni + 1}";
                np.FindPropertyRelative("itemCost").intValue = 0;
                np.FindPropertyRelative("isDefault").boolValue = false;
                np.FindPropertyRelative("itemIcon").objectReferenceValue = null;
                serializedObject.ApplyModifiedProperties();
            };
        }

        // ── Item Grid (visual overview) ────────────────────────────────────
        private void DrawItemGrid()
        {
            var manager = (CF_ShopManager)target;
            var items = manager.GetAllItems();
            if (items == null || items.Count == 0)
            {
                EditorGUILayout.HelpBox("No items configured yet.", MessageType.Info);
                return;
            }

            var saveData = LoadSaveData();
            EditorGUILayout.LabelField("Quick Preview", MutedLabel());
            GUILayout.Space(2);

            const int cols = 5;
            const float cellH = 64f;
            float totalW = EditorGUIUtility.currentViewWidth - 32f;
            float cellW = totalW / cols;

            for (int row = 0; row < Mathf.CeilToInt(items.Count / (float)cols); row++)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    for (int col = 0; col < cols; col++)
                    {
                        int i = row * cols + col;
                        if (i >= items.Count) { GUILayout.FlexibleSpace(); continue; }

                        var cfg = items[i];
                        bool isDefault = cfg.isDefault;
                        bool unlocked = saveData != null && saveData.unlockedItems != null
                                         && saveData.unlockedItems.Contains(cfg.id);
                        bool equipped = saveData != null && saveData.equippedItem.Equals(cfg.id);

                        Color bg = equipped ? ColEquipped
                                 : unlocked ? ColUnlocked
                                 : isDefault ? ColDefault
                                 : ColLocked;

                        var cellRect = GUILayoutUtility.GetRect(cellW, cellH, GUILayout.Width(cellW));
                        EditorGUI.DrawRect(cellRect, bg);

                        // top accent
                        EditorGUI.DrawRect(new Rect(cellRect.x, cellRect.y, cellRect.width, 3),
                            equipped ? ColGold : isDefault ? new Color(0.65f, 0.45f, 0.85f) : ColAccent);

                        // icon
                        if (cfg.itemIcon != null)
                        {
                            var tex = AssetPreview.GetAssetPreview(cfg.itemIcon);
                            if (tex != null)
                            {
                                float s = Mathf.Min(cellRect.width - 6f, 28f);
                                float ix = cellRect.x + (cellRect.width - s) / 2f;
                                EditorGUI.DrawPreviewTexture(new Rect(ix, cellRect.y + 4, s, s), tex, null, ScaleMode.ScaleToFit);
                            }
                        }
                        else
                        {
                            string emoji = equipped ? "✅" : isDefault ? "🎁" : unlocked ? "🔓" : "🔒";
                            GUI.Label(new Rect(cellRect.x, cellRect.y + 2, cellRect.width, 26), emoji,
                                new GUIStyle(GUI.skin.label) { fontSize = 18, alignment = TextAnchor.MiddleCenter });
                        }

                        // name
                        GUI.Label(new Rect(cellRect.x, cellRect.y + 36, cellRect.width, 14),
                            string.IsNullOrEmpty(cfg.itemName) ? $"#{i}" : cfg.itemName,
                            new GUIStyle(EditorStyles.centeredGreyMiniLabel)
                            {
                                normal = { textColor = Color.white },
                                fontStyle = equipped ? FontStyle.Bold : FontStyle.Normal
                            });

                        // cost / status
                        string costLabel = isDefault ? "Free" : equipped ? "Equipped" : unlocked ? "Owned" : $"🪙{cfg.itemCost}";
                        GUI.Label(new Rect(cellRect.x, cellRect.y + 50, cellRect.width, 12), costLabel,
                            new GUIStyle(EditorStyles.centeredGreyMiniLabel)
                            {
                                normal = { textColor = equipped ? ColGold : isDefault ? new Color(0.8f, 0.7f, 1f) : ColMuted },
                                fontStyle = FontStyle.Bold
                            });

                        if (col < cols - 1) GUILayout.Space(2);
                    }
                }
                GUILayout.Space(2);
            }
        }

        // ══════════════════════════════════════════════════════════════════
        //  SETUP & DEBUG
        // ══════════════════════════════════════════════════════════════════

        private void DrawDebug()
        {
            var manager = (CF_ShopManager)target;
            var items = manager.GetAllItems();
            var saveData = LoadSaveData();

            // ── Status banner ─────────────────────────────────────────────
            var bannerRect = GUILayoutUtility.GetRect(0, 50, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(bannerRect, new Color(0.16f, 0.20f, 0.30f));

            int unlockedCount = saveData?.unlockedItems?.Count ?? 0;
            string equippedName = "—";
            if (saveData != null && items != null)
            {
                foreach (var it in items)
                    if (it.id.Equals(saveData.equippedItem)) { equippedName = it.itemName; break; }
            }

            GUI.Label(new Rect(bannerRect.x + 8, bannerRect.y + 5, bannerRect.width - 12, 18),
                $"🛒  Unlocked: {unlockedCount} / {items?.Count ?? 0}   •   Equipped: {equippedName}",
                new GUIStyle(EditorStyles.boldLabel) { normal = { textColor = Color.white } });
            GUI.Label(new Rect(bannerRect.x + 8, bannerRect.y + 24, bannerRect.width - 12, 14),
                Application.isPlaying ? "▶  Play Mode — live data" : "✏️  Edit Mode — writes directly to PlayerPrefs",
                new GUIStyle(EditorStyles.miniLabel)
                { normal = { textColor = Application.isPlaying ? new Color(0.5f, 1f, 0.5f) : new Color(0.9f, 0.8f, 0.3f) } });
            GUI.Label(new Rect(bannerRect.x + 8, bannerRect.y + 38, bannerRect.width - 12, 14),
                saveData == null ? "⚪  No save data found" : $"💾  Save key: {CF_SafetyKey.Data.SHOP_SAVE_KEY}",
                new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = ColMuted } });

            GUILayout.Space(10);

            if (items == null || items.Count == 0)
            {
                EditorGUILayout.HelpBox("Add items in the Shop Items list first.", MessageType.Warning);
                return;
            }

            // ── Unlock / Lock individual items ────────────────────────────
            EditorGUILayout.LabelField("Unlock / Lock Items", _sectionLabelStyle);
            GUILayout.Space(3);

            int cols = 3;
            for (int row = 0; row < Mathf.CeilToInt(items.Count / (float)cols); row++)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    for (int col = 0; col < cols; col++)
                    {
                        int i = row * cols + col;
                        if (i >= items.Count) { GUILayout.FlexibleSpace(); continue; }

                        var cfg = items[i];
                        bool unlocked = saveData?.unlockedItems?.Contains(cfg.id) ?? false;
                        bool equipped = saveData != null && saveData.equippedItem.Equals(cfg.id);

                        string label = unlocked ? $"🔓 {cfg.itemName}" : $"🔒 {cfg.itemName}";
                        GUI.backgroundColor = equipped ? ColEquipped : unlocked ? ColUnlocked : new Color(0.3f, 0.3f, 0.38f);

                        if (GUILayout.Button(new GUIContent(label, unlocked ? "Click to lock" : "Click to unlock"),
                            GUILayout.Height(26)))
                        {
                            ToggleUnlock(cfg.id, !unlocked);
                            if (Application.isPlaying) manager.Load();
                            Repaint();
                        }
                    }
                }
                GUILayout.Space(2);
            }
            GUI.backgroundColor = Color.white;

            GUILayout.Space(8);

            // ── Set Equipped ──────────────────────────────────────────────
            EditorGUILayout.LabelField("Set Equipped Item", _sectionLabelStyle);
            GUILayout.Space(3);

            for (int row = 0; row < Mathf.CeilToInt(items.Count / (float)cols); row++)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    for (int col = 0; col < cols; col++)
                    {
                        int i = row * cols + col;
                        if (i >= items.Count) { GUILayout.FlexibleSpace(); continue; }

                        var cfg = items[i];
                        bool isEquip = saveData != null && saveData.equippedItem.Equals(cfg.id);
                        bool unlocked = saveData?.unlockedItems?.Contains(cfg.id) ?? cfg.isDefault;

                        GUI.enabled = unlocked;
                        GUI.backgroundColor = isEquip ? ColEquipped : new Color(0.28f, 0.28f, 0.36f);

                        if (GUILayout.Button(new GUIContent(
                            isEquip ? $"✅ {cfg.itemName}" : cfg.itemName,
                            unlocked ? "Set as equipped" : "Unlock first"),
                            GUILayout.Height(26)))
                        {
                            SetEquipped(cfg.id);
                            if (Application.isPlaying) manager.Load();
                            Repaint();
                        }
                    }
                }
                GUILayout.Space(2);
            }
            GUI.enabled = true;
            GUI.backgroundColor = Color.white;

            GUILayout.Space(8);

            // ── Bulk Actions ──────────────────────────────────────────────
            EditorGUILayout.LabelField("Bulk Actions", _sectionLabelStyle);
            GUILayout.Space(3);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUI.backgroundColor = ColUnlocked;
                if (GUILayout.Button("🔓  Unlock All", GUILayout.Height(28)))
                {
                    var sd = LoadSaveData() ?? new ShopSaveData();
                    foreach (var it in items)
                        if (!sd.unlockedItems.Contains(it.id))
                            sd.unlockedItems.Add(it.id);
                    WriteSaveData(sd);
                    if (Application.isPlaying) manager.Load();
                    Repaint();
                }

                GUI.backgroundColor = ColLocked;
                if (GUILayout.Button("🔒  Lock All (keep default)", GUILayout.Height(28)))
                {
                    var sd = LoadSaveData() ?? new ShopSaveData();
                    sd.unlockedItems.Clear();
                    foreach (var it in items)
                        if (it.isDefault) sd.unlockedItems.Add(it.id);
                    WriteSaveData(sd);
                    if (Application.isPlaying) manager.Load();
                    Repaint();
                }

                GUI.backgroundColor = ColDanger;
                if (GUILayout.Button("🗑  Reset Save", GUILayout.Height(28)))
                {
                    if (EditorUtility.DisplayDialog("Reset Shop Save",
                        "Delete all shop save data? (unlocks, equipped state)",
                        "Yes, Reset", "Cancel"))
                    {
                        PlayerPrefs.DeleteKey(CF_SafetyKey.Data.SHOP_SAVE_KEY);
                        PlayerPrefs.Save();
                        if (Application.isPlaying) manager.Load();
                        Debug.Log("[Shop] Save data reset.");
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
            EditorGUILayout.TextField("equippedItem", data.equippedItem.ToString());
            EditorGUILayout.LabelField("unlockedItems");
            EditorGUI.indentLevel++;
            if (data.unlockedItems == null || data.unlockedItems.Count == 0)
                EditorGUILayout.LabelField("(none)", MutedLabel());
            else
                foreach (var id in data.unlockedItems)
                    EditorGUILayout.LabelField($"• {id}");
            EditorGUI.indentLevel--;
            GUI.enabled = true;
        }

        // ══════════════════════════════════════════════════════════════════
        //  PLAYERPREFS HELPERS
        // ══════════════════════════════════════════════════════════════════

        private ShopSaveData LoadSaveData()
        {
            if (Application.isPlaying)
            {
                // Rebuild from runtime state if in play mode
                var mgr = (CF_ShopManager)target;
                var items = mgr.GetAllItems();
                if (items != null)
                {
                    var sd = new ShopSaveData();
                    foreach (var it in items)
                    {
                        if (mgr.IsUnlocked(it.id)) sd.unlockedItems.Add(it.id);
                        if (mgr.IsEquipped(it.id)) sd.equippedItem = it.id;
                    }
                    return sd;
                }
            }

            string json = PlayerPrefs.GetString(CF_SafetyKey.Data.SHOP_SAVE_KEY, "");
            return string.IsNullOrEmpty(json) ? null : JsonUtility.FromJson<ShopSaveData>(json);
        }

        private void WriteSaveData(ShopSaveData data)
        {
            PlayerPrefs.SetString(CF_SafetyKey.Data.SHOP_SAVE_KEY, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }

        private void ToggleUnlock(ShopItemType id, bool unlock)
        {
            var data = LoadSaveData() ?? new ShopSaveData();
            if (unlock)
            {
                if (!data.unlockedItems.Contains(id))
                    data.unlockedItems.Add(id);
            }
            else
            {
                data.unlockedItems.Remove(id);
                if (data.equippedItem.Equals(id))
                    data.equippedItem = default;
            }
            WriteSaveData(data);
        }

        private void SetEquipped(ShopItemType id)
        {
            var data = LoadSaveData() ?? new ShopSaveData();
            if (!data.unlockedItems.Contains(id))
                data.unlockedItems.Add(id);
            data.equippedItem = id;
            WriteSaveData(data);
        }

        // ══════════════════════════════════════════════════════════════════
        //  HEADER
        // ══════════════════════════════════════════════════════════════════

        private new void DrawHeader()
        {
            var rect = GUILayoutUtility.GetRect(0, 44, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, ColBg);
            GUI.Label(new Rect(rect.x + 8, rect.y + 6, 30, 30), "🛒",
                new GUIStyle(GUI.skin.label) { fontSize = 22 });
            GUI.Label(new Rect(rect.x + 44, rect.y + 5, rect.width - 50, 20),
                "Shop Manager", _headerStyle);
            GUI.Label(new Rect(rect.x + 44, rect.y + 26, rect.width - 50, 14),
                "CataFury  •  Item Shop System",
                new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = ColMuted } });
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