using System.Collections.Generic;
using UnityEngine;

namespace Wja8YNiR_PrismPath
{
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))]
	[ExecuteInEditMode]
	public class VolumetricLineBehavior : MonoBehaviour
	{
		static readonly Vector3 Average = new Vector3(1f / 3f, 1f / 3f, 1f / 3f);

		#region private variables
		[SerializeField] private LayerMask _plantMask = ~0;

		[SerializeField]
		public Material m_templateMaterial;

		[SerializeField]
		private bool m_doNotOverwriteTemplateMaterialProperties;

		[SerializeField]
		private Vector3 m_startPos;

		[SerializeField]
		private Vector3 m_endPos = new Vector3(0f, 0f, 100f);

		[SerializeField]
		private Color m_lineColor;

		[SerializeField]
		private float m_lineWidth;

		[SerializeField]
		[Range(0.0f, 1.0f)]
		private float m_lightSaberFactor;

		private Material m_material;

		private MeshFilter m_meshFilter;

		private float _currentProgress = 0f;
		private readonly HashSet<int> _hitPlants = new();
		#endregion

		#region properties
		public Material TemplateMaterial
		{
			get { return m_templateMaterial; }
			set { m_templateMaterial = value; }
		}

		public bool DoNotOverwriteTemplateMaterialProperties
		{
			get { return m_doNotOverwriteTemplateMaterialProperties; }
			set { m_doNotOverwriteTemplateMaterialProperties = value; }
		}

		public Color LineColor
		{
			get { return m_lineColor; }
			set
			{
				CreateMaterial();
				if (null != m_material)
				{
					m_lineColor = value;
					m_material.color = m_lineColor;
				}
			}
		}

		public float LineWidth
		{
			get { return m_lineWidth; }
			set
			{
				CreateMaterial();
				if (null != m_material)
				{
					m_lineWidth = value;
					m_material.SetFloat("_LineWidth", m_lineWidth);
				}
				UpdateBounds();
			}
		}

		public float LightSaberFactor
		{
			get { return m_lightSaberFactor; }
			set
			{
				CreateMaterial();
				if (null != m_material)
				{
					m_lightSaberFactor = value;
					m_material.SetFloat("_LightSaberFactor", m_lightSaberFactor);
				}
			}
		}

		public Vector3 StartPos
		{
			get { return m_startPos; }
			set
			{
				m_startPos = value;
				SetStartAndEndPoints(m_startPos, m_endPos);
			}
		}

		public Vector3 EndPos
		{
			get { return m_endPos; }
			set
			{
				m_endPos = value;
				SetStartAndEndPoints(m_startPos, m_endPos);
			}
		}

		#endregion

		#region methods
		private void CreateMaterial()
		{
			if (null == m_material || null == GetComponent<MeshRenderer>().sharedMaterial)
			{
				if (null != m_templateMaterial)
				{
					m_material = Material.Instantiate(m_templateMaterial);
					GetComponent<MeshRenderer>().sharedMaterial = m_material;
					SetAllMaterialProperties();
				}
				else
				{
					m_material = GetComponent<MeshRenderer>().sharedMaterial;
				}
			}
		}

		private void DestroyMaterial()
		{
			if (null != m_material)
			{
				DestroyImmediate(m_material);
				m_material = null;
			}
		}

		private float CalculateLineScale()
		{
			return Vector3.Dot(transform.lossyScale, Average);
		}

		public void UpdateLineScale()
		{
			if (null != m_material)
			{
				m_material.SetFloat("_LineScale", CalculateLineScale());
			}
		}

		private void SetAllMaterialProperties()
		{
			SetStartAndEndPoints(m_startPos, m_endPos);

			if (null != m_material)
			{
				if (!m_doNotOverwriteTemplateMaterialProperties)
				{
					m_material.color = m_lineColor;
					m_material.SetFloat("_LineWidth", m_lineWidth);
					m_material.SetFloat("_LightSaberFactor", m_lightSaberFactor);
				}
				UpdateLineScale();
			}
		}

		private Bounds CalculateBounds()
		{
			var maxWidth = Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);
			var scaledLineWidth = maxWidth * LineWidth * 0.5f;

			var min = new Vector3(
				Mathf.Min(m_startPos.x, m_endPos.x) - scaledLineWidth,
				Mathf.Min(m_startPos.y, m_endPos.y) - scaledLineWidth,
				Mathf.Min(m_startPos.z, m_endPos.z) - scaledLineWidth
			);
			var max = new Vector3(
				Mathf.Max(m_startPos.x, m_endPos.x) + scaledLineWidth,
				Mathf.Max(m_startPos.y, m_endPos.y) + scaledLineWidth,
				Mathf.Max(m_startPos.z, m_endPos.z) + scaledLineWidth
			);

			return new Bounds
			{
				min = min,
				max = max
			};
		}

		public void UpdateBounds()
		{
			if (null != m_meshFilter)
			{
				var mesh = m_meshFilter.sharedMesh;
				Debug.Assert(null != mesh);
				if (null != mesh)
				{
					mesh.bounds = CalculateBounds();
				}
			}
		}

		public void SetStartAndEndPoints(Vector3 startPoint, Vector3 endPoint)
		{
			m_startPos = startPoint;
			m_endPos = endPoint;

			Vector3[] vertexPositions = {
				m_startPos,
				m_startPos,
				m_startPos,
				m_startPos,
				m_endPos,
				m_endPos,
				m_endPos,
				m_endPos,
			};

			Vector3[] other = {
				m_endPos,
				m_endPos,
				m_endPos,
				m_endPos,
				m_startPos,
				m_startPos,
				m_startPos,
				m_startPos,
			};

			if (null != m_meshFilter)
			{
				var mesh = m_meshFilter.sharedMesh;
				Debug.Assert(null != mesh);
				if (null != mesh)
				{
					mesh.vertices = vertexPositions;
					mesh.normals = other;
					UpdateBounds();
				}
			}
		}

		public void SetProgress(float progress)
		{
			_currentProgress = progress;
			CheckPlantsWithProgress(progress);
		}

		public void ResetPlantCheck()
		{
			_hitPlants.Clear();
			_currentProgress = 0f;
		}

		void CheckPlantsWithProgress(float progress)
		{
			if (progress <= 0f) return;

			Vector3 worldStart = transform.TransformPoint(m_startPos);
			Vector3 worldEnd = transform.TransformPoint(m_endPos);

			Vector3 currentEnd = Vector3.Lerp(worldStart, worldEnd, progress);

			Vector3 dir = currentEnd - worldStart;
			float distance = dir.magnitude;
			if (distance <= 1e-5f) return;
			dir /= distance;

			RaycastHit[] hits = Physics.RaycastAll(worldStart, dir, distance, _plantMask, QueryTriggerInteraction.Collide);

			foreach (var hit in hits)
			{
				if (hit.collider.TryGetComponent(out Wja8YNiR_Plants plant))
				{
					int id = hit.collider.gameObject.GetInstanceID();
					if (_hitPlants.Add(id))
					{
						Debug.DrawLine(worldStart, hit.point, Color.red, 0.5f);
						plant.TriggerPlant?.Invoke();
					}
				}
			}
		}
		#endregion

		#region event functions
		void Start()
		{
			Mesh mesh = new Mesh();
			m_meshFilter = GetComponent<MeshFilter>();
			m_meshFilter.mesh = mesh;
			SetStartAndEndPoints(m_startPos, m_endPos);
			mesh.uv = VolumetricLineVertexData.TexCoords;
			mesh.uv2 = VolumetricLineVertexData.VertexOffsets;
			mesh.SetIndices(VolumetricLineVertexData.Indices, MeshTopology.Triangles, 0);
			CreateMaterial();
		}

		void OnDestroy()
		{
			if (null != m_meshFilter)
			{
				if (Application.isPlaying)
				{
					Mesh.Destroy(m_meshFilter.sharedMesh);
				}
				else
				{
					Mesh.DestroyImmediate(m_meshFilter.sharedMesh);
				}
				m_meshFilter.sharedMesh = null;
			}
			DestroyMaterial();
		}

		void Update()
		{
			if (transform.hasChanged)
			{
				UpdateLineScale();
				UpdateBounds();
			}
		}

		void OnDrawGizmos()
		{
			Gizmos.color = Color.green;
			Gizmos.DrawLine(gameObject.transform.TransformPoint(m_startPos), gameObject.transform.TransformPoint(m_endPos));
		}
		#endregion
	}
}