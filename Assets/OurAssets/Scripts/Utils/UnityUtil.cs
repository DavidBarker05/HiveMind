using System.Runtime.CompilerServices;
using UnityEngine;

namespace Util
{
	namespace UnityUtils
	{
		public static class UnityUtil
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static GameObject GetParentObject(this GameObject go) => go.transform.parent.gameObject;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static GameObject GetParentObject(this MonoBehaviour mb) => mb.transform.parent.gameObject;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static GameObject GetGameObject(this RaycastHit hitInfo) => hitInfo.collider.gameObject;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static T GetComponent<T>(this RaycastHit hitInfo)
			{
				return hitInfo.collider.gameObject.GetComponent<T>();
				GameObject go = hitInfo.collider.gameObject;
				T comp;
				do
				{
					comp = go.GetComponent<T>();
					go = go.transform.parent.gameObject;
				} while (comp == null && go.transform.parent != null);
				return comp;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static T GetComponent<T>(this Collider collider) => collider.gameObject.GetComponent<T>();

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static bool IsNegativeInfinity(this Vector3 v) => v.Equals(Vector3.negativeInfinity);
		}
	}
}
