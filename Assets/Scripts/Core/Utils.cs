// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;

namespace OperationBlackwell.Core {
	public class Utils {
		// Graciously taken from the interwebs, draws a line like Debug.DrawLine does.
		public static void DrawLine(Vector3 start, Vector3 end, float duration = 0.2f) {
			GameObject myLine = new GameObject();
			myLine.transform.position = start;
			myLine.AddComponent<LineRenderer>();
			myLine.name = "GridLine" + start.x + "|" + start.z + "|" + end.x + "|" + end.z;
			LineRenderer lr = myLine.GetComponent<LineRenderer>();
			lr.material = (Material)Resources.Load("Materials/Line");
			lr.startColor = Color.white;
			lr.endColor = Color.white;
			lr.SetWidth(0.1f, 0.1f);
			lr.SetPosition(0, start);
			lr.SetPosition(1, end);
		}

		/*
		* CodeMonkey code, grab me some 3d mouse position.
		* Requires a collider below the play grid for our use case!
		* TODO: This collider grid is hacked in now, make this nicer.
		*/
		public static Vector3 GetMouseWorldPosition3d() {
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if(Physics.Raycast(ray, out RaycastHit raycastHit, 999f)) {
				return raycastHit.point;
			} else {
				return Vector3.zero;
			}
		}

		/*
		 * The following functions are taken from CodeMonkey.
		 * They come from the MeshUtils class in his utilities, but to
		 * avoid polluting the codebase with everything, we grab only what we need.
		 */
		public static void CreateEmptyMeshArrays(int quadCount, out Vector3[] vertices, out Vector2[] uvs, out int[] triangles) {
			vertices = new Vector3[4 * quadCount];
			uvs = new Vector2[4 * quadCount];
			triangles = new int[6 * quadCount];
		}

		private static Quaternion[] cachedQuaternionEulerArr_;
		private static void CacheQuaternionEuler() {
			if(cachedQuaternionEulerArr_ != null) {
				return;
			}
			cachedQuaternionEulerArr_ = new Quaternion[360];
			for(int i = 0; i < 360; i++) {
				cachedQuaternionEulerArr_[i] = Quaternion.Euler(0, 0, i);
			}
		}

		private static Quaternion GetQuaternionEuler(float rotFloat) {
			int rot = Mathf.RoundToInt(rotFloat);
			rot = rot % 360;
			if(rot < 0) {
				rot += 360;
			}
			//if (rot >= 360) rot -= 360;
			if(cachedQuaternionEulerArr_ == null) {
				CacheQuaternionEuler();
			}
			return cachedQuaternionEulerArr_[rot];
		}

		public static void AddToMeshArrays(Vector3[] vertices, Vector2[] uvs, int[] triangles, int index, Vector3 pos, float rot, Vector3 baseSize, Vector2 uv00, Vector2 uv11) {
			// Relocate vertices.
			int vIndex = index * 4;
			int vIndex0 = vIndex;
			int vIndex1 = vIndex + 1;
			int vIndex2 = vIndex + 2;
			int vIndex3 = vIndex + 3;

			baseSize *= .5f;

			bool skewed = baseSize.x != baseSize.y;
			if(skewed) {
				vertices[vIndex0] = pos + GetQuaternionEuler(rot) * new Vector3(-baseSize.x, 0, baseSize.z);
				vertices[vIndex1] = pos + GetQuaternionEuler(rot) * new Vector3(-baseSize.x, 0, -baseSize.z);
				vertices[vIndex2] = pos + GetQuaternionEuler(rot) * new Vector3(baseSize.x, 0, -baseSize.z);
				vertices[vIndex3] = pos + GetQuaternionEuler(rot) * baseSize;
			} else {
				vertices[vIndex0] = pos + GetQuaternionEuler(rot - 270) * baseSize;
				vertices[vIndex1] = pos + GetQuaternionEuler(rot - 180) * baseSize;
				vertices[vIndex2] = pos + GetQuaternionEuler(rot - 90) * baseSize;
				vertices[vIndex3] = pos + GetQuaternionEuler(rot - 0) * baseSize;
			}

			// Relocate UVs.
			uvs[vIndex0] = new Vector2(uv00.x, uv11.y);
			uvs[vIndex1] = new Vector2(uv00.x, uv00.y);
			uvs[vIndex2] = new Vector2(uv11.x, uv00.y);
			uvs[vIndex3] = new Vector2(uv11.x, uv11.y);

			// Create triangles.
			int tIndex = index * 6;

			triangles[tIndex + 0] = vIndex0;
			triangles[tIndex + 1] = vIndex3;
			triangles[tIndex + 2] = vIndex1;

			triangles[tIndex + 3] = vIndex1;
			triangles[tIndex + 4] = vIndex3;
			triangles[tIndex + 5] = vIndex2;
		}
	}
}
