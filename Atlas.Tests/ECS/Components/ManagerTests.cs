﻿using Atlas.ECS.Components.Component;
using Atlas.ECS.Entities;
using Atlas.Tests.Attributes;
using Atlas.Tests.Testers.Components;
using Atlas.Tests.Testers.Utilities;
using NUnit.Framework;
using System;
using System.Collections;
using System.Linq;

namespace Atlas.Tests.ECS.Components;

[TestFixture]
class ManagerTests
{
	private Random Random = new Random();

	#region Add
	[Test]
	[Repeat(10)]
	public void When_AddManagers_Then_ManagersEqualsCount()
	{
		var count = Random.Next(11);

		var component = new TestComponent(true);

		for(var i = 0; i < count; ++i)
			component.AddManager(new AtlasEntity());

		foreach(var entity in component)
			Assert.That(entity.Components.Count == 1);

		Assert.That(component.Managers.Count == count);
	}

	[Test]
	public void When_AddManager_Then_HasManager()
	{
		var component = new TestComponent();
		var entity = new AtlasEntity();

		component.AddManager(entity);

		Assert.That(entity.GetComponent<TestComponent>() == component);
		Assert.That(component.HasManager(entity));
	}

	[TestCase<ITestComponent, TestComponent>]
	[TestCase<TestComponent, TestComponent>]
	public void When_AddManager_WithType_Then_HasManager<TType, TComponent>()
		where TType : class, IComponent
		where TComponent : class, TType, new()
	{
		var component = new TComponent();
		var entity = new AtlasEntity();

		component.AddManager<TType>(entity);

		Assert.That(entity.GetComponent<TType>() == component);
		Assert.That(component.HasManager(entity));
	}

	[Test]
	[Repeat(10)]
	public void When_AddManager_WithTypeAndIndex_Then_HasManager()
	{
		var count = Random.Next(11);
		var index = Random.Next(count + 1);

		var component = new TestComponent(true);
		var entity = new AtlasEntity();

		for(var i = 0; i < count; ++i)
			component.AddManager(new AtlasEntity());

		component.AddManager<TestComponent>(entity, index);

		Assert.That(entity.GetComponent<TestComponent>() == component);
		Assert.That(component.Managers[index] == entity);
	}

	[Test]
	[Repeat(10)]
	public void When_AddManager_Twice_Then_ManagerAdded()
	{
		var count = Random.Next(2, 11);
		var index1 = Random.Next(count);
		var index2 = Random.NextExclude(count, index1);

		var component = new TestComponent(true);
		var entity = new AtlasEntity();

		for(var i = 0; i < count; ++i)
			component.AddManager(new AtlasEntity());

		component.AddManager(entity, index1);
		component.AddManager(entity, index2);

		Assert.That(component.Managers[index1] != entity);
		Assert.That(component.Managers[index2] == entity);
	}
	#endregion

	#region Remove
	[Test]
	[Repeat(10)]
	public void When_RemoveManagers_Then_ManagersEqualsZero()
	{
		var count = Random.Next(2, 11);

		var component = new TestComponent(true);

		for(var i = 0; i < count; ++i)
			component.AddManager(new AtlasEntity());

		foreach(var entity in component.Managers)
		{
			component.RemoveManager(entity);
			Assert.That(entity.Components.Count == 0);
		}

		Assert.That(component.Managers.Count == 0);
	}

	[TestCase<ITestComponent, TestComponent>]
	[TestCase<TestComponent, TestComponent>]
	public void When_RemoveManager_Then_ManagerRemoved<TType, TComponent>()
		where TType : class, IComponent
		where TComponent : class, TType, new()
	{
		var component = new TComponent();
		var entity = new AtlasEntity();

		component.AddManager<TType>(entity);
		component.RemoveManager<TType>(entity);

		Assert.That(entity.GetComponent<TType>() == null);
		Assert.That(!component.HasManager(entity));
	}

	[Test]
	public void When_RemoveManager_AsManager_Then_ManagerRemove()
	{
		var component = new TestComponent();
		var entity = new AtlasEntity();

		component.AddManager(entity);
		component.RemoveManager(entity);

		Assert.That(entity.GetComponent<TestComponent>() == null);
		Assert.That(!component.HasManager(entity));
	}

	[Test]
	public void When_RemoveManager_AsIndex_Then_ManagerRemove()
	{
		var component = new TestComponent();
		var entity = new AtlasEntity();

		component.AddManager(entity);
		component.RemoveManager(0);

		Assert.That(entity.GetComponent<TestComponent>() == null);
		Assert.That(!component.HasManager(entity));
	}

	[Test]
	public void When_RemoveManager_NotAdded_Then_ManagerEqualsNull()
	{
		var component = new TestComponent();
		var entity = new AtlasEntity();

		Assert.That(component.RemoveManager(entity) == null);
	}

	[TestCase(0)]
	[TestCase(1)]
	[TestCase(2)]
	[TestCase(7)]
	[TestCase(11)]
	public void When_RemoveManagers_Then_ManagersRemoved(int count)
	{
		var component = new TestComponent(true);
		var managers = Enumerable.Range(0, count).Select(i => new AtlasEntity()).ToList();

		foreach(var manager in managers)
			component.AddManager(manager);

		component.RemoveManagers();

		foreach(var manager in managers)
			Assert.That(manager.Components.Count == 0);

		Assert.That(component.Managers.Count == 0);
	}
	#endregion

	#region Swap / Set
	[Test]
	[Repeat(10)]
	public void When_SwapManagers_AsIndex_Then_ManagersSwapped()
	{
		var count = Random.Next(2, 11);
		var index1 = Random.Next(count);
		var index2 = Random.NextExclude(count, index1);

		var component = new TestComponent(true);

		for(var i = 0; i < count; ++i)
			component.AddManager(new AtlasEntity());

		var entity1 = component.Managers[index1];
		var entity2 = component.Managers[index2];

		Assert.That(component.SwapManagers(index1, index2));
		Assert.That(component.Managers[index1] == entity2);
		Assert.That(component.Managers[index2] == entity1);
	}

	[Test]
	[Repeat(10)]
	public void When_SwapManagers_AsEntity_Then_ManagersSwapped()
	{
		var count = Random.Next(2, 11);
		var index1 = Random.Next(count);
		var index2 = Random.NextExclude(count, index1);

		var component = new TestComponent(true);

		for(var i = 0; i < count; ++i)
			component.AddManager(new AtlasEntity());

		var entity1 = component.Managers[index1];
		var entity2 = component.Managers[index2];

		component.SwapManagers(entity1, entity2);

		Assert.That(component.GetManagerIndex(entity1) == index2);
		Assert.That(component.GetManagerIndex(entity2) == index1);
	}

	[Test]
	[Repeat(10)]
	public void When_SwapManagers_WithInvalidIndices_Then_NoManagersSwapped()
	{
		var count = Random.Next(2, 11);
		var index1 = Random.Next(-10, 0);
		var index2 = Random.Next(count, count + 10);

		var component = new TestComponent(true);

		for(var i = 0; i < count; ++i)
			component.AddManager(new AtlasEntity());

		Assert.That(!component.SwapManagers(index1, index2));
	}

	[TestCase(true, false)]
	[TestCase(false, true)]
	[TestCase(false, false)]
	[TestCase(null, null)]
	[TestCase(true, null)]
	[TestCase(false, null)]
	[TestCase(null, true)]
	[TestCase(null, false)]
	public void When_SwapManagers_WithInvalidManager_Then_NoManagersSwapped(bool? add1, bool? add2)
	{
		var component = new TestComponent(true);
		var entity1 = null as AtlasEntity;
		var entity2 = null as AtlasEntity;

		for(var i = 0; i < 5; ++i)
			component.AddManager(new AtlasEntity());

		if(add1.HasValue)
		{
			entity1 = new AtlasEntity();
			if(add1.Value)
				component.AddManager(entity1);
		}

		if(add2.HasValue)
		{
			entity2 = new AtlasEntity();
			if(add2.Value)
				component.AddManager(entity2);
		}

		Assert.That(!component.SwapManagers(entity1, entity2));
	}

	[Test]
	[Repeat(10)]
	public void When_SetManagerIndex_Then_IndexExpected()
	{
		var count = Random.Next(2, 11);
		var index1 = Random.Next(count);
		var index2 = Random.NextExclude(count, index1);

		var component = new TestComponent(true);

		for(var i = 0; i < count; ++i)
			component.AddManager(new AtlasEntity());

		var manager = component.Managers[index1];

		component.SetManagerIndex(manager, index2);

		Assert.That(component.Managers[index1] != manager);
		Assert.That(component.Managers[index2] == manager);
		Assert.That(component.Managers.Count == count);
	}

	[TestCase(2, 0)]
	[TestCase(5, 4)]
	[TestCase(8, 2)]
	[TestCase(12, 9)]
	public void When_SetManagerIndex_WithInvalidManager_Then_ReturnFalse(int count, int index)
	{
		var component = new TestComponent(true);
		var manager = new AtlasEntity();

		for(var i = 0; i < count; ++i)
			component.AddManager(new AtlasEntity());

		Assert.That(!component.SetManagerIndex(manager, index));
	}
	#endregion

	#region IsShareable
	[TestCase(true, 2)]
	[TestCase(false, 1)]
	public void When_AddManager_With_IsShareable_Then_CountExpected(bool isShareable, int count)
	{
		var component = new TestComponent(isShareable);
		component.AddManager(new AtlasEntity());
		component.AddManager(new AtlasEntity());

		Assert.That(component.Managers.Count == count);
	}

	[TestCase(true)]
	[TestCase(false)]
	public void When_AddManager_With_IsShareable_Then_ManagerExpected(bool isShareable)
	{
		var component = new TestComponent(isShareable);
		var entity = new AtlasEntity();

		component.AddManager(entity);

		Assert.That(component.Manager == (isShareable ? null : entity));
	}
	#endregion

	[Test]
	public void When_IterateManagers_Then_ContainsManagers()
	{
		var component = new TestComponent(true);
		var managers = Enumerable.Range(0, 10).Select(i => new AtlasEntity()).ToList();

		foreach(var manager in managers)
			component.AddManager(manager);

		foreach(var manager in (IEnumerable)component)
			Assert.That(managers.Contains(manager));
	}
}