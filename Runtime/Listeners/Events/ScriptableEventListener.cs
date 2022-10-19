﻿using System;
using UnityEngine;
using UnityEngine.Events;

namespace AuroraPunks.ScriptableValues
{
#if UNITY_EDITOR
	[AddComponentMenu("Scriptable Values/Listeners/Events/Scriptable Event Listener", 1100)]
#endif
	public class ScriptableEventListener : MonoBehaviour
	{
		[SerializeField] 
		private ScriptableEvent targetEvent = default;
		[SerializeField] 
		private StartListenEvents startListening = StartListenEvents.Awake;
		[SerializeField] 
		private StopListenEvents stopListening = StopListenEvents.OnDestroy;
		[SerializeField] 
		private UnityEvent onInvoked = new UnityEvent();

		protected bool isListening = false;
		
		public ScriptableEvent TargetEvent { get { return targetEvent; } set { targetEvent = value; } }
		public StartListenEvents StartListening { get { return startListening; } set { startListening = value; } }
		public StopListenEvents StopListening { get { return stopListening; } set { stopListening = value; } }

		public UnityEvent OnInvoked { get { return onInvoked; } }
		
		protected virtual void Awake()
		{
			isListening = false;

			if (!isListening && startListening == StartListenEvents.Awake)
			{
				ToggleListening(true);
			}
		}
		
		protected void Start()
		{
			if (!isListening && startListening == StartListenEvents.Start)
			{
				ToggleListening(true);
			}
		}

		protected virtual void OnEnable()
		{
			if (!isListening && startListening == StartListenEvents.OnEnable)
			{
				ToggleListening(true);
			}
		}

		protected virtual void OnDisable()
		{
			if (isListening && stopListening == StopListenEvents.OnDisable)
			{
				ToggleListening(false);
			}
		}

		protected virtual void OnDestroy()
		{
			if (isListening && stopListening == StopListenEvents.OnDestroy)
			{
				ToggleListening(false);
			}
		}

		protected virtual void ToggleListening(bool listen)
		{
			isListening = listen;

			if (listen)
			{
				targetEvent.OnInvoked += OnEventInvoked;
			}
			else
			{
				targetEvent.OnInvoked -= OnEventInvoked;
			}
		}

		private void OnEventInvoked(object sender, EventArgs e)
		{
			onInvoked.Invoke();
		}
	}
}