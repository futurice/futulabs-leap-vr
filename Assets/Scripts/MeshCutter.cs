using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Futulabs
{
	public class MeshCut
	{

		public enum MeshCutPieces
		{
			LEFT_SIDE = 0,
			RIGHT_SIDE = 1
		}

		public class MeshCutSide
		{
			public List<Vector3>  vertices  = new List<Vector3>();
			public List<Vector3>  normals   = new List<Vector3>();
			public List<Vector2>  uvs       = new List<Vector2>();
			public List<int>      triangles = new List<int>();
			public List<List<int>> subIndices = new List<List<int>>();


			public void ClearAll()
			{
				vertices.Clear();
				normals.Clear();
				uvs.Clear();
				triangles.Clear();
				subIndices.Clear();
			}

			public void AddTriangle( int p1, int p2, int p3, int submesh)
			{
				// triangle index order goes 1,2,3,4....
				int base_index = vertices.Count;

				subIndices[submesh].Add(base_index);
				subIndices[submesh].Add(base_index+1);
				subIndices[submesh].Add(base_index+2);

				triangles.Add(base_index);
				triangles.Add(base_index+1);
				triangles.Add(base_index+2);

				vertices.Add(targetMesh.vertices[p1]);
				vertices.Add(targetMesh.vertices[p2]);
				vertices.Add(targetMesh.vertices[p3]);

				normals.Add(targetMesh.normals[p1]);
				normals.Add(targetMesh.normals[p2]);
				normals.Add(targetMesh.normals[p3]);

				uvs.Add(targetMesh.uv[p1]);
				uvs.Add(targetMesh.uv[p2]);
				uvs.Add(targetMesh.uv[p3]);

			}

			public void AddTriangle(Vector3[] points3, Vector3[] normals3, Vector2[] uvs3, Vector3 faceNormal, int submesh)
			{
				Vector3 calculated_normal = Vector3.Cross((points3[1] - points3[0]).normalized, (points3[2] - points3[0]).normalized);

				int p1 = 0;
				int p2 = 1;
				int p3 = 2;

				if(Vector3.Dot(calculated_normal, faceNormal) < 0){

					p1 = 2;
					p2 = 1;
					p3 = 0;
				}

				int base_index = vertices.Count;

				subIndices[submesh].Add(base_index);
				subIndices[submesh].Add(base_index+1);
				subIndices[submesh].Add(base_index+2);

				triangles.Add(base_index);
				triangles.Add(base_index+1);
				triangles.Add(base_index+2);

				vertices.Add(points3[p1]);
				vertices.Add(points3[p2]);
				vertices.Add(points3[p3]);

				normals.Add(normals3[p1]);
				normals.Add(normals3[p2]);
				normals.Add(normals3[p3]);

				uvs.Add(uvs3[p1]);
				uvs.Add(uvs3[p2]);
				uvs.Add(uvs3[p3]);
			}

		}

		private static MeshCutSide leftSide = new MeshCutSide();
		private static MeshCutSide rightSide = new MeshCutSide();

		private static Plane blade;
		private static Mesh targetMesh;

		// New vertices for capping the cutting surface
		private static List<Vector3> newVertices = new List<Vector3>();


		/// <summary>
		/// Cuts the specified target into two pieces and caps the sides with the specified Material.
		/// </summary>
		/// <param name="target">The target that is cut - must have a MeshFilter component.</param>
		/// <param name="anchorPoint">Anchor point of the blade i.e. the point where the blade cuts the object.</param>
		/// <param name="normalDirection">Normal direction of the blade plane.</param> 
		/// <param name="capMaterial">Cap material - placed as material on the cutting surface of the two sides.</param>
		/// <param name="copyRigidbody">Should the rigidbody be copied to both of the two new pieces.</param>
		/// <param name="copyRigidbody">Should both of the new pieces have colliders.</param>
		public static GameObject[] Cut(GameObject target, Vector3 anchorPoint, Vector3 normalDirection, Material capMaterial, bool copyRigidbody =true, bool copyCollider =true)
		{
			// set the blade relative to victim
			blade = new Plane(target.transform.InverseTransformDirection(-normalDirection),
				target.transform.InverseTransformPoint(anchorPoint));

			// get the victims mesh
			targetMesh = target.GetComponent<MeshFilter>().mesh;

			if (targetMesh == null)
			{
				Debug.LogError("MeshCut Cut: Target did not have a MeshFilter component - returning null");
				return null;
			}

			// reset values
			newVertices.Clear();
			leftSide.ClearAll();
			rightSide.ClearAll();

			bool[] sides = new bool[3];
			int[] indices;
			int   p1,p2,p3;

			// go throught the submeshes
			int subMeshCount = targetMesh.subMeshCount;

			for (int sub=0; sub < subMeshCount; ++sub)
			{
				indices = targetMesh.GetIndices(sub);

				leftSide.subIndices.Add(new List<int>());
				rightSide.subIndices.Add(new List<int>());

				for (int i=0; i<indices.Length; i+=3)
				{

					p1 = indices[i];
					p2 = indices[i+1];
					p3 = indices[i+2];

					sides[0] = blade.GetSide(targetMesh.vertices[p1]);
					sides[1] = blade.GetSide(targetMesh.vertices[p2]);
					sides[2] = blade.GetSide(targetMesh.vertices[p3]);


					// whole triangle
					if (sides[0] == sides[1] && sides[0] == sides[2]){

						if (sides[0])
						{
							// left side
							leftSide.AddTriangle(p1,p2,p3,sub);
						}
						else
						{
							rightSide.AddTriangle(p1,p2,p3,sub);
						}

					}
					else
					{
						// cut the triangle	
						CutFace(sub, sides, p1, p2, p3);
					}
				}
			}
				
			Material[] mats = target.GetComponent<MeshRenderer>().sharedMaterials;

			if (mats[mats.Length-1].name != capMaterial.name)
			{
				// Add cap indices
				leftSide.subIndices.Add(new List<int>());
				rightSide.subIndices.Add(new List<int>());

				Material[] newMats = new Material[mats.Length+1];
				mats.CopyTo(newMats, 0);
				newMats[mats.Length] = capMaterial;
				mats = newMats;
			}

			// Cap the openings
			Capping();

			// Left Mesh
			Mesh leftHalfMesh = new Mesh();
			leftHalfMesh.name =  "Split Mesh Left";
			leftHalfMesh.vertices  = leftSide.vertices.ToArray();
			leftHalfMesh.triangles = leftSide.triangles.ToArray();
			leftHalfMesh.normals   = leftSide.normals.ToArray();
			leftHalfMesh.uv        = leftSide.uvs.ToArray();

			leftHalfMesh.subMeshCount = leftSide.subIndices.Count;
			int leftHalfMeshSubMeshCount = leftHalfMesh.subMeshCount;

			for (int i=0; i < leftHalfMeshSubMeshCount; ++i)
			{
				leftHalfMesh.SetIndices(leftSide.subIndices[i].ToArray(), MeshTopology.Triangles, i);	
			}

			// Right Mesh
			Mesh rightHalfMesh = new Mesh();
			rightHalfMesh.name = "Split Mesh Right";
			rightHalfMesh.vertices  = rightSide.vertices.ToArray();
			rightHalfMesh.triangles = rightSide.triangles.ToArray();
			rightHalfMesh.normals   = rightSide.normals.ToArray();
			rightHalfMesh.uv        = rightSide.uvs.ToArray();

			rightHalfMesh.subMeshCount = rightSide.subIndices.Count;
			int rightHalfSubMeshCount = rightHalfMesh.subMeshCount;

			for (int i=0; i < rightHalfSubMeshCount; ++i)
			{
				rightHalfMesh.SetIndices(rightSide.subIndices[i].ToArray(), MeshTopology.Triangles, i);
			}

			// Assign the game objects
			target.name = "left side";
			target.GetComponent<MeshFilter>().mesh = leftHalfMesh;
			GameObject leftSideObj = target;

			GameObject rightSideObj = new GameObject("right side", typeof(MeshFilter), typeof(MeshRenderer));
			rightSideObj.transform.position = target.transform.position;
			rightSideObj.transform.rotation = target.transform.rotation;
			rightSideObj.transform.localScale = target.transform.localScale;
			rightSideObj.GetComponent<MeshFilter>().mesh = rightHalfMesh;

			// Maintain colliders and rigidbodies on both pieces i.e. populate the right piece
			Rigidbody leftSideRigidbody = leftSideObj.GetComponent<Rigidbody>();
			MeshCollider leftSideCollider = leftSideObj.GetComponent<MeshCollider>();

			if (copyRigidbody && leftSideRigidbody != null)
			{
				Rigidbody rigidbody = rightSideObj.AddComponent<Rigidbody>();
				rigidbody.useGravity = leftSideRigidbody.useGravity;
			}

			if (copyCollider && leftSideCollider != null)
			{
				MeshCollider meshCollider = rightSideObj.AddComponent<MeshCollider>();
				meshCollider.convex = true;
			}

			// Assign mats
			leftSideObj.GetComponent<MeshRenderer>().materials = mats;
			rightSideObj.GetComponent<MeshRenderer>().materials = mats;

			return new GameObject[]{ leftSideObj, rightSideObj };
		}


		private static void CutFace(int submesh, bool[] sides, int index1, int index2, int index3)
		{
			Vector3[] leftPoints = new Vector3[2];
			Vector3[] leftNormals = new Vector3[2];
			Vector2[] leftUvs = new Vector2[2];
			Vector3[] rightPoints = new Vector3[2];
			Vector3[] rightNormals = new Vector3[2];
			Vector2[] rightUvs = new Vector2[2];

			bool didset_left = false;
			bool didset_right = false;

			int p = index1;

			for (int side=0; side<3; side++)
			{
				switch (side)
				{
					case 0: p = index1; break;
					case 1: p = index2; break;
					case 2: p = index3; break;
				}

				if (sides[side])
				{
					if (!didset_left)
					{
						didset_left = true;

						leftPoints[0]   = targetMesh.vertices[p];
						leftPoints[1]   = leftPoints[0];
						leftUvs[0]     = targetMesh.uv[p];
						leftUvs[1]     = leftUvs[0];
						leftNormals[0] = targetMesh.normals[p];
						leftNormals[1] = leftNormals[0];

					}
					else
					{
						leftPoints[1]    = targetMesh.vertices[p];
						leftUvs[1]      = targetMesh.uv[p];
						leftNormals[1]  = targetMesh.normals[p];

					}
				}
				else
				{
					if (!didset_right)
					{
						didset_right = true;

						rightPoints[0]   = targetMesh.vertices[p];
						rightPoints[1]   = rightPoints[0];
						rightUvs[0]     = targetMesh.uv[p];
						rightUvs[1]     = rightUvs[0];
						rightNormals[0] = targetMesh.normals[p];
						rightNormals[1] = rightNormals[0];

					}
					else
					{
						rightPoints[1]   = targetMesh.vertices[p];
						rightUvs[1]     = targetMesh.uv[p];
						rightNormals[1] = targetMesh.normals[p];

					}
				}
			}
				
			float normalizedDistance = 0.0f;
			float distance = 0;
			blade.Raycast(new Ray(leftPoints[0], (rightPoints[0] - leftPoints[0]).normalized), out distance);

			normalizedDistance =  distance/(rightPoints[0] - leftPoints[0]).magnitude;
			Vector3 newVertex1 = Vector3.Lerp(leftPoints[0], rightPoints[0], normalizedDistance);
			Vector2 newUv1     = Vector2.Lerp(leftUvs[0], rightUvs[0], normalizedDistance);
			Vector3 newNormal1 = Vector3.Lerp(leftNormals[0] , rightNormals[0], normalizedDistance);

			newVertices.Add(newVertex1);

			blade.Raycast(new Ray(leftPoints[1], (rightPoints[1] - leftPoints[1]).normalized), out distance);

			normalizedDistance =  distance/(rightPoints[1] - leftPoints[1]).magnitude;
			Vector3 newVertex2 = Vector3.Lerp(leftPoints[1], rightPoints[1], normalizedDistance);
			Vector2 newUv2     = Vector2.Lerp(leftUvs[1], rightUvs[1], normalizedDistance);
			Vector3 newNormal2 = Vector3.Lerp(leftNormals[1] , rightNormals[1], normalizedDistance);

			newVertices.Add(newVertex2);


			leftSide.AddTriangle(new Vector3[]{leftPoints[0], newVertex1, newVertex2},
				new Vector3[]{leftNormals[0], newNormal1, newNormal2 },
				new Vector2[]{leftUvs[0], newUv1, newUv2}, newNormal1,
				submesh);

			leftSide.AddTriangle(new Vector3[]{leftPoints[0], leftPoints[1], newVertex2},
				new Vector3[]{leftNormals[0], leftNormals[1], newNormal2},
				new Vector2[]{leftUvs[0], leftUvs[1], newUv2}, newNormal2,
				submesh);

			rightSide.AddTriangle(new Vector3[]{rightPoints[0], newVertex1, newVertex2},
				new Vector3[]{rightNormals[0], newNormal1, newNormal2},
				new Vector2[]{rightUvs[0], newUv1, newUv2}, newNormal1,
				submesh);

			rightSide.AddTriangle(new Vector3[]{rightPoints[0], rightPoints[1], newVertex2},
				new Vector3[]{rightNormals[0], rightNormals[1], newNormal2},
				new Vector2[]{rightUvs[0], rightUvs[1], newUv2}, newNormal2,
				submesh);

		}

		private static List<Vector3> capVertTracker = new List<Vector3>();
		private static List<Vector3> capVertpolygon = new List<Vector3>();

		private static void Capping()
		{
			capVertTracker.Clear();

			for (int i=0; i<newVertices.Count; i++)
			{
				if (!capVertTracker.Contains(newVertices[i]))
				{
					capVertpolygon.Clear();
					capVertpolygon.Add(newVertices[i]);
					capVertpolygon.Add(newVertices[i+1]);

					capVertTracker.Add(newVertices[i]);
					capVertTracker.Add(newVertices[i+1]);


					bool isDone = false;
					while (!isDone)
					{
						isDone = true;

						for (int k=0; k<newVertices.Count; k+=2)
						{ 
							// go through the pairs
							if (newVertices[k] == capVertpolygon[capVertpolygon.Count-1] && !capVertTracker.Contains(newVertices[k+1]))
							{
								// if so add the other
								isDone = false;
								capVertpolygon.Add(newVertices[k+1]);
								capVertTracker.Add(newVertices[k+1]);

							}
							else if (newVertices[k+1] == capVertpolygon[capVertpolygon.Count-1] && !capVertTracker.Contains(newVertices[k]))
							{
								// if so add the other
								isDone = false;
								capVertpolygon.Add(newVertices[k]);
								capVertTracker.Add(newVertices[k]);
							}
						}
					}

					FillCap(capVertpolygon);
				}
			}

		}

		static void FillCap(List<Vector3> vertices)
		{
			// center of the cap
			Vector3 center = Vector3.zero;

			foreach (Vector3 point in vertices)
			{
				center += point;
			}

			center = center/vertices.Count;

			// you need an axis based on the cap
			Vector3 upward = Vector3.zero;
			// 90 degree turn
			upward.x = blade.normal.y;
			upward.y = -blade.normal.x;
			upward.z = blade.normal.z;
			Vector3 left = Vector3.Cross(blade.normal, upward);

			Vector3 displacement = Vector3.zero;
			Vector3 newUV1 = Vector3.zero;
			Vector3 newUV2 = Vector3.zero;

			for (int i=0; i<vertices.Count; i++)
			{
				displacement = vertices[i] - center;
				newUV1 = Vector3.zero;
				newUV1.x = 0.5f + Vector3.Dot(displacement, left);
				newUV1.y = 0.5f + Vector3.Dot(displacement, upward);
				newUV1.z = 0.5f + Vector3.Dot(displacement, blade.normal);

				displacement = vertices[(i+1) % vertices.Count] - center;
				newUV2 = Vector3.zero;
				newUV2.x = 0.5f + Vector3.Dot(displacement, left);
				newUV2.y = 0.5f + Vector3.Dot(displacement, upward);
				newUV2.z = 0.5f + Vector3.Dot(displacement, blade.normal);

				//	uvs.Add(new Vector2(relativePosition.x, relativePosition.y));
				//	normals.Add(blade.normal);

				leftSide.AddTriangle( new Vector3[]{
					vertices[i], vertices[(i+1) % vertices.Count], center
				},new Vector3[]{
					-blade.normal, -blade.normal, -blade.normal
				},new Vector2[]{
					newUV1, newUV2, new Vector2(0.5f, 0.5f)
				},-blade.normal, leftSide.subIndices.Count-1);

				rightSide.AddTriangle( new Vector3[]{
					vertices[i], vertices[(i+1) % vertices.Count], center
				},new Vector3[]{
					blade.normal, blade.normal, blade.normal
				},new Vector2[]{
					newUV1, newUV2, new Vector2(0.5f, 0.5f)
				},blade.normal, rightSide.subIndices.Count-1);
			}
		}
	}
}