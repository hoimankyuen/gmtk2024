using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QuickerEffects
{
	public class Blinker : MonoBehaviour
	{
		public enum Direction
		{
			XPositive,
			XNegative,
			YPositive,
			YNegative,
			ZPositive,
			ZNegative,
		}

		// ========================================================= Parameters =========================================================

		[SerializeField]
		private Color color = Color.white;
		public Color Color
		{
			get
			{
				return color;
			}
			set
			{
				if (color != value)
				{
					color = value;
					needsUpdateMaterial = true;
				}
			}
		}

		public Direction direction = Direction.YPositive;

		[SerializeField]
		private float speed = 1.5f;
		public float Speed
		{
			get
			{
				return speed;
			}
			set
			{
				if (speed != value)
				{
					speed = value;
					needsUpdateMaterial = true;
				}
			}
		}

		[SerializeField]
		private float cyclePeriod = 2f;
		public float CyclePeriod
		{
			get
			{
				return cyclePeriod;
			}
			set
			{
				if (cyclePeriod != value)
				{
					cyclePeriod = value;
					needsUpdateMaterial = true;
				}
			}
		}

		[SerializeField]
		private bool show = true;
		public bool Show
		{
			get
			{
				return show;
			}
			set
			{
				if (show != value)
				{
					show = value;
					needsUpdateMaterial = true;
				}
			}
		}

		// ========================================================= States =========================================================

		private Vector3 yStart = new Vector3(0, -2, 0);
		private Vector3 yEnd = new Vector3(0, 2, 0);
		private float bandHeight;
		private float bandFalloff;

		private List<Renderer> registeredRenderers = new List<Renderer>();
		private List<Renderer> appliedRenderers = new List<Renderer>();
		private Material blinkerMaterial;

		private bool needsUpdateRenderers;
		private bool needsUpdateMaterial;

		// ========================================================= Monobehaviour Methods =========================================================

		void OnValidate()
		{
			needsUpdateMaterial = true;
		}

		void Awake()
		{
			InstantiateMaterials();
			Refresh();
		}

		void Update()
		{
			if (needsUpdateRenderers)
			{
				RetrieveRenderers();
				AppendRemoveMaterials(enabled);
				needsUpdateRenderers = false;
			}

			if (needsUpdateMaterial)
			{
				UpdateMaterialPropertiesDemand();
				needsUpdateMaterial = false;
			}

			CheckForMissingRenderers();
			RecalculateEffectRange();
			UpdateMaterialPropertiesAlways();
		}

		void OnDestroy()
		{
			RemoveMaterials();
		}

		void OnEnable()
		{
			CheckForMissingRenderers();
			AppendRemoveMaterials(true);
		}

		void OnDisable()
		{
			CheckForMissingRenderers();
			AppendRemoveMaterials(false);
		}

		// ========================================================= Processing =========================================================

		private void InstantiateMaterials()
		{
			blinkerMaterial = Instantiate(Resources.Load<Material>(@"Materials/ColorBlinker"));
			blinkerMaterial.name = "ColorBlinker (Instance)";
		}

		private void RemoveMaterials()
		{
			Destroy(blinkerMaterial);
		}

		public void Refresh()
		{
			needsUpdateRenderers = true;
			needsUpdateMaterial = true;
		}

		// ========================================================= Renderers Manipulations =========================================================

		private void RetrieveRenderers()
		{
			// recursive search for all available renderers, skips all those after the next effect component
			Transform current = transform;
			List<Renderer> originalRenderers = RetrieveRenderers(transform);

			// sorting and only consider previously untreated renderers renderers
			List<Renderer> singleTexturedRenderers = new List<Renderer>();
			List<Renderer> multiTexturedRenderers = new List<Renderer>();
			foreach (Renderer originalRenderer in originalRenderers)
			{
				if (originalRenderer is ParticleSystemRenderer)
				{
					continue;
				}

				if (!registeredRenderers.Contains(originalRenderer))
				{
					if (originalRenderer.sharedMaterials.Length == 1 ||
					originalRenderer.sharedMaterials[1].name.Equals("OutlineMask (Instance)") ||
					originalRenderer.sharedMaterials[1].name.Equals("OutlineFill (Instance)") ||
					originalRenderer.sharedMaterials[1].name.Equals("ColorBlinker (Instance)") ||
					originalRenderer.sharedMaterials[1].name.Equals("ColorOverlay (Instance)"))
					{
						singleTexturedRenderers.Add(originalRenderer);
					}
					else
					{
						multiTexturedRenderers.Add(originalRenderer);
					}
					registeredRenderers.Add(originalRenderer);
				}
			}

			// generate display objects for renderer with multiple textures
			foreach (Renderer multiTextureRenderer in multiTexturedRenderers)
			{
				// skip generation if there already exist a display object
				if (multiTextureRenderer.transform.Find(multiTextureRenderer.gameObject.name + " (Blinker)"))
				{
					continue;
				}

				// create game object
				GameObject displayObject = new GameObject(multiTextureRenderer.gameObject.name + " (Blinker)");
				displayObject.transform.parent = multiTextureRenderer.transform;
				displayObject.transform.localPosition = Vector3.zero;
				displayObject.transform.localRotation = Quaternion.identity;
				displayObject.transform.localScale = Vector3.one;
				//displayObject.hideFlags = HideFlags.HideInHierarchy;

				if (multiTextureRenderer.GetType() == typeof(MeshRenderer))
				{
					// set all triangles to be in the same submesh
					Mesh mesh = new Mesh();
					mesh.name = "Blinker Mesh";
					MeshFilter originalFilter = multiTextureRenderer.GetComponent<MeshFilter>();
					mesh.SetVertices(originalFilter.sharedMesh.vertices.ToList());
					mesh.SetTriangles(originalFilter.sharedMesh.triangles, 0);
					mesh.SetNormals(originalFilter.sharedMesh.normals.ToList());
					mesh.SetTangents(originalFilter.sharedMesh.tangents.ToList());

					// apply combined mesh to display object
					MeshFilter meshFilter = displayObject.AddComponent<MeshFilter>();
					meshFilter.sharedMesh = mesh;
					MeshRenderer meshRenderer = displayObject.AddComponent<MeshRenderer>();
					meshRenderer.material = Resources.Load<Material>("Materials/BlinkerEmpty");

					// treat the newly created mesh as one of the normal mesh
					singleTexturedRenderers.Add(meshRenderer);
				}
				else if (multiTextureRenderer.GetType() == typeof(SkinnedMeshRenderer))
				{
					SkinnedMeshRenderer originalSmr = (SkinnedMeshRenderer)multiTextureRenderer;

					// set all triangles to be in the same submesh
					Mesh mesh = new Mesh();
					mesh.name = "Blinker Mesh";
					mesh.SetVertices(originalSmr.sharedMesh.vertices.ToList());
					mesh.SetTriangles(originalSmr.sharedMesh.triangles, 0);
					mesh.SetNormals(originalSmr.sharedMesh.normals.ToList());
					mesh.SetTangents(originalSmr.sharedMesh.tangents.ToList());
					mesh.boneWeights = originalSmr.sharedMesh.boneWeights;
					mesh.bindposes = originalSmr.sharedMesh.bindposes;

					// apply combined mesh to display object
					SkinnedMeshRenderer smr = displayObject.AddComponent<SkinnedMeshRenderer>();
					smr.bones = originalSmr.bones;
					smr.renderingLayerMask = originalSmr.renderingLayerMask;
					smr.quality = originalSmr.quality;
					smr.updateWhenOffscreen = originalSmr.updateWhenOffscreen;
					smr.skinnedMotionVectors = originalSmr.skinnedMotionVectors;
					smr.rootBone = originalSmr.rootBone;
					smr.sharedMesh = mesh;
					smr.material = Resources.Load<Material>("Materials/BlinkerEmpty");

					// treat the newly created mesh as one of the normal mesh
					singleTexturedRenderers.Add(smr);
				}
			}

			// finalize renderer list
			appliedRenderers.AddRange(singleTexturedRenderers);
		}

		private List<Renderer> RetrieveRenderers(Transform parent)
		{
			List<Renderer> renderers = new List<Renderer>();

			EffectStopper effectStopper = parent.GetComponent<EffectStopper>();
			if (effectStopper == null || !effectStopper.stopBlinkerEffect)
			{
				Renderer r = parent.GetComponent<Renderer>();
				if (r != null)
					renderers.Add(r);
			}
			else if (effectStopper.applyTarget == EffectStopper.ApplyTarget.SelfAndChildren)
			{
				return renderers;
			}

			for (int i = 0; i < parent.childCount; i++)
			{
				Transform child = parent.GetChild(i);
				if (child.GetComponent<Overlay>() != null)
					continue;
				renderers.AddRange(RetrieveRenderers(child));
			}
			return renderers;
		}

		private void CheckForMissingRenderers()
		{
			for (int i = 0; i < appliedRenderers.Count; i++)
			{
				if (appliedRenderers[i] == null)
				{
					appliedRenderers.RemoveAt(i);
					i--;
				}
			}

			for (int i = 0; i < registeredRenderers.Count; i++)
			{
				if (registeredRenderers[i] == null)
				{
					registeredRenderers.RemoveAt(i);
					i--;
				}
			}
		}

		// ========================================================= Effect Manipulations =========================================================

		private void UpdateMaterialPropertiesDemand()
		{
			blinkerMaterial.SetColor("_Color", color);
			blinkerMaterial.SetFloat("_Show", show ? 1 : 0);
			blinkerMaterial.SetFloat("_BlinkBand", bandHeight);
			blinkerMaterial.SetFloat("_BlinkFalloff", bandFalloff);
			blinkerMaterial.SetFloat("_BlinkSpeed", speed);
			blinkerMaterial.SetFloat("_BlinkPeriod", cyclePeriod);
		}

		private void UpdateMaterialPropertiesAlways()
		{
			blinkerMaterial.SetVector("_BlinkStart", yStart);
			blinkerMaterial.SetVector("_BlinkEnd", yEnd);
		}

		public void RecalculateEffectRange()
		{
			float startVal = float.PositiveInfinity;
			float endVal = float.NegativeInfinity;
			Vector3 rendererMedian = Vector3.zero;
			foreach (Renderer renderer in appliedRenderers)
			{
				Bounds bounds = renderer.bounds;
				switch (direction)
				{
					case Direction.XPositive:
					case Direction.XNegative:
						if (bounds.max.x > endVal)
							endVal = bounds.max.x;
						if (bounds.min.x < startVal)
							startVal = bounds.min.x;
						break;
					case Direction.YPositive:
					case Direction.YNegative:
						if (bounds.max.y > endVal)
							endVal = bounds.max.y;
						if (bounds.min.y < startVal)
							startVal = bounds.min.y;
						break;
					case Direction.ZPositive:
					case Direction.ZNegative:
						if (bounds.max.z > endVal)
							endVal = bounds.max.z;
						if (bounds.min.z < startVal)
							startVal = bounds.min.z;
						break;
				}
				rendererMedian += renderer.transform.position;
			}
			rendererMedian = rendererMedian / appliedRenderers.Count;
			yStart = rendererMedian;
			yEnd = rendererMedian;
			switch (direction)
			{
				case Direction.XPositive:
					yStart.x = startVal;
					yEnd.x = endVal;
					break;
				case Direction.XNegative:
					yStart.x = endVal;
					yEnd.x = startVal;
					break;
				case Direction.YPositive:
					yStart.y = startVal;
					yEnd.y = endVal;
					break;
				case Direction.YNegative:
					yStart.y = endVal;
					yEnd.y = startVal;
					break;
				case Direction.ZPositive:
					yStart.z = startVal;
					yEnd.z = endVal;
					break;
				case Direction.ZNegative:
					yStart.z = endVal;
					yEnd.z = startVal;
					break;
			}

			float objectTotalHeight = endVal - startVal;
			float newBandHeight = objectTotalHeight / 40f;
			float newBandFalloff = objectTotalHeight / 4f * 3f;
			if (Mathf.Abs(bandHeight - newBandHeight) > 0.00001f)
			{
				bandHeight = newBandHeight;
				needsUpdateMaterial = true;
			}
			if (Mathf.Abs(bandFalloff - newBandFalloff) > 0.00001f)
			{
				bandFalloff = newBandFalloff;
				needsUpdateMaterial = true;
			}
		}

		private void AppendRemoveMaterials(bool append)
		{
			if (append)
			{
				// Append overlay shaders
				for (int i = 0; i < appliedRenderers.Count; i++)
				{
					List<Material> materials = appliedRenderers[i].sharedMaterials.ToList();
					if (!materials.Contains(blinkerMaterial))
					{
						materials.Add(blinkerMaterial);
					}
					appliedRenderers[i].materials = materials.ToArray();
				}
			}
			else
			{
				// Remove overlay shaders
				for (int i = 0; i < appliedRenderers.Count; i++)
				{
					List<Material> materials = appliedRenderers[i].sharedMaterials.ToList();
					if (!materials.Contains(blinkerMaterial))
					{
						materials.Remove(blinkerMaterial);
					}
					appliedRenderers[i].materials = materials.ToArray();
				}
			}
		}
	}
}