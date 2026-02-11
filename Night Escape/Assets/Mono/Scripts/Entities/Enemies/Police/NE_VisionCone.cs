using UnityEngine;

namespace NightEscape
{
    public class NE_VisionCone : MonoBehaviour
    {
        [Header("Visual Settings")]
        public Material VisionConeMaterial;
        public float VisionRange;
        public float VisionAngle;
        public LayerMask VisionObstructingLayer;
        public int VisionConeResolution = 120;

        [Header("Detection Settings")]
        public float DetectionRange = 10f;
        public float DetectionAngle = 60f;
        public int DetectionResolution = 60;
        public LayerMask DetectionObstructingLayer;

        [Header("Gizmos")]
        public bool ShowDetectionGizmos = true;
        public Color DetectionGizmosColor = Color.red;

        private Mesh VisionConeMesh;
        private MeshFilter MeshFilter_;
        private NE_Police _policeController;
        private GameObject _detectedPlayer;

        void Start()
        {
            var renderer = gameObject.AddComponent<MeshRenderer>();
            renderer.material = VisionConeMaterial;

            renderer.receiveShadows = false;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            MeshFilter_ = gameObject.AddComponent<MeshFilter>();
            VisionConeMesh = new Mesh();

            _policeController = GetComponentInParent<NE_Police>();
        }


        void Update()
        {
            DrawVisionCone();
            DetectPlayer();
        }

        void DrawVisionCone()
        {
            int[] triangles = new int[(VisionConeResolution - 1) * 3];
            Vector3[] Vertices = new Vector3[VisionConeResolution + 1];
            Vertices[0] = Vector3.zero;
            float Currentangle = -VisionAngle / 2;
            float angleIcrement = VisionAngle / (VisionConeResolution - 1);
            float Sine;
            float Cosine;

            for (int i = 0; i < VisionConeResolution; i++)
            {
                Sine = Mathf.Sin(Currentangle * Mathf.Deg2Rad);
                Cosine = Mathf.Cos(Currentangle * Mathf.Deg2Rad);
                Vector3 RaycastDirection = (transform.forward * Cosine) + (transform.right * Sine);
                Vector3 VertForward = (Vector3.forward * Cosine) + (Vector3.right * Sine);

                if (Physics.Raycast(transform.position, RaycastDirection, out RaycastHit hit, VisionRange, VisionObstructingLayer))
                {
                    Vertices[i + 1] = VertForward * hit.distance;
                }
                else
                {
                    Vertices[i + 1] = VertForward * VisionRange;
                }

                Currentangle += angleIcrement;
            }

            for (int i = 0, j = 0; i < triangles.Length; i += 3, j++)
            {
                triangles[i] = 0;
                triangles[i + 1] = j + 1;
                triangles[i + 2] = j + 2;
            }

            VisionConeMesh.Clear();
            VisionConeMesh.vertices = Vertices;
            VisionConeMesh.triangles = triangles;
            VisionConeMesh.RecalculateNormals();
            VisionConeMesh.RecalculateBounds();
            MeshFilter_.mesh = VisionConeMesh;
        }

        private void DetectPlayer()
        {
            if (_detectedPlayer != null)
            {
                return;
            }

            float currentAngle = -DetectionAngle / 2;
            float angleIncrement = DetectionAngle / (DetectionResolution - 1);

            for (int i = 0; i < DetectionResolution; i++)
            {
                float sine = Mathf.Sin(currentAngle * Mathf.Deg2Rad);
                float cosine = Mathf.Cos(currentAngle * Mathf.Deg2Rad);
                Vector3 raycastDirection = (transform.forward * cosine) + (transform.right * sine);

                if (Physics.Raycast(transform.position, raycastDirection, out RaycastHit hit, DetectionRange, DetectionObstructingLayer))
                {
                    if (hit.collider.CompareTag(NE_SafetyKey.KEY_TAG_PLAYER))
                    {
                        _detectedPlayer = hit.collider.gameObject;

                        NE_PlayerController player = _detectedPlayer.GetComponentInParent<NE_PlayerController>();

                        if (_policeController != null)
                        {
                            if (player != null)
                            {
                                _policeController.DetectedPlayer(player);
                            }
                        }

                        return;
                    }
                }

                currentAngle += angleIncrement;
            }
        }

        private void OnDrawGizmos()
        {
            if (!ShowDetectionGizmos) return;

            Gizmos.color = DetectionGizmosColor;

            float currentAngle = -DetectionAngle / 2;
            float angleIncrement = DetectionAngle / (DetectionResolution - 1);
            Vector3 previousPoint = transform.position;

            for (int i = 0; i < DetectionResolution; i++)
            {
                float sine = Mathf.Sin(currentAngle * Mathf.Deg2Rad);
                float cosine = Mathf.Cos(currentAngle * Mathf.Deg2Rad);
                Vector3 raycastDirection = (transform.forward * cosine) + (transform.right * sine);

                Vector3 endPoint;
                if (Physics.Raycast(transform.position, raycastDirection, out RaycastHit hit, DetectionRange, DetectionObstructingLayer))
                {
                    endPoint = hit.point;
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(transform.position, endPoint);
                    Gizmos.DrawWireSphere(endPoint, 0.1f);
                    Gizmos.color = DetectionGizmosColor;
                }
                else
                {
                    endPoint = transform.position + raycastDirection * DetectionRange;
                    Gizmos.DrawLine(transform.position, endPoint);
                }

                if (i > 0)
                {
                    Gizmos.DrawLine(previousPoint, endPoint);
                }

                previousPoint = endPoint;
                currentAngle += angleIncrement;
            }

            float leftAngle = -DetectionAngle / 2;
            float rightAngle = DetectionAngle / 2;

            Vector3 leftDirection = (transform.forward * Mathf.Cos(leftAngle * Mathf.Deg2Rad)) +
                                   (transform.right * Mathf.Sin(leftAngle * Mathf.Deg2Rad));
            Vector3 rightDirection = (transform.forward * Mathf.Cos(rightAngle * Mathf.Deg2Rad)) +
                                    (transform.right * Mathf.Sin(rightAngle * Mathf.Deg2Rad));

            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + leftDirection * DetectionRange);
            Gizmos.DrawLine(transform.position, transform.position + rightDirection * DetectionRange);

#if UNITY_EDITOR
            UnityEditor.Handles.Label(
                transform.position + transform.forward * DetectionRange * 0.5f + Vector3.up * 0.5f,
                $"Detection: {DetectionRange}m / {DetectionAngle}°"
            );
#endif
        }
    }
}