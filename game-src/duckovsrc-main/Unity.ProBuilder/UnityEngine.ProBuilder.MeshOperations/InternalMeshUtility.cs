using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.ProBuilder.MeshOperations;

internal static class InternalMeshUtility
{
	internal static Vector3 AverageNormalWithIndexes(SharedVertex shared, int[] all, IList<Vector3> norm)
	{
		Vector3 zero = Vector3.zero;
		int num = 0;
		for (int i = 0; i < all.Length; i++)
		{
			if (shared.Contains(all[i]))
			{
				zero += norm[all[i]];
				num++;
			}
		}
		return zero / num;
	}

	public static ProBuilderMesh CreateMeshWithTransform(Transform t, bool preserveFaces)
	{
		Mesh sharedMesh = t.GetComponent<MeshFilter>().sharedMesh;
		Vector3[] meshChannel = MeshUtility.GetMeshChannel(t.gameObject, (Mesh x) => x.vertices);
		Color[] meshChannel2 = MeshUtility.GetMeshChannel(t.gameObject, (Mesh x) => x.colors);
		Vector2[] meshChannel3 = MeshUtility.GetMeshChannel(t.gameObject, (Mesh x) => x.uv);
		List<Vector3> list = (preserveFaces ? new List<Vector3>(sharedMesh.vertices) : new List<Vector3>());
		List<Color> list2 = (preserveFaces ? new List<Color>(sharedMesh.colors) : new List<Color>());
		List<Vector2> list3 = (preserveFaces ? new List<Vector2>(sharedMesh.uv) : new List<Vector2>());
		List<Face> list4 = new List<Face>();
		for (int num = 0; num < sharedMesh.subMeshCount; num++)
		{
			int[] triangles = sharedMesh.GetTriangles(num);
			for (int num2 = 0; num2 < triangles.Length; num2 += 3)
			{
				int num3 = -1;
				if (preserveFaces)
				{
					for (int num4 = 0; num4 < list4.Count; num4++)
					{
						if (list4[num4].distinctIndexesInternal.Contains(triangles[num2]) || list4[num4].distinctIndexesInternal.Contains(triangles[num2 + 1]) || list4[num4].distinctIndexesInternal.Contains(triangles[num2 + 2]))
						{
							num3 = num4;
							break;
						}
					}
				}
				if (num3 > -1 && preserveFaces)
				{
					int num5 = list4[num3].indexesInternal.Length;
					int[] array = new int[num5 + 3];
					Array.Copy(list4[num3].indexesInternal, 0, array, 0, num5);
					array[num5] = triangles[num2];
					array[num5 + 1] = triangles[num2 + 1];
					array[num5 + 2] = triangles[num2 + 2];
					list4[num3].indexesInternal = array;
					continue;
				}
				int[] triangles2;
				if (preserveFaces)
				{
					triangles2 = new int[3]
					{
						triangles[num2],
						triangles[num2 + 1],
						triangles[num2 + 2]
					};
				}
				else
				{
					list.Add(meshChannel[triangles[num2]]);
					list.Add(meshChannel[triangles[num2 + 1]]);
					list.Add(meshChannel[triangles[num2 + 2]]);
					list2.Add((meshChannel2 != null) ? meshChannel2[triangles[num2]] : Color.white);
					list2.Add((meshChannel2 != null) ? meshChannel2[triangles[num2 + 1]] : Color.white);
					list2.Add((meshChannel2 != null) ? meshChannel2[triangles[num2 + 2]] : Color.white);
					list3.Add(meshChannel3[triangles[num2]]);
					list3.Add(meshChannel3[triangles[num2 + 1]]);
					list3.Add(meshChannel3[triangles[num2 + 2]]);
					triangles2 = new int[3]
					{
						num2,
						num2 + 1,
						num2 + 2
					};
				}
				list4.Add(new Face(triangles2, num, AutoUnwrapSettings.tile, 0, -1, -1, manualUVs: true));
			}
		}
		GameObject gameObject = Object.Instantiate(t.gameObject);
		gameObject.GetComponent<MeshFilter>().sharedMesh = null;
		ProBuilderMesh proBuilderMesh = gameObject.AddComponent<ProBuilderMesh>();
		proBuilderMesh.RebuildWithPositionsAndFaces(list.ToArray(), list4.ToArray());
		proBuilderMesh.colorsInternal = list2.ToArray();
		proBuilderMesh.textures = list3;
		proBuilderMesh.gameObject.name = t.name;
		gameObject.transform.position = t.position;
		gameObject.transform.localRotation = t.localRotation;
		gameObject.transform.localScale = t.localScale;
		proBuilderMesh.CenterPivot(null);
		return proBuilderMesh;
	}

	public static bool ResetPbObjectWithMeshFilter(ProBuilderMesh pb, bool preserveFaces)
	{
		MeshFilter component = pb.gameObject.GetComponent<MeshFilter>();
		if (component == null || component.sharedMesh == null)
		{
			Log.Error(pb.name + " does not have a mesh or Mesh Filter component.");
			return false;
		}
		Mesh sharedMesh = component.sharedMesh;
		int vertexCount = sharedMesh.vertexCount;
		Vector3[] meshChannel = MeshUtility.GetMeshChannel(pb.gameObject, (Mesh x) => x.vertices);
		Color[] meshChannel2 = MeshUtility.GetMeshChannel(pb.gameObject, (Mesh x) => x.colors);
		Vector2[] meshChannel3 = MeshUtility.GetMeshChannel(pb.gameObject, (Mesh x) => x.uv);
		List<Vector3> list = (preserveFaces ? new List<Vector3>(sharedMesh.vertices) : new List<Vector3>());
		List<Color> list2 = (preserveFaces ? new List<Color>(sharedMesh.colors) : new List<Color>());
		List<Vector2> list3 = (preserveFaces ? new List<Vector2>(sharedMesh.uv) : new List<Vector2>());
		List<Face> list4 = new List<Face>();
		MeshRenderer meshRenderer = pb.gameObject.GetComponent<MeshRenderer>();
		if (meshRenderer == null)
		{
			meshRenderer = pb.gameObject.AddComponent<MeshRenderer>();
		}
		int num = meshRenderer.sharedMaterials.Length;
		for (int num2 = 0; num2 < sharedMesh.subMeshCount; num2++)
		{
			int[] triangles = sharedMesh.GetTriangles(num2);
			for (int num3 = 0; num3 < triangles.Length; num3 += 3)
			{
				int num4 = -1;
				if (preserveFaces)
				{
					for (int num5 = 0; num5 < list4.Count; num5++)
					{
						if (list4[num5].distinctIndexesInternal.Contains(triangles[num3]) || list4[num5].distinctIndexesInternal.Contains(triangles[num3 + 1]) || list4[num5].distinctIndexesInternal.Contains(triangles[num3 + 2]))
						{
							num4 = num5;
							break;
						}
					}
				}
				if (num4 > -1 && preserveFaces)
				{
					int num6 = list4[num4].indexesInternal.Length;
					int[] array = new int[num6 + 3];
					Array.Copy(list4[num4].indexesInternal, 0, array, 0, num6);
					array[num6] = triangles[num3];
					array[num6 + 1] = triangles[num3 + 1];
					array[num6 + 2] = triangles[num3 + 2];
					list4[num4].indexesInternal = array;
					continue;
				}
				int[] triangles2;
				if (preserveFaces)
				{
					triangles2 = new int[3]
					{
						triangles[num3],
						triangles[num3 + 1],
						triangles[num3 + 2]
					};
				}
				else
				{
					list.Add(meshChannel[triangles[num3]]);
					list.Add(meshChannel[triangles[num3 + 1]]);
					list.Add(meshChannel[triangles[num3 + 2]]);
					list2.Add((meshChannel2 != null && meshChannel2.Length == vertexCount) ? meshChannel2[triangles[num3]] : Color.white);
					list2.Add((meshChannel2 != null && meshChannel2.Length == vertexCount) ? meshChannel2[triangles[num3 + 1]] : Color.white);
					list2.Add((meshChannel2 != null && meshChannel2.Length == vertexCount) ? meshChannel2[triangles[num3 + 2]] : Color.white);
					list3.Add(meshChannel3[triangles[num3]]);
					list3.Add(meshChannel3[triangles[num3 + 1]]);
					list3.Add(meshChannel3[triangles[num3 + 2]]);
					triangles2 = new int[3]
					{
						num3,
						num3 + 1,
						num3 + 2
					};
				}
				list4.Add(new Face(triangles2, Math.Clamp(num2, 0, num - 1), AutoUnwrapSettings.tile, 0, -1, -1, manualUVs: true));
			}
		}
		pb.positionsInternal = list.ToArray();
		pb.texturesInternal = list3.ToArray();
		pb.facesInternal = list4.ToArray();
		pb.sharedVerticesInternal = SharedVertex.GetSharedVerticesWithPositions(list.ToArray());
		pb.colorsInternal = list2.ToArray();
		return true;
	}

	internal static void FilterUnusedSubmeshIndexes(ProBuilderMesh mesh)
	{
		Material[] sharedMaterials = mesh.renderer.sharedMaterials;
		int num = sharedMaterials.Length;
		bool[] array = new bool[num];
		Face[] facesInternal = mesh.facesInternal;
		foreach (Face face in facesInternal)
		{
			array[Math.Clamp(face.submeshIndex, 0, num - 1)] = true;
		}
		IEnumerable<int> enumerable = array.AllIndexesOf((bool x) => !x);
		if (!enumerable.Any())
		{
			return;
		}
		facesInternal = mesh.facesInternal;
		foreach (Face face2 in facesInternal)
		{
			int submeshIndex = face2.submeshIndex;
			foreach (int item in enumerable)
			{
				if (submeshIndex > item)
				{
					face2.submeshIndex--;
				}
			}
		}
		mesh.renderer.sharedMaterials = sharedMaterials.RemoveAt(enumerable);
	}
}
