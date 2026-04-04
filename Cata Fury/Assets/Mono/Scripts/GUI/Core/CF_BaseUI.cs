using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CataFury
{
    public abstract class CF_BaseUI : MonoBehaviour
    {
        [System.Serializable]
        struct Entry
        {
            public UIPageId id;
            public CF_UIPage page;
        }
        [SerializeField] List<Entry> pages = new();

        protected readonly Dictionary<UIPageId, CF_UIPage> _map = new();
        protected readonly Stack<UIPageId> _stack = new();
        protected CF_UIManager uiManager;

        protected virtual void Awake()
        {
            uiManager = ServiceLocator.Get<CF_UIManager>();

            foreach (Entry e in pages)
            {
                if (e.page == null) continue;
                _map[e.id] = e.page;
                e.page.Init(this);
            }
        }

        protected virtual void OnEnable()
        {
            if (uiManager) uiManager.OnGameStateChanged += HandleGameState;
        }

        protected virtual void OnDisable()
        {
            if (uiManager) uiManager.OnGameStateChanged -= HandleGameState;
        }

        protected abstract void HandleGameState(GameState s);

        public void Open(UIPageId id, object ctx = null, bool clearStack = false)
        {
            if (!_map.TryGetValue(id, out CF_UIPage page)) return;
            StopAllCoroutines();
            StartCoroutine(CoOpen(id, page, ctx, clearStack));
        }

        IEnumerator CoOpen(UIPageId id, CF_UIPage page, object ctx, bool clearStack)
        {
            if (clearStack) yield return StartCoroutine(CloseAllRoutine());
            if (_stack.Count > 0) yield return StartCoroutine(_map[_stack.Peek()].Hide());
            _stack.Push(id);
            page.ApplyContext(ctx);
            yield return StartCoroutine(page.Show());
        }

        public void Back()
        {
            if (_stack.Count <= 1) return;
            StopAllCoroutines();
            StartCoroutine(CoBack());
        }

        IEnumerator CoBack()
        {
            CF_UIPage cur = _map[_stack.Pop()];
            yield return StartCoroutine(cur.Hide());

            CF_UIPage prev = _map[_stack.Peek()];
            yield return StartCoroutine(prev.Show());
        }

        public void CloseAll() => StartCoroutine(CloseAllRoutine());

        IEnumerator CloseAllRoutine()
        {
            while (_stack.Count > 0)
            {
                CF_UIPage p = _map[_stack.Pop()];
                yield return StartCoroutine(p.Hide());
            }
        }
    }
}
