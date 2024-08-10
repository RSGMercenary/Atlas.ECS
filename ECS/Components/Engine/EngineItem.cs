﻿using Atlas.Core.Messages;
using System;

namespace Atlas.ECS.Components.Engine;

internal class EngineItem<T> : IEngineItem
	where T : IEngineItem, IMessenger<T>
{
	private readonly T Instance;
	private readonly Func<IEngine, T, bool> Condition;
	private readonly Action<IEngine, IEngine> Listener;
	private IEngine engine;

	public EngineItem(T instance, Func<IEngine, T, bool> condition, Action<IEngine, IEngine> listener = null)
	{
		Instance = instance;
		Condition = condition;
		Listener = listener;
	}

	public IEngine Engine
	{
		get => engine;
		set
		{
			if(!(value != null && engine == null && Condition(value, Instance)) &&
				!(value == null && engine != null && !Condition(engine, Instance)))
				return;
			var previous = engine;
			engine = value;
			Listener?.Invoke(value, previous);
			Instance.Message<IEngineMessage<T>>(new EngineMessage<T>(value, previous));
		}
	}
}