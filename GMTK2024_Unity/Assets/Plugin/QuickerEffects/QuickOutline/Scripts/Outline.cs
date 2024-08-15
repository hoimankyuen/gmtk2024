//
// Improved outline from the outline by Chris Nolet.
//

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QuickerEffects
{
	[DisallowMultipleComponent]
	public class Outline : MonoBehaviour
	{
		public enum Mode
		{
			OutlineAll,
			OutlineVisible,
			OutlineHidden,
			OutlineAndSilhouette,
			SilhouetteOnly
		}

		[Serializable]
		private class ListVector3
		{
			public List<Vector3> data;
		}

		// ========================================================= Parameters =========================================================

		[SerializeField]
		private Mode outlineMode = Mode.OutlineAll;
		public Mode OutlineMode
		{
			get { return outlineMode; }
			set
			{
				if (outlineMode != value)
				{
					outlineMode = value;
					needsUpdateMaterial = true;
				}
			}
		}

		[SerializeField]
		private Color color = Color.white;
		public Color Color
		{
			get { return color; }
			set
			{
				if (color != value)
				{
					color = value;
					needsUpdateMaterial = true;
				}
			}
		}

		[SerializeField, Range(0f, 10f)]
		private float width = 2f;
		public float Width
		{
			get { return width; }
			set
			{
				if (width != value)
				{
					width = value;
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

		[SerializeField]
		private bool smoothNormals = true;
		public bool SmoothNormal
		{
			get
			{
				return smoothNormals;
			}
			set
			{
				if (smoothNormals != value)
				{
					smoothNormals = value;
					needsReloadNormals = true;
					needsUpdateMaterial = true;
				}
			}
		}

		[Header("Optional")]
		[SerializeField, Tooltip("Precompute enabled: Per-vertex calculations are performed in the editor and serialized with the object. "
			+ "Precompute disabled: Per-vertex calculations are performed at runtime in Awake(). This may cause a pause for large meshes.")]
		private bool precomputeOutline = false;

		// ========================================================= States =========================================================

		private List<Renderer> registeredRenderers = new List<Renderer>();
		private List<Renderer> appliedRenderers = new List<Renderer>();
		private Material outlineMaskMaterial;
		private Material outlineFillMaterial;

		private static HashSet<Mesh> registeredMeshes = new HashSet<Mesh>();

		[SerializeField, HideInInspector]
		private List<Mesh> bakeKeys = new List<Mesh>();
		[SerializeField, HideInInspector]
		private List<ListVector3> bakeValues = new List<ListVector3>();

		private bool needsUpdateRenderers;
		private bool needsReloadNormals;
		private bool needsUpdateMaterial;

		// ========================================================= Monobehaviour Methods =========================================================

		void OnValidate()
		{
			// Update material properties
			needsReloadNormals = true;
			needsUpdateMaterial = true;

			// Clear cache when baking is disabled or corrupted
			if (!precomputeOutline && bakeKeys.Count != 0 || bakeKeys.Count != bakeValues.Count)
			{
				bakeKeys.Clear();
				bakeValues.Clear();
			}

			// Generate smooth normals when baking is enabled
			if (precomputeOutline && bakeKeys.Count == 0)
			{
				Bake();
			}
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
				CheckForMissingRenderers();
				AppendRemoveMaterials(enabled);
				needsUpdateRenderers = false;
			}

			if (needsReloadNormals)
			{
				LoadNormals();
				needsReloadNormals = false;
			}

			if (needsUpdateMaterial)
			{
				UpdateMaterialProperties();
				needsUpdateMaterial = false;
			}
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
			outlineMaskMaterial = Instantiate(Resources.Load<Material>(@"Materials/OutlineMask"));
			outlineFillMaterial = Instantiate(Resources.Load<Material>(@"Materials/OutlineFill"));
			outlineMaskMaterial.name = "OutlineMask (Instance)";
			outlineFillMaterial.name = "OutlineFill (Instance)";
		}

		private void RemoveMaterials()
		{
			Destroy(outlineMaskMaterial);
			Destroy(outlineFillMaterial);
		}

		public void Refresh()
		{
			needsUpdateRenderers = true;
			needsReloadNormals = true;
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
				if (multiTextureRenderer.transform.Find(multiTextureRenderer.gameObject.name + " (Overlay)"))
				{
					continue;
				}

				// create game object
				GameObject displayObject = new GameObject(multiTextureRenderer.gameObject.name + " (Outline)");
				displayObject.transform.parent = multiTextureRenderer.transform;
				displayObject.transform.localPosition = Vector3.zero;
				displayObject.transform.localRotation = Quaternion.identity;
				displayObject.transform.localScale = Vector3.one;
				//displayObject.hideFlags = HideFlags.HideInHierarchy;

				if (multiTextureRenderer.GetType() == typeof(MeshRenderer))
				{
					// set all triangles to be in the same submesh
					Mesh mesh = new Mesh();
					mesh.name = "Outline Mesh";
					MeshFilter originalFilter = multiTextureRenderer.GetComponent<MeshFilter>();
					mesh.SetVertices(originalFilter.sharedMesh.vertices.ToList());
					mesh.SetTriangles(originalFilter.sharedMesh.triangles, 0);
					mesh.SetNormals(originalFilter.sharedMesh.normals.ToList());
					mesh.SetTangents(originalFilter.sharedMesh.tangents.ToList());

					// apply combined mesh to display object
					MeshFilter meshFilter = displayObject.AddComponent<MeshFilter>();
					meshFilter.sharedMesh = mesh;
					MeshRenderer meshRenderer = displayObject.AddComponent<MeshRenderer>();
					meshRenderer.material = Resources.Load<Material>("Materials/OutlineEmpty");

					// treat the newly created mesh as one of the normal mesh
					singleTexturedRenderers.Add(meshRenderer);
				}
				else if (multiTextureRenderer.GetType() == typeof(SkinnedMeshRenderer))
				{
					SkinnedMeshRenderer originalSmr = (SkinnedMeshRenderer)multiTextureRenderer;

					// set all triangles to be in the same submesh
					Mesh mesh = new Mesh();
					mesh.name = "Outline Mesh";
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
					smr.material = Resources.Load<Material>("Materials/OutlineEmpty");

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
			if (effectStopper == null || !effectStopper.stopOutlineEffect)
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

		// ========================================================= Normal Manipulations =========================================================

		private void Bake()
		{
			// Generate smooth normals for each mesh
			HashSet<Mesh> bakedMeshes = new HashSet<Mesh>();

			foreach (MeshFilter meshFilter in GetComponentsInChildren<MeshFilter>())
			{

				// Skip duplicates
				if (!bakedMeshes.Add(meshFilter.sharedMesh))
				{
					continue;
				}

				// Serialize smooth normals
				List<Vector3> smoothNormals = GetSmoothedNormals(meshFilter.sharedMesh);

				bakeKeys.Add(meshFilter.sharedMesh);
				bakeValues.Add(new ListVector3() { data = smoothNormals });
			}
		}

		private void LoadNormals()
		{
			// generate smooth normals if needed
			if (smoothNormals)
			{
				foreach (MeshFilter meshFilter in GetComponentsInChildren<MeshFilter>())
				{
					if (registeredMeshes.Add(meshFilter.sharedMesh))
					{
						// generate smooth normals
						int index = bakeKeys.IndexOf(meshFilter.sharedMesh);
						List<Vector3> normals = (index >= 0) ? bakeValues[index].data : GetSmoothedNormals(meshFilter.sharedMesh);

						// Store smooth normals in UV3
						meshFilter.sharedMesh.SetUVs(3, normals);
					}
				}
			}

			// Clear UV4 on skinned mesh renderers
			foreach (SkinnedMeshRenderer skinnedMeshRenderer in GetComponentsInChildren<SkinnedMeshRenderer>())
			{
				if (registeredMeshes.Add(skinnedMeshRenderer.sharedMesh))
				{
					skinnedMeshRenderer.sharedMesh.uv4 = new Vector2[skinnedMeshRenderer.sharedMesh.vertexCount];
				}
			}
		}

		private List<Vector3> GetSmoothedNormals(Mesh mesh)
		{

			// Group vertices by location
			var groups = mesh.vertices.Select((vertex, index) => new KeyValuePair<Vector3, int>(vertex, index)).GroupBy(pair => pair.Key);

			// Copy normals to a new list
			var normals = new List<Vector3>(mesh.normals);

			// Average normals for grouped vertices
			foreach (var group in groups)
			{

				// Skip single vertices
				if (group.Count() == 1)
				{
					continue;
				}

				// Calculate the average normal
				var normal = Vector3.zero;

				foreach (var pair in group)
				{
					normal += mesh.normals[pair.Value];
				}

				normal.Normalize();

				// Assign smooth normal to each vertex
				foreach (var pair in group)
				{
					normals[pair.Value] = normal;
				}
			}

			return normals;
		}

		// ========================================================= Effect Manipulations =========================================================

		private void UpdateMaterialProperties()
		{
			// Apply properties according to mode
			outlineMaskMaterial.SetFloat("_StencilPass", show ? (float)UnityEngine.Rendering.StencilOp.Replace : (float)UnityEngine.Rendering.StencilOp.Keep);
			outlineFillMaterial.SetColor("_OutlineColor", color);
			outlineFillMaterial.SetFloat("_OutlineShow", show ? 1 : 0);
			outlineFillMaterial.SetFloat("_OutlineUseSmoothedNormal", smoothNormals ? 1 : 0);

			switch (outlineMode)
			{
				case Mode.OutlineAll:
					outlineMaskMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
					outlineFillMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
					outlineFillMaterial.SetFloat("_OutlineWidth", width);
					break;

				case Mode.OutlineVisible:
					outlineMaskMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
					outlineFillMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.LessEqual);
					outlineFillMaterial.SetFloat("_OutlineWidth", width);
					break;

				case Mode.OutlineHidden:
					outlineMaskMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
					outlineFillMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Greater);
					outlineFillMaterial.SetFloat("_OutlineWidth", width);
					break;

				case Mode.OutlineAndSilhouette:
					outlineMaskMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.LessEqual);
					outlineFillMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
					outlineFillMaterial.SetFloat("_OutlineWidth", width);
					break;

				case Mode.SilhouetteOnly:
					outlineMaskMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.LessEqual);
					outlineFillMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Greater);
					outlineFillMaterial.SetFloat("_OutlineWidth", 0);
					break;
			}
		}

		private void AppendRemoveMaterials(bool append)
		{
			if (append)
			{
				// Append outline shaders
				for (int i = 0; i < appliedRenderers.Count; i++)
				{
					List<Material> materials = appliedRenderers[i].sharedMaterials.ToList();
					if (!materials.Contains(outlineMaskMaterial))
					{
						materials.Add(outlineMaskMaterial);
					}
					if (!materials.Contains(outlineFillMaterial))
					{
						materials.Add(outlineFillMaterial);
					}
					appliedRenderers[i].materials = materials.ToArray();
				}
			}
			else
			{
				// Remove outline shaders
				for (int i = 0; i < appliedRenderers.Count; i++)
				{
					List<Material> materials = appliedRenderers[i].sharedMaterials.ToList();
					if (materials.Contains(outlineMaskMaterial))
					{
						materials.Remove(outlineMaskMaterial);
					}
					if (materials.Contains(outlineFillMaterial))
					{
						materials.Remove(outlineFillMaterial);
					}
					appliedRenderers[i].materials = materials.ToArray();
				}
			}
		}
	}
}