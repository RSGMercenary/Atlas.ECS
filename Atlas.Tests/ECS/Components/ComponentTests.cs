﻿using Atlas.ECS.Components.Component;
using Atlas.ECS.Components.Engine;
using Atlas.ECS.Entities;
using Atlas.Tests.Attributes;
using Atlas.Tests.Testers.Components;
using NUnit.Framework;
using System;
using System.Linq;

namespace Atlas.Tests.ECS.Components;

[TestFixture]
internal class ComponentTests
{
	#region Add
	[Test]
	public void When_AddComponent_As_Component_Then_ComponentAdded()
	{
		var entity = new AtlasEntity();
		var component = new TestComponent();

		entity.AddComponent(component);

		Assert.That(entity.GetComponent(component.GetType()) == component);
	}

	[TestCase(typeof(TestComponent))]
	[TestCase(typeof(ITestComponent))]
	public void When_AddComponent_As_Type_Then_ComponentAdded(Type type)
	{
		var entity = new AtlasEntity();
		var component = new TestComponent();

		entity.AddComponent(component, type);

		Assert.That(entity.GetComponent(type) == component);
	}

	[TestCase<ITestComponent, TestComponent>]
	[TestCase<TestComponent, TestComponent>]
	public void When_AddComponent_As_GenericType_Then_ComponentAdded<TType, TComponent>()
		where TType : class, IComponent
		where TComponent : class, TType, new()
	{
		var entity = new AtlasEntity();
		var component = new TComponent();

		entity.AddComponent<TType>(component);

		Assert.That(entity.GetComponent<TType>() == component);
	}

	[TestCase<ITestComponent, TestComponent>]
	[TestCase<TestComponent, TestComponent>]
	public void When_AddComponent_As_GenericTypeAndComponent_Then_ComponentAdded<TType, TComponent>()
		where TType : class, IComponent
		where TComponent : class, TType, new()
	{
		var entity = new AtlasEntity();
		var component = new TComponent();

		entity.AddComponent<TComponent, TType>(component);

		Assert.That(entity.GetComponent<TComponent, TType>() == component);
	}

	[TestCase<TestComponent>]
	public void When_AddComponent_As_GenericNew_Then_ComponentAdded<TComponent>()
		where TComponent : class, IComponent, new()
	{
		var entity = new AtlasEntity();
		var component = entity.AddComponent<TComponent>();

		Assert.That(entity.GetComponent<TComponent>() == component);
	}

	[TestCase<ITestComponent, TestComponent>]
	public void When_AddComponent_As_GenericTypeAndNew_Then_ComponentAdded<TType, TComponent>()
		where TType : class, IComponent
		where TComponent : class, TType, new()
	{
		var entity = new AtlasEntity();
		var component = entity.AddComponent<TComponent, TType>();

		Assert.That(entity.GetComponent<TType>() == component);
		Assert.That(entity.GetComponent(typeof(TType)) == component);
		Assert.That(entity.GetComponent<TComponent>(typeof(TType)) == component);
		Assert.That(entity.GetComponent<TComponent, TType>() == component);
	}

	[Test]
	public void When_AddComponent_TwiceSame_Then_ComponentSame()
	{
		var entity = new AtlasEntity();
		var component = new TestComponent();
		var type = component.GetType();

		entity.AddComponent(component);
		entity.AddComponent(component);

		Assert.That(entity.GetComponent(type) == component);
	}

	[Test]
	public void When_AddComponent_TwiceDifferent_Then_ComponentDifferent()
	{
		var entity = new AtlasEntity();
		var component1 = new TestComponent();
		var component2 = new TestComponent();
		var type = component1.GetType();

		entity.AddComponent(component1);
		entity.AddComponent(component2);

		Assert.That(entity.GetComponent(type) == component2);
	}

	[Test]
	[Repeat(20)]
	public void When_AddComponent_As_Index_Then_ManagerAtIndex()
	{
		var entity = new AtlasEntity();
		var component = new TestComponent(true);
		var random = new Random();

		for(var i = random.Next(0, 10); i > 0; --i)
			component.AddManager(new AtlasEntity());

		var index = random.Next(0, component.Managers.Count + 1);

		entity.AddComponent(component, index);

		Assert.That(component.Managers[index] == entity);
	}

	[Test]
	[Repeat(20)]
	public void When_AddComponent_As_IndexAndTypes_Then_ManagerAtIndex()
	{
		var entity = new AtlasEntity();
		var component = new TestComponent(true);
		var random = new Random();

		for(var i = random.Next(0, 10); i > 0; --i)
			component.AddManager<ITestComponent>(new AtlasEntity());

		var index = random.Next(0, component.Managers.Count + 1);

		entity.AddComponent<TestComponent, ITestComponent>(component, index);

		Assert.That(component.Managers[index] == entity);
	}

	[Test]
	public void When_AddComponent_ToOtherEntity_And_NotIsShareable_Then_ComponentAdded()
	{
		var entity1 = new AtlasEntity();
		var entity2 = new AtlasEntity();
		var component = new TestComponent();

		entity1.AddComponent(component);
		entity2.AddComponent(component);

		Assert.That(entity1.GetComponent<TestComponent>() == null);
		Assert.That(entity2.GetComponent<TestComponent>() == component);
		Assert.That(component.TestDispose == false);
		Assert.That(component.AutoDispose == true);
	}
	#endregion

	#region Remove
	[Test]
	public void When_RemoveComponent_As_Component_Then_ComponentRemoved()
	{
		var entity = new AtlasEntity();
		var component = new TestComponent();
		var type = component.GetType();

		entity.AddComponent(component);
		entity.RemoveComponent<TestComponent>();

		Assert.That(entity.GetComponent(type) == null);
	}

	[TestCase(typeof(TestComponent))]
	[TestCase(typeof(ITestComponent))]
	public void When_RemoveComponent_As_Type_Then_ComponentRemoved(Type type)
	{
		var entity = new AtlasEntity();
		var component = new TestComponent();

		entity.AddComponent(component, type);
		entity.RemoveComponent(type);

		Assert.That(entity.GetComponent(type) == null);
	}

	[Test]
	public void When_RemoveComponent_Then_NoComponentRemoved()
	{
		var entity = new AtlasEntity();

		Assert.That(entity.RemoveComponent<TestComponent>() == null);
	}

	[TestCase<ITestComponent, TestComponent>]
	[TestCase<TestComponent, TestComponent>]
	public void When_RemoveComponent_As_GenericType_Then_ComponentRemoved<TType, TComponent>()
		where TType : class, IComponent
		where TComponent : class, TType, new()
	{
		var entity = new AtlasEntity();
		var component = new TComponent();

		entity.AddComponent<TType>(component);
		entity.RemoveComponent<TType>();

		Assert.That(entity.GetComponent<TType>() == null);
	}

	[TestCase<ITestComponent, TestComponent>]
	[TestCase<TestComponent, TestComponent>]
	public void When_RemoveComponent_As_Type_Then_ComponentRemoved<TType, TComponent>()
		where TType : class, IComponent
		where TComponent : class, TType, new()
	{
		var entity = new AtlasEntity();
		var component = new TComponent();

		entity.AddComponent(component, typeof(TType));
		entity.RemoveComponent<TComponent>(typeof(TType));

		Assert.That(entity.GetComponent<TComponent>(typeof(TType)) == null);
	}

	[TestCase<ITestComponent, TestComponent>]
	[TestCase<TestComponent, TestComponent>]
	public void When_RemoveComponent_AsGenericTypeAndComponent_Then_ComponentRemoved<TType, TComponent>()
		where TType : class, IComponent
		where TComponent : class, TType, new()
	{
		var entity = new AtlasEntity();
		var component = new TComponent();

		entity.AddComponent<TComponent, TType>(component);
		entity.RemoveComponent<TComponent, TType>();

		Assert.That(entity.GetComponent<TComponent, TType>() == null);
	}

	[Test]
	public void When_RemoveComponents_Then_ComponentsRemoved()
	{
		var entity = new AtlasEntity();
		var component = new TestComponent();

		entity.AddComponent(component);
		entity.RemoveComponents();

		Assert.That(entity.Components.Count == 0);
	}
	#endregion

	#region Get
	[Test]
	[Repeat(20)]
	public void When_GetDescendantComponents_Then_ComponentsFound([Values(false, true)] bool self)
	{
		var random = new Random();
		var entity = new AtlasEntity();
		var count = 0;

		entity.AddComponent<TestComponent>();
		AddChildren(entity, random, 5, ref count);

		var components = entity.GetDescendantComponents<TestComponent>(-1, self).ToList();
		Assert.That(components.Count == count);
	}

	[Test]
	[Repeat(20)]
	public void When_GetAncestorComponent_Then_ComponentFound([Values(false, true)] bool self, [Values(false, true)] bool add)
	{
		var random = new Random();
		var entity = new AtlasEntity();
		var count = 0;

		entity.AddComponent<TestComponent>();
		AddChildren(entity, random, 5, ref count);

		IEntity descendant = entity;
		while(descendant.Children.Count > 0)
			descendant = descendant.Children[random.Next(descendant.Children.Count)];

		var component = descendant.GetAncestorComponent<TestComponent>(-1, self);
		Assert.That(component != null);
	}

	[Test]
	[Repeat(20)]
	public void When_GetAncestorComponent_Then_ComponentNotFound()
	{
		var parent = new AtlasEntity();
		var child = new AtlasEntity();

		parent.AddChild(child);

		var component = child.GetAncestorComponent<TestComponent>();

		Assert.That(component == null);
	}
	#endregion

	#region Engine
	[TestCase(true)]
	[TestCase(false)]
	public void When_AddComponentEngine_With_IsRoot_Then_ThrowsExpected(bool isRoot)
	{
		var entity = new AtlasEntity(isRoot);
		var engine = new AtlasEngine();

		Assert.That(() => entity.AddComponent<IEngine>(engine), isRoot ? Throws.Nothing : Throws.Exception);
	}
	#endregion

	#region Dispose
	[TestCase(true)]
	[TestCase(false)]
	public void When_Dispose_With_IsAutoDisposable_Then_ComponentDisposed(bool isAutoDisposable)
	{
		var entity = new AtlasEntity();
		var component = new TestComponent();

		component.AutoDispose = isAutoDisposable;
		entity.AddComponent(component);
		entity.RemoveComponent(component);

		Assert.That(component.AutoDispose == isAutoDisposable);
		Assert.That(component.TestDispose == isAutoDisposable);
	}
	#endregion

	private void AddChildren(IEntity entity, Random random, int depth, ref int count)
	{
		if(depth <= 0)
		{
			++count;
			entity.AddComponent<TestComponent>();
			return;
		}

		for(int i = random.Next(1, 6); i > 0; --i)
			AddChildren(entity.AddChild(new AtlasEntity()), random, --depth, ref count);
	}
}