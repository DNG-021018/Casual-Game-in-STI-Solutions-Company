using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace bJakGZQ3_Outer_World
{
    public class bJakGZQ3_EndGameMenu : bJakGZQ3_UIPage
    {
        [Header("Panel")]
        [SerializeField] Panels Panel;

        [Header("Button")]
        [SerializeField] bJakGZQ3_UIButton replayBtn;
        [SerializeField] bJakGZQ3_UIButton exitBtn;

        [Header("Text")]
        [SerializeField] TextMeshProUGUI Time;
        [SerializeField] TextMeshProUGUI Round;
        [SerializeField] TextMeshProUGUI Steps;

        [Header("Audio")]
        [SerializeField] AudioClip AwakeClip;
        bJakGZQ3_LevelManager levelManager;

        [Header("Sunlight")]
        [SerializeField] RectTransform sunlight1;
        [SerializeField] RectTransform sunlight2;
        [SerializeField, Range(10f, 360f)] float rotationSpeed = 90f;

        public override void Init(bJakGZQ3_BaseUI parent)
        {
            base.Init(parent);
        }

        void Start()
        {
            levelManager = bJakGZQ3_LevelManager.Instance;

            if (replayBtn != null) replayBtn.Bind(() =>
            {
                bJakGZQ3_LoadingScreenRoot.Instance.LoadScene("GamePlay");
            });

            if (exitBtn != null) exitBtn.Bind(() =>
            {
                bJakGZQ3_LoadingScreenRoot.Instance.LoadScene("StartGame");
            });

            if (levelManager != null)
            {
                if (Round != null)
                {
                    Round.text = levelManager.GetFinalRound();
                }

                if (Time != null)
                {
                    Time.text = levelManager.GetFinalTime();
                }

                if (Steps != null)
                {
                    Steps.text = levelManager.GetFinalSteps();
                }
            }
        }

        void OnDestroy()
        {
            if (replayBtn != null) replayBtn.UnBind();
            if (exitBtn != null) exitBtn.UnBind();
        }

        void StartSunlightRotation(RectTransform sunlight)
        {
            if (sunlight == null) return;
            sunlight.DOKill();
            float duration = 360f / rotationSpeed;

            sunlight
                .DORotate(new Vector3(0f, 0f, 360f), duration, RotateMode.LocalAxisAdd)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Restart)
                .SetUpdate(true);
        }

        void StopSunlightRotation(RectTransform sunlight)
        {
            if (sunlight != null)
            {
                sunlight.DOKill();
            }
        }

        public override IEnumerator Show(object ctx = null)
        {
            yield return new WaitForSeconds(2f);
            base.Show(ctx);

            StartSunlightRotation(sunlight1);
            StartSunlightRotation(sunlight2);

            if (bJakGZQ3_AudioManager.Instance && AwakeClip)
            {
                bJakGZQ3_AudioManager.Instance.PlaySfx(AwakeClip);
            }

            yield return ShowScalePanels(
                duration, showEase, 0f, 1f,
                (Panel.panel, Vector3.zero, Panel.panel.localScale)
            );
        }

        public override IEnumerator Hide()
        {
            base.Hide();

            StopSunlightRotation(sunlight1);
            StopSunlightRotation(sunlight2);

            yield return HideScalePanels(
                duration, hideEase, 1f, 0f,
                (Panel.panel, Panel.panel.localScale, Vector3.zero)
            );
        }
    }
}
