using UnityEngine;

namespace AuroraPunks.ScriptableValues
{
	/// <summary>
	///     A <see cref="ScriptableList{T}" /> of <see cref="GameObject" />.
	/// </summary>
#if UNITY_EDITOR
	[CreateAssetMenu(fileName = "New Scriptable Game Object List", menuName = "Aurora Punks/Scriptable Values/Collections/Game Object List", order = ORDER)]
#endif
	public sealed class ScriptableGameObjectList : ScriptableList<GameObject> { }
}