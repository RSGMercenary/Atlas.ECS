﻿using Atlas.Core.Collections.LinkList;
using Atlas.Core.Extensions;
using Atlas.ECS.Components.Component;
using Atlas.ECS.Components.Engine.Entities;
using Atlas.ECS.Entities;
using Atlas.ECS.Families;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Atlas.ECS.Components.Engine.Families;

[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
internal sealed class FamilyManager : IFamilyManager
{
	#region Events
	public event Action<IFamilyManager, IFamily> Added;
	public event Action<IFamilyManager, IFamily> Removed;
	#endregion

	#region Fields
	private readonly LinkList<IFamily> families = new();
	private readonly Dictionary<Type, IReadOnlyFamily> types = new();
	private readonly Dictionary<Type, int> references = new();
	#endregion

	internal FamilyManager(IEngine engine)
	{
		Engine = engine;
		Engine.Entities.Added += EntityAdded;
		Engine.Entities.Removed += EntityRemoved;
	}

	public IEngine Engine { get; }

	private void EntityAdded(IEntityManager manager, IEntity entity)
	{
		foreach(var family in families)
			family.AddEntity(entity);
		entity.ComponentAdded += ComponentAdded;
		entity.ComponentRemoved += ComponentRemoved;
	}

	private void EntityRemoved(IEntityManager manager, IEntity entity)
	{
		entity.ComponentAdded -= ComponentAdded;
		entity.ComponentRemoved -= ComponentRemoved;
		foreach(var family in families)
			family.RemoveEntity(entity);
	}

	#region Construct
	public IFamilyConstructor Constructor { get; set; }

	private IFamily<TFamilyMember> CreateFamily<TFamilyMember>()
		where TFamilyMember : class, IFamilyMember, new() => Constructor != null ? Constructor.Construct<TFamilyMember>() : new AtlasFamily<TFamilyMember>();
	#endregion

	#region Add
	public IReadOnlyFamily<TFamilyMember> Add<TFamilyMember>()
		where TFamilyMember : class, IFamilyMember, new()
	{
		var type = typeof(TFamilyMember);
		if(!types.TryGetValue(type, out IFamily family))
		{
			family = CreateFamily<TFamilyMember>();

			families.Add(family);
			types.Add(type, family);
			references.Add(type, 0);
			family.Engine = Engine;

			foreach(var entity in Engine.Entities.Entities)
				family.AddEntity(entity);

			Added?.Invoke(this, family);
		}
		++references[type];
		return (IReadOnlyFamily<TFamilyMember>)types[type];
	}
	#endregion

	#region Remove
	public bool Remove<TFamilyMember>()
		where TFamilyMember : class, IFamilyMember, new()
	{
		var type = typeof(TFamilyMember);
		if(!types.TryGetValue(type, out IFamily family))
			return false;
		if(--references[type] > 0)
			return false;

		families.Remove(family);
		types.Remove(type);
		references.Remove(type);
		family.Engine = null;

		Removed?.Invoke(this, family);
		return true;
	}
	#endregion

	#region Get
	[JsonProperty]
	public IReadOnlyLinkList<IReadOnlyFamily> Families => families;

	public IReadOnlyDictionary<Type, IReadOnlyFamily> Types => types;

	public IReadOnlyFamily<TFamilyMember> Get<TFamilyMember>()
		where TFamilyMember : class, IFamilyMember, new()
	{
		return (IReadOnlyFamily<TFamilyMember>)Get(typeof(TFamilyMember));
	}

	public IReadOnlyFamily Get(Type type) => types.TryGetValue(type, out var family) ? family : null;
	#endregion

	#region Has
	public bool Has<TFamilyMember>()
		where TFamilyMember : class, IFamilyMember, new()
	{
		return Has(typeof(TFamilyMember));
	}

	public bool Has(Type type) => types.ContainsKey(type);

	public bool Has(IReadOnlyFamily family) => types.ContainsValue(family);
	#endregion

	#region Listeners
	private void ComponentAdded(IEntity entity, IComponent component, Type type)
	{
		foreach(var family in families)
			family.AddEntity(entity, type);
	}

	private void ComponentRemoved(IEntity entity, IComponent component, Type type)
	{
		foreach(var family in families)
			family.RemoveEntity(entity, type);
	}
	#endregion
}