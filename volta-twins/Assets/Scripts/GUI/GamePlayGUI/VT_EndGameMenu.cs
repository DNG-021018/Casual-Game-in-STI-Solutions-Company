using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace VoltaTwins
{
    public class VT_EndGameMenu : VT_UIPage
    {
        [Header("Panel")]
        [SerializeField] Panels Panel;

        [Header("Button")]
        [SerializeField] VT_UIButton nextLevelBtn;
        [SerializeField] VT_UIButton replayBtn;
        [SerializeField] VT_UIButton exitBtn;

        [Header("Clip")]
        [SerializeField] AudioClip AwakeClip;

        [Header("Level Image Display")]
        [SerializeField] Image levelImage;

        [Header("Level Sprites")]
        [SerializeField] List<Sprite> levelSprites;

        VT_LevelManager levelManager;

        public override void Init(VT_BaseUI parent)
        {
            base.Init(parent);
        }

        void Start()
        {
            levelManager = VT_LevelManager.Instance;

            if (nextLevelBtn != null) nextLevelBtn.Bind(() =>
            {
                VT_GameManager.Instance.currentLevel++;
                VT_LoadingScreenRoot.Instance.LoadScene("GamePlay");
            });

            if (replayBtn != null) replayBtn.Bind(() =>
            {
                VT_LoadingScreenRoot.Instance.LoadScene("GamePlay");
            });

            if (exitBtn != null) exitBtn.Bind(() =>
            {
                VT_LoadingScreenRoot.Instance.LoadScene("StartGame");
            });

            if (levelImage != null)
            {
                if (VT_GameManager.Instance != null) levelImage.sprite = levelSprites[!levelManager.checkNextLevelInvalid() ? VT_GameManager.Instance.currentLevel : VT_GameManager.Instance.currentLevel - 1];
            }

            if (nextLevelBtn != null) nextLevelBtn.gameObject.SetActive(!levelManager.checkNextLevelInvalid());
        }

        void OnDestroy()
        {
            if (nextLevelBtn != null) nextLevelBtn.UnBind();
            if (replayBtn != null) replayBtn.UnBind();
            if (exitBtn != null) exitBtn.UnBind();
        }

        protected override void CacheStartPositions()
        {
            if (Panel.panel) Panel.panel.anchoredPosition = new Vector2(0, -offscreenPadding);
        }

        public override IEnumerator Show(object ctx = null)
        {
            base.Show(ctx);
            CacheStartPositions();

            if (VT_AudioManager.Instance && AwakeClip)
            {
                VT_AudioManager.Instance.PlaySfx(AwakeClip);
            }

            yield return ShowScalePanels(
                duration, showEase, 0f, 1f,
                (Panel.panel, Vector3.zero, Panel.panel.localScale)
            );
        }

        public override IEnumerator Hide()
        {
            base.Hide();

            CacheStartPositions();

            yield return HideScalePanels(
                duration, hideEase, 1f, 0f,
                (Panel.panel, Panel.panel.localScale, Vector3.zero)
            );
        }
    }
}
