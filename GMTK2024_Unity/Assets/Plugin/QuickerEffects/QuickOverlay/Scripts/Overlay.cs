using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QuickerEffects
{
	public class Overlay : MonoBehaviour
	{
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

		private List<Renderer> registeredRenderers = new List<Renderer>();
		private List<Renderer> appliedRenderers = new List<Renderer>();
		private Material overlayMaterial;

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
				CheckForMissingRenderers();
				AppendRemoveMaterials(enabled);
				needsUpdateRenderers = false;
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
			overlayMaterial = Instantiate(Resources.Load<Material>(@"Materials/ColorOverlay"));
			overlayMaterial.name = "ColorOverlay (Instance)";
		}

		private void RemoveMaterials()
		{
			Destroy(overlayMaterial);
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
				if (multiTextureRenderer.transform.Find(multiTextureRenderer.gameObject.name + " (Overlay)"))
				{
					continue;
				}

				// create game object
				GameObject displayObject = new GameObject(multiTextureRenderer.gameObject.name + " (Overlay)");
				displayObject.transform.parent = multiTextureRenderer.transform;
				displayObject.transform.localPosition = Vector3.zero;
				displayObject.transform.localRotation = Quaternion.identity;
				displayObject.transform.localScale = Vector3.one;
				//displayObject.hideFlags = HideFlags.HideInHierarchy;

				if (multiTextureRenderer.GetType() == typeof(MeshRenderer))
				{
					// set all triangles to be in the same submesh
					Mesh mesh = new Mesh();
					mesh.name = "Overlay Mesh";
					MeshFilter originalFilter = multiTextureRenderer.GetComponent<MeshFilter>();
					mesh.SetVertices(originalFilter.sharedMesh.vertices.ToList());
					mesh.SetTriangles(originalFilter.sharedMesh.triangles, 0);
					mesh.SetNormals(originalFilter.sharedMesh.normals.ToList());
					mesh.SetTangents(originalFilter.sharedMesh.tangents.ToList());

					// apply combined mesh to display object
					MeshFilter meshFilter = displayObject.AddComponent<MeshFilter>();
					meshFilter.sharedMesh = mesh;
					MeshRenderer meshRenderer = displayObject.AddComponent<MeshRenderer>();
					meshRenderer.material = Resources.Load<Material>("Materials/OverlayEmpty");

					// treat the newly created mesh as one of the normal mesh
					singleTexturedRenderers.Add(meshRenderer);
				}
				else if (multiTextureRenderer.GetType() == typeof(SkinnedMeshRenderer))
				{
					SkinnedMeshRenderer originalSmr = (SkinnedMeshRenderer)multiTextureRenderer;

					// set all triangles to be in the same submesh
					Mesh mesh = new Mesh();
					mesh.name = "Overlay Mesh";
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
					smr.material = Resources.Load<Material>("Materials/OverlayEmpty");

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
			if (effectStopper == null || !effectStopper.stopOverlayEffect)
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

		private void UpdateMaterialProperties()
		{
			overlayMaterial.SetColor("_Color", color);
			overlayMaterial.SetFloat("_Show", show ? 1 : 0);
		}

		private void AppendRemoveMaterials(bool append)
		{
			if (append)
			{
				// Append overlay shaders
				for (int i = 0; i < appliedRenderers.Count; i++)
				{
					List<Material> materials = appliedRenderers[i].sharedMaterials.ToList();
					if (!materials.Contains(overlayMaterial))
					{
						materials.Add(overlayMaterial);
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
					if (materials.Contains(overlayMaterial))
					{
						materials.Remove(overlayMaterial);
					}
					appliedRenderers[i].materials = materials.ToArray();
				}
			}
		}
	}
}