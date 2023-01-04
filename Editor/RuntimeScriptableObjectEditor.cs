﻿using System.Collections.Generic;
using AuroraPunks.ScriptableValues.Debugging;
using AuroraPunks.ScriptableValues.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace AuroraPunks.ScriptableValues
{
	public abstract class RuntimeScriptableObjectEditor : UnityEditor.Editor
	{
		private SerializedProperty collectStackTraces;
		private StackTraceElement stackTraces;

		private bool hasCreatedDefaultInspector;

		protected virtual string StackTracesLabel { get { return "Invocation Stack Traces"; } }

		protected virtual void OnEnable()
		{
			hasCreatedDefaultInspector = false;
			collectStackTraces = serializedObject.FindProperty(nameof(collectStackTraces));

			((IStackTraceProvider) target).OnStackTraceAdded += OnStackTraceAdded;

			GatherProperties();
		}

		protected virtual void OnDisable()
		{
			((IStackTraceProvider) target).OnStackTraceAdded -= OnStackTraceAdded;
			stackTraces?.Dispose();
		}

		protected virtual void GatherProperties() { }

		protected virtual void OnStackTraceAdded() { }

		public override VisualElement CreateInspectorGUI()
		{
			VisualElement root = new EntireInspectorElement();
			CreateGUIBeforeStackTraces(root);
			CreateDefaultInspectorGUI(root);

			stackTraces = new StackTraceElement((IStackTraceProvider) target, collectStackTraces, StackTracesLabel)
			{
				style =
				{
					marginTop = 4
				}
			};

			root.Add(stackTraces);

			return root;
		}
		
		protected void CreateDefaultInspectorGUI(VisualElement root)
		{
			if (hasCreatedDefaultInspector)
			{
				return;
			}
			
			SerializedProperty iterator = serializedObject.GetIterator();
			bool enterChildren = true;

			List<SerializedProperty> ignoreProperties = new List<SerializedProperty>()
			{
				collectStackTraces
			};

			HashSet<string> ignorePropertyNames = new HashSet<string>()
			{
				"m_Script"
			};

			GetExcludingProperties(ignoreProperties);
			
			foreach (SerializedProperty property in ignoreProperties)
			{
				ignorePropertyNames.Add(property.propertyPath);
			}

			while (iterator.NextVisible(enterChildren))
			{
				enterChildren = false;
				if (ignorePropertyNames.Contains(iterator.name))
				{
					continue;
				}

				root.Add(new PropertyField(iterator));
			}

			hasCreatedDefaultInspector = true;
		}

		protected virtual void CreateGUIBeforeStackTraces(VisualElement root) { }
		
		protected virtual void GetExcludingProperties(List<SerializedProperty> properties) { }
	}
}