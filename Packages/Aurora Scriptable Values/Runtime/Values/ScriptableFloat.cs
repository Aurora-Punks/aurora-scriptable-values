﻿using UnityEngine;

namespace AuroraPunks.ScriptableValues
{
#if UNITY_EDITOR
	[CreateAssetMenu(fileName = "New Scriptable Float", menuName = "Aurora Punks/Scriptable Values/Values/Float Value")]
#endif
	public sealed class ScriptableFloat : ScriptableValue<float> { }
}