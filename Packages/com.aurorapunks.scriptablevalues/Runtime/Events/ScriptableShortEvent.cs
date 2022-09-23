﻿using UnityEngine;

namespace AuroraPunks.ScriptableValues
{
#if UNITY_EDITOR
	[CreateAssetMenu(fileName = "New Scriptable Short Event", menuName = "Aurora Punks/Scriptable Values/Events/Short Event", order = 1103)]
#endif
	public sealed class ScriptableShortEvent : ScriptableEvent<short> { }
}