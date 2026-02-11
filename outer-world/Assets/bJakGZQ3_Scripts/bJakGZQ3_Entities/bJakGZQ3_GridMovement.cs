using UnityEngine;
using DG.Tweening;
using System;
using System.Collections.Generic;

namespace bJakGZQ3_Outer_World
{
    public class bJakGZQ3_GridMovement : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private bJakGZQ3_GridSystem grid;

        [Header("Move Settings")]
        [SerializeField] private float moveDuration = 0.12f;
        [SerializeField] private float moveCooldown = 0.05f;

        [Header("Rotate Settings")]
        [SerializeField] private float rotateDuration = 0.10f;
        [SerializeField] private Ease rotateEase = Ease.OutSine;
        [SerializeField] private bool rotateWithMove = true;
        [SerializeField] private bool rotateEvenIfBlocked = true;
        private Transform rotateTransform;

        bool _isMoving;
        public bool IsMoving => _isMoving;
        float _lastMoveEndTime;
        bool canMove;
        Sequence _moveSeq;

        void Awake()
        {
            if (!grid) grid = FindFirstObjectByType<bJakGZQ3_GridSystem>();
            rotateTransform = transform;
        }

        void Start()
        {
            EnableMovement();
        }

        public bool TryGetDirection(
            Vector2 deltaXZ,
            float minSwipeDistanceWorld,
            float axisBias,
            out CellDirection dir
        )
        {
            dir = default;

            if (!canMove || grid == null) return false;

            Transform gt = grid.transform;
            Vector2 right2D = new Vector2(gt.right.x, gt.right.z).normalized;
            Vector2 forward2D = new Vector2(gt.forward.x, gt.forward.z).normalized;

            float alongRight = Vector2.Dot(deltaXZ, right2D);
            float alongForward = Vector2.Dot(deltaXZ, forward2D);

            float ax = Mathf.Abs(alongRight);
            float ay = Mathf.Abs(alongForward);
            float major = Mathf.Max(ax, ay);

            if (major < minSwipeDistanceWorld)
                return false;

            if (ax > ay + axisBias)
                dir = alongRight > 0 ? CellDirection.RIGHT : CellDirection.LEFT;
            else
                dir = alongForward > 0 ? CellDirection.UP : CellDirection.DOWN;

            return true;
        }

        public bool TryGetRandomDirection(out CellDirection dir)
        {
            dir = default;

            if (!canMove || grid == null) return false;

            List<CellDirection> dirs = new(4)
            {
                CellDirection.UP,
                CellDirection.DOWN,
                CellDirection.LEFT,
                CellDirection.RIGHT
            };

            for (int i = 0; i < dirs.Count; i++)
            {
                int r = UnityEngine.Random.Range(i, dirs.Count);
                CellDirection tmp = dirs[i];
                dirs[i] = dirs[r];
                dirs[r] = tmp;
            }

            for (int i = 0; i < dirs.Count; i++)
            {
                CellDirection tryDir = dirs[i];
                if (grid.TryGetNextCellCenter(transform.position, tryDir, out Vector3 _))
                {
                    dir = tryDir;
                    return true;
                }
            }

            return false;
        }

        public void Move(CellDirection dir, Action OnMove = null, Action OnMoveSucess = null, Action OnMoveNotSucess = null)
        {
            if (!(bJakGZQ3_GameManager.Instance.GetState() == GameState.Play)) return;
            if (!canMove) return;
            if (grid == null) return;
            if (_isMoving) return;
            if (Time.time < _lastMoveEndTime + moveCooldown) return;
            _lastMoveEndTime = 0;
            Vector3 faceDir = DirToWorld(dir);
            if (faceDir.sqrMagnitude < 1e-6f) faceDir = transform.forward;
            Quaternion targetRot = Quaternion.LookRotation(faceDir, Vector3.up);

            bool shouldMove = grid.TryGetNextCellCenter(transform.position, dir, out Vector3 center);

            if (shouldMove)
            {
                Vector3 target = new Vector3(center.x, transform.position.y, center.z);
                _isMoving = true;

                Sequence seq = DOTween.Sequence();
                _moveSeq = seq;

                OnMove?.Invoke();

                if (rotateWithMove)
                {
                    seq.Append(rotateTransform.DORotateQuaternion(targetRot, rotateDuration).SetEase(rotateEase));
                    seq.Join(transform.DOMove(target, moveDuration).SetEase(Ease.OutSine));
                }
                else
                {
                    seq.Append(rotateTransform.DORotateQuaternion(targetRot, rotateDuration).SetEase(rotateEase));
                    seq.Append(transform.DOMove(target, moveDuration).SetEase(Ease.OutSine));
                }

                seq.OnComplete(() =>
                {
                    _lastMoveEndTime = Time.time;
                    _isMoving = false;
                    OnMoveSucess?.Invoke();
                });
            }
            else
            {
                if (rotateEvenIfBlocked)
                {
                    rotateTransform.DORotateQuaternion(targetRot, rotateDuration).SetEase(rotateEase);
                }
                OnMoveNotSucess?.Invoke();
            }
        }

        Vector3 DirToWorld(CellDirection dir)
        {
            Transform gt = grid.transform;
            Vector3 right = new Vector3(gt.right.x, 0f, gt.right.z).normalized;
            Vector3 forward = new Vector3(gt.forward.x, 0f, gt.forward.z).normalized;

            switch (dir)
            {
                case CellDirection.LEFT: return -right;
                case CellDirection.RIGHT: return right;
                case CellDirection.UP: return forward;
                case CellDirection.DOWN: return -forward;
                default: return forward;
            }
        }

        public void EnableMovement() => canMove = true;
        public void DisableMovement() => canMove = false;
        public void PauseMove() => _moveSeq?.Pause();
        public void ResumeMove() => _moveSeq?.Play();

        public bool TryPeekNextCellCenter(CellDirection dir, out Vector3 center)
        {
            center = default;
            if (grid == null) return false;
            return grid.TryGetNextCellCenter(transform.position, dir, out center);
        }
    }
}
