﻿using Atlas.Core.Collections.Group;
using Atlas.ECS.Components.Component;
using Atlas.ECS.Components.Engine;
using Atlas.ECS.Entities;
using Atlas.ECS.Systems;
using System;
using System.Collections.Generic;

namespace Atlas.ECS.Components.SystemProvider;

public class AtlasSystemProvider : AtlasComponent<ISystemProvider>, ISystemProvider
{
	public event Action<ISystemProvider, Type> SystemAdded;
	public event Action<ISystemProvider, Type> SystemRemoved;

	private readonly Group<Type> types = new();

	public AtlasSystemProvider() { }

	public AtlasSystemProvider(params Type[] types) : this(types as IEnumerable<Type>) { }

	public AtlasSystemProvider(IEnumerable<Type> types)
	{
		foreach(var type in types)
			AddSystem(type);
	}

	protected override void Disposing()
	{
		RemoveSystems();
		base.Disposing();
	}

	#region Engine/Systems
	protected override void AddingManager(IEntity entity, int index)
	{
		base.AddingManager(entity, index);
		entity.EngineChanged += UpdateSystems;
		UpdateSystems(entity.Engine, true);
	}

	protected override void RemovingManager(IEntity entity, int index)
	{
		entity.EngineChanged -= UpdateSystems;
		UpdateSystems(entity.Engine, false);
		base.RemovingManager(entity, index);
	}

	private void UpdateSystems(IEntity entity, IEngine current, IEngine previous)
	{
		UpdateSystems(previous, false);
		UpdateSystems(current, true);
	}

	private void UpdateSystems(IEngine engine, bool add)
	{
		if(engine == null)
			return;
		foreach(var type in types)
		{
			if(add)
				engine.Systems.Add(type);
			else
				engine.Systems.Remove(type);
		}
	}
	#endregion

	#region Get
	public IReadOnlyGroup<Type> Systems => types;
	#endregion

	#region Has
	public bool HasSystem<TKey>() where TKey : class, ISystem => HasSystem(typeof(TKey));

	public bool HasSystem(Type type) => types.Contains(type);
	#endregion

	#region Add
	public bool AddSystem<TKey>() where TKey : class, ISystem => AddSystem(typeof(TKey));

	public bool AddSystem(Type type)
	{
		if(type == null)
			return false;
		if(!type.IsAssignableTo(typeof(ISystem)))
			return false;
		if(types.Contains(type))
			return false;
		types.Add(type);
		Manager?.Engine?.Systems.Add(type);
		SystemAdded?.Invoke(this, type);
		return true;
	}
	#endregion

	#region Remove
	public bool RemoveSystem<TKey>() where TKey : class, ISystem => RemoveSystem(typeof(TKey));

	public bool RemoveSystem(Type type)
	{
		if(type == null)
			return false;
		if(!types.Contains(type))
			return false;
		types.Remove(type);
		Manager?.Engine?.Systems.Remove(type);
		SystemRemoved?.Invoke(this, type);
		return true;
	}

	public bool RemoveSystems()
	{
		if(types.Count <= 0)
			return false;
		foreach(var type in types)
			RemoveSystem(type);
		return true;
	}
	#endregion
}