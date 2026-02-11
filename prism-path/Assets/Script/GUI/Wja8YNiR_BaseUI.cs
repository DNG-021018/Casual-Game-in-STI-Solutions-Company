using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UIPageId
{
    // ==================== MAIN MENU ====================
    MainMenu = 1,
    Settings = 2,
    Tutorial = 3,
    LevelSelect = 4,

    // ==================== GAMEPLAY ====================
    GamePlay = 20,
    Pause = 21,
    WinGame = 22,
    MirrorControlPanel = 23,
    LoseGame = 24,
}

public enum SlideDir
{
    Left,
    Right,
    Up,
    Down,
    None
}

namespace Wja8YNiR_PrismPath
{
    public abstract class Wja8YNiR_BaseUI : MonoBehaviour
    {
        [System.Serializable]
        struct Entry
        {
            public UIPageId id;
            public Wja8YNiR_UIPage page;
        }
        [SerializeField] List<Entry> pages = new();

        protected readonly Dictionary<UIPageId, Wja8YNiR_UIPage> _map = new();
        protected readonly Stack<UIPageId> _stack = new();

        protected virtual void Awake()
        {
            foreach (Entry e in pages)
            {
                if (e.page == null) continue;
                _map[e.id] = e.page;
                e.page.Init(this);
            }
        }

        protected virtual void OnEnable()
        {
            if (Wja8YNiR_GameManager.Instance) Wja8YNiR_GameManager.Instance.OnGameStateChanged += HandleGameState;
        }

        protected virtual void OnDisable()
        {
            if (Wja8YNiR_GameManager.Instance) Wja8YNiR_GameManager.Instance.OnGameStateChanged -= HandleGameState;
        }

        protected abstract void HandleGameState(GameState s);

        public void Open(UIPageId id, object ctx = null, bool clearStack = false)
        {
            if (!_map.TryGetValue(id, out Wja8YNiR_UIPage page)) return;
            StopAllCoroutines();
            StartCoroutine(CoOpen(id, page, ctx, clearStack));
        }

        IEnumerator CoOpen(UIPageId id, Wja8YNiR_UIPage page, object ctx, bool clearStack)
        {
            if (clearStack) yield return StartCoroutine(CloseAllRoutine());
            if (_stack.Count > 0) yield return StartCoroutine(_map[_stack.Peek()].Hide());
            _stack.Push(id);
            page.ApplyContext(ctx);
            yield return StartCoroutine(page.Show(ctx));
        }

        public void Back()
        {
            if (_stack.Count <= 1) return;
            StopAllCoroutines();
            StartCoroutine(CoBack());
        }

        IEnumerator CoBack()
        {
            Wja8YNiR_UIPage cur = _map[_stack.Pop()];
            yield return StartCoroutine(cur.Hide());

            Wja8YNiR_UIPage prev = _map[_stack.Peek()];
            yield return StartCoroutine(prev.Show());
        }

        public void CloseAll() => StartCoroutine(CloseAllRoutine());

        IEnumerator CloseAllRoutine()
        {
            while (_stack.Count > 0)
            {
                Wja8YNiR_UIPage p = _map[_stack.Pop()];
                yield return StartCoroutine(p.Hide());
            }
        }
    }
}
