using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AuroraPunks.ScriptableValues
{
	public abstract partial class RuntimeScriptableObject : ScriptableObject
	{
		private void OnEnable()
		{
#if UNITY_EDITOR
			EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#else
            ResetValues();
#endif
		}

		public virtual void ResetValues() { }

#if UNITY_EDITOR
		private void OnDisable()
		{
			EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
		}

		private void OnPlayModeStateChanged(PlayModeStateChange state)
		{
			switch (state)
			{
				case PlayModeStateChange.ExitingEditMode:
					ResetValues();
					break;
				case PlayModeStateChange.EnteredEditMode:
					OnExitPlayMode();
					break;
			}
		}

		/// <summary>
		///     Internal method for calling OnExitPlayMode in tests.
		/// </summary>
		internal void Test_ExitPlayMode()
		{
			OnExitPlayMode();
		}

		protected virtual void OnExitPlayMode() { }
#endif
	}
}