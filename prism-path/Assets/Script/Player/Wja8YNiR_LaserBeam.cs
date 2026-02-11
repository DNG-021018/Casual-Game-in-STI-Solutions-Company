using System;
using System.Collections.Generic;
using UnityEngine;

namespace Wja8YNiR_PrismPath
{
    public class Wja8YNiR_LaserBeam : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private LayerMask _mirrorMask;
        [SerializeField] private LayerMask _blockMask;
        [SerializeField] private Transform _startPoint;

        [SerializeField] private float _defaultLength = 1000;
        public float DefaultLength => _defaultLength;

        [SerializeField] private int _numOfReflections = 20;
        public float NumOfReflections => _numOfReflections;

        [Header("Volumetric line")]
        [SerializeField] private GameObject volumetricPrefab;
        private readonly List<VolumetricLineBehavior> _VolumetricList = new();

        [SerializeField] private Transform _pointLight;
        private List<Transform> _pointLightList = new();

        [Header("Volumetric Settings")]
        [SerializeField] private float _volumetricSpeed = 2f;
        private float[] _volumetricProgress;

        private LineRenderer _lineRenderer;
        private RaycastHit hit;
        private Ray ray;
        private bool _isStart;

        private int _blockedSegmentIndex = -1;
        private bool _hasBlockedSegment = false;
        public event Action OnLaserBlocked;
        void OnEnable()
        {
            //Wja8YNiR_GamePlay.Shoot += StartShooting;
        }

        void OnDisable()
        {
            //Wja8YNiR_GamePlay.Shoot -= StartShooting;
        }

        void Start()
        {
            _lineRenderer = GetComponentInChildren<LineRenderer>();
            if (_startPoint == null)
            {
                _startPoint.position = Vector3.zero;
            }

            for (int i = 0; i < _numOfReflections; i++)
            {
                GameObject light = Instantiate(_pointLight.gameObject, _startPoint.transform);
                _pointLightList.Add(light.transform);
                light.SetActive(false);
            }

            for (int i = _VolumetricList.Count; i < _numOfReflections; i++)
            {
                GameObject go = Instantiate(volumetricPrefab, _startPoint);
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;
                VolumetricLineBehavior vb = go.GetComponent<VolumetricLineBehavior>();
                if (vb == null)
                {
                    Destroy(go);
                    break;
                }
                _VolumetricList.Add(vb);
                _VolumetricList[i].gameObject.SetActive(false);
            }

            _volumetricProgress = new float[Math.Max(1, _VolumetricList.Count)];
            for (int i = 0; i < _volumetricProgress.Length; i++) _volumetricProgress[i] = 0f;
        }

        void Update()
        {
            if (Wja8YNiR_LevelManager.Instance.isGameFinish) return;
            ReflectLaser();
            RenderVolumetric();

            if (_hasBlockedSegment && _blockedSegmentIndex >= 0 && _blockedSegmentIndex < _volumetricProgress.Length)
            {
                if (_volumetricProgress[_blockedSegmentIndex] >= 1f)
                {
                    ResetLazer();
                }
            }
        }

        void ReflectLaser()
        {
            if (!_isStart || _lineRenderer == null || volumetricPrefab == null)
            {
                for (int i = 0; i < _VolumetricList.Count; i++)
                {
                    if (_VolumetricList[i] != null) _VolumetricList[i].gameObject.SetActive(false);
                    if (i < _pointLightList.Count && _pointLightList[i] != null) _pointLightList[i].gameObject.SetActive(false);
                }
                return;
            }

            Vector3 dir = Quaternion.AngleAxis(-90f, Vector3.up) * transform.forward;
            ray = new Ray(_startPoint.position, dir);

            _lineRenderer.positionCount = 1;
            _lineRenderer.SetPosition(0, _startPoint.position);

            float remainLength = DefaultLength;

            int combinedMask = _mirrorMask.value | _blockMask.value;

            for (int i = 0; i < NumOfReflections; i++)
            {
                RaycastHit[] hits = Physics.RaycastAll(ray, remainLength, combinedMask, QueryTriggerInteraction.Collide);
                if (hits.Length > 0)
                {
                    Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
                    RaycastHit nearest = hits[0];
                    GameObject hitObj = nearest.collider.gameObject;

                    bool isMirror = IsInLayerMask(hitObj, _mirrorMask);
                    bool isBlock = IsInLayerMask(hitObj, _blockMask);

                    hit = nearest;

                    _lineRenderer.positionCount += 1;
                    _lineRenderer.SetPosition(_lineRenderer.positionCount - 1, hit.point);
                    RenderLighting(i);

                    if (isMirror)
                    {
                        remainLength -= hit.distance;
                        ray = new Ray(hit.point, Vector3.Reflect(ray.direction, hit.normal));
                        continue;
                    }

                    if (isBlock)
                    {
                        _blockedSegmentIndex = i;
                        _hasBlockedSegment = true;

                        for (int k = i + 1; k < _pointLightList.Count; k++)
                            _pointLightList[k].gameObject.SetActive(false);

                        break;
                    }
                }
                else
                {
                    _lineRenderer.positionCount += 1;
                    _lineRenderer.SetPosition(_lineRenderer.positionCount - 1, ray.origin + (ray.direction * remainLength));
                    RenderLighting(i);

                    for (int k = i; k < _pointLightList.Count; k++)
                        _pointLightList[k].gameObject.SetActive(false);
                    break;
                }
            }
        }

        void RenderLighting(int i)
        {
            if (i < 0 || i >= _pointLightList.Count) return;
            return;
        }

        void RenderVolumetric()
        {
            if (!_isStart || _lineRenderer == null || volumetricPrefab == null)
            {
                for (int i = 0; i < _VolumetricList.Count; i++)
                {
                    if (_VolumetricList[i] != null) _VolumetricList[i].gameObject.SetActive(false);
                    if (i < _pointLightList.Count && _pointLightList[i] != null) _pointLightList[i].gameObject.SetActive(false);
                }
                return;
            }

            int segmentCount = Mathf.Max(0, _lineRenderer.positionCount - 1);
            int useCount = Mathf.Min(segmentCount, _VolumetricList.Count);

            if (_volumetricProgress == null || _volumetricProgress.Length != _VolumetricList.Count)
            {
                _volumetricProgress = new float[_VolumetricList.Count];
            }

            for (int i = 0; i < useCount; i++)
            {
                Vector3 worldStart = _lineRenderer.GetPosition(i);
                Vector3 worldEnd = _lineRenderer.GetPosition(i + 1);

                VolumetricLineBehavior vb = _VolumetricList[i];
                if (vb == null) continue;

                bool canAdvance = (i == 0) || (_volumetricProgress[i - 1] >= 1f);

                if (canAdvance)
                {
                    _volumetricProgress[i] += Time.deltaTime * _volumetricSpeed;
                    _volumetricProgress[i] = Mathf.Clamp01(_volumetricProgress[i]);
                }

                bool segmentVisible = _volumetricProgress[i] > 0f;
                vb.gameObject.SetActive(segmentVisible);

                Vector3 localStart = vb.transform.InverseTransformPoint(worldStart);
                Vector3 localEnd = vb.transform.InverseTransformPoint(worldEnd);

                vb.StartPos = localStart;
                vb.EndPos = Vector3.Lerp(localStart, localEnd, _volumetricProgress[i]);

                vb.SetProgress(_volumetricProgress[i]);

                if (i < _pointLightList.Count && _pointLightList[i] != null)
                {
                    if (_volumetricProgress[i] >= 1f)
                    {
                        _pointLightList[i].gameObject.SetActive(true);
                        _pointLightList[i].position = worldEnd;
                    }
                    else
                    {
                        _pointLightList[i].gameObject.SetActive(false);
                    }
                }
            }

            for (int i = useCount; i < _VolumetricList.Count; i++)
            {
                VolumetricLineBehavior vb = _VolumetricList[i];
                if (vb != null)
                {
                    vb.gameObject.SetActive(false);
                    vb.ResetPlantCheck();
                }
                if (_volumetricProgress != null && i < _volumetricProgress.Length) _volumetricProgress[i] = 0f;
                if (i < _pointLightList.Count && _pointLightList[i] != null) _pointLightList[i].gameObject.SetActive(false);
            }
        }

        public void ResetLazer()
        {
            OnLaserBlocked?.Invoke();
            Wja8YNiR_GameManager.Instance.SetState(GameState.Playing);
            _isStart = false;
            _hasBlockedSegment = false;
            _blockedSegmentIndex = -1;

            if (_lineRenderer != null)
            {
                _lineRenderer.positionCount = 0;
            }

            for (int i = 0; i < _VolumetricList.Count; i++)
            {
                if (_VolumetricList[i] != null)
                {
                    _VolumetricList[i].gameObject.SetActive(false);
                    _VolumetricList[i].ResetPlantCheck();
                }
            }
            for (int i = 0; i < _pointLightList.Count; i++)
            {
                if (_pointLightList[i] != null) _pointLightList[i].gameObject.SetActive(false);
            }

            if (_volumetricProgress != null)
            {
                for (int i = 0; i < _volumetricProgress.Length; i++)
                    _volumetricProgress[i] = 0f;
            }
        }

        public void StartShooting()
        {
            _hasBlockedSegment = false;
            _blockedSegmentIndex = -1;

            if (_volumetricProgress != null)
            {
                for (int i = 0; i < _volumetricProgress.Length; i++)
                {
                    _volumetricProgress[i] = 0f;
                }
            }

            for (int i = 0; i < _VolumetricList.Count; i++)
            {
                if (_VolumetricList[i] != null)
                {
                    _VolumetricList[i].ResetPlantCheck();
                }
            }

            _isStart = true;
        }

        private bool IsInLayerMask(GameObject obj, LayerMask mask)
        {
            return (mask.value & (1 << obj.layer)) != 0;
        }
    }
}