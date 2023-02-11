﻿namespace AuroraPunks.ScriptableValues.Tests.Values
{
	public class ScriptableFloatValueTests : ScriptableValueTest<ScriptableFloat, float>
	{
		protected override float MakeDifferentValue(float value)
		{
			return value - 1;
		}
	}
}