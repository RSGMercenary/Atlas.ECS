﻿using Atlas.Core.Collections.Group;
using Atlas.ECS.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Atlas.ECS.Components.Engine.Entities;

internal class EntityManager : IEntityManager
{
	public event Action<IEntityManager, IEntity> Added;
	public event Action<IEntityManager, IEntity> Removed;

	private readonly Group<IEntity> entities = new();
	private readonly Dictionary<string, IEntity> globalNames = new();

	public IEngine Engine { get; }

	internal EntityManager(IEngine engine)
	{
		Engine = engine;
	}

	#region Add/Remove
	internal void AddEntity(IEntity entity)
	{
		//Change the Entity's global name if it already exists.
		if(globalNames.ContainsKey(entity.GlobalName))
			entity.GlobalName = AtlasEntity.UniqueName;

		globalNames[entity.GlobalName] = entity;
		entities.Add(entity);
		entity.Engine = Engine;

		entity.ChildAdded += ChildAdded;
		entity.RootChanged += RootChanged;
		entity.GlobalNameChanged += GlobalNameChanged;

		Added?.Invoke(this, entity);

		foreach(var child in entity.Children.Forward())
			AddEntity(child);
	}

	internal void RemoveEntity(IEntity entity)
	{
		//Protect against parents signaling a child being removed which never got to be added.
		if(!globalNames.TryGetValue(entity.GlobalName, out var global) || global != entity)
			return;

		entity.ChildAdded -= ChildAdded;
		entity.RootChanged -= RootChanged;
		entity.GlobalNameChanged -= GlobalNameChanged;

		foreach(var child in entity.Children.Backward())
			RemoveEntity(child);

		Removed?.Invoke(this, entity);

		globalNames.Remove(entity.GlobalName);
		entities.Remove(entity);
		entity.Engine = null;
	}
	#endregion

	#region Get
	[JsonIgnore]
	public IReadOnlyGroup<IEntity> Entities => entities;

	public IReadOnlyDictionary<string, IEntity> GlobalNames => globalNames;

	public IEntity Get(string globalName) => globalNames.TryGetValue(globalName, out var entity) ? entity : null;
	#endregion

	#region Has
	public bool Has(string globalName) => globalNames.ContainsKey(globalName);

	public bool Has(IEntity entity) => globalNames.TryGetValue(entity.GlobalName, out var global) && global == entity;
	#endregion

	#region Listeners
	private void ChildAdded(IEntity parent, IEntity child, int index)
	{
		if(!globalNames.TryGetValue(child.GlobalName, out var entity) || entity != child)
			AddEntity(child);
	}

	private void RootChanged(IEntity entity, IEntity current, IEntity previous)
	{
		if(entity == Engine.Manager)
			entity.RemoveComponentType(Engine);
		else if(current == null)
			RemoveEntity(entity);
	}

	private void GlobalNameChanged(IEntity entity, string current, string previous)
	{
		if(globalNames.TryGetValue(previous, out var global) && global == entity)
		{
			globalNames.Remove(previous);
			globalNames.Add(current, entity);
		}
	}
	#endregion
}