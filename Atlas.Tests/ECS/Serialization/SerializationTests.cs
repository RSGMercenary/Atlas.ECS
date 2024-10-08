﻿using Atlas.Core.Objects.Update;
using Atlas.ECS.Components.Component;
using Atlas.ECS.Components.Engine;
using Atlas.ECS.Entities;
using Atlas.ECS.Families;
using Atlas.ECS.Serialization;
using Atlas.ECS.Systems;
using Atlas.Tests.Testers.Components;
using Atlas.Tests.Testers.Families;
using Atlas.Tests.Testers.Systems;
using Atlas.Tests.Testers.Utilities;
using Newtonsoft.Json;
using NUnit.Framework;
using System;

namespace Atlas.Tests.ECS.Serialization;

[TestFixture]
class SerializationTests
{
	public Random Random;

	[SetUp]
	public void SetUp()
	{
		Random = new Random();
	}

	[Test]
	[Repeat(20)]
	public void When_SerializeEntity_Then_DeserializeEntity_IsEqual()
	{
		GetRootAndClone(out var root, out var clone);

		var json1 = root.Serialize(Formatting.Indented);
		var json2 = clone.Serialize(Formatting.Indented);

		var json = root.Serialize(Formatting.Indented, -1, "GlobalName");

		var d = AtlasSerializer.Deserialize<IEntity>(json1);

		Assert.That(json1 == json2);
	}

	[TestCase]
	[Repeat(20)]
	public void When_SerializeEntity_WithDepth_Then_DeserializeEntity_IsEqual()
	{
		var root = new AtlasEntity(true);

		var depth = Random.Next(6);
		var maxDepth = Random.Next(-1, depth + 1);

		AddChildren(root, Random, depth);

		var json = AtlasSerializer.Serialize(root);
		var clone = AtlasSerializer.Deserialize<IEntity>(json);

		Assert.That(root.Serialize(Formatting.None, maxDepth) == clone.Serialize(Formatting.None, maxDepth));
	}

	[Test]
	[Repeat(20)]
	public void When_SerializeComponent_Then_DeserializeComponent_IsEqual()
	{
		var component = new TestComponent(Random.NextBool());

		component.AutoDispose = Random.NextBool();

		if(component.IsShareable)
		{
			for(int i = Random.Next(2, 6); i > 0; --i)
				component.AddManager(new AtlasEntity());
		}
		else if(Random.NextBool())
			component.AddManager(new AtlasEntity());

		var json = AtlasSerializer.Serialize(component);
		var clone = AtlasSerializer.Deserialize<IComponent>(json);

		Assert.That(component.Serialize() == clone.Serialize());
	}

	[Test]
	public void When_SerializeEngine_Then_JsonIsntNull()
	{
		var root = new AtlasEntity(true);
		var child = new AtlasEntity("Child", "Child");
		var engine = new AtlasEngine();

		root.AddComponent(engine);
		root.AddChild(child);

		child.AddComponent<TestComponent>();

		engine.Systems.Add<TestFixedFamilySystem>();
		engine.Systems.Add<TestVariableFamilySystem>();

		var json = engine.Serialize(Formatting.Indented);

		Assert.That(json != null);
	}

	[Test]
	[Repeat(20)]
	public void When_SerializeFamily_Then_DeserializeFamily_IsEqual()
	{
		var family = new AtlasFamily<TestFamilyMember>();

		for(int i = Random.Next(11); i > 0; --i)
		{
			var entity = new AtlasEntity();
			if(Random.NextBool())
				entity.AddComponent<TestComponent>();

			family.AddEntity(entity);
		}

		var json = AtlasSerializer.Serialize(family);
		var clone = AtlasSerializer.Deserialize<IFamily>(json);

		Assert.That(family.Serialize() == clone.Serialize());
	}

	[Test]
	[Repeat(20)]
	public void When_SerializeSystem_Then_DeserializeSystem_IsEqual()
	{
		var root = new AtlasEntity(true);
		var engine = new AtlasEngine();
		var timeSteps = new[] { TimeStep.None, TimeStep.Variable, TimeStep.Fixed };

		root.AddComponent(engine);

		for(int i = Random.Next(6); i > 0; --i)
		{
			var entity = new AtlasEntity();
			var component = new TestComponent();

			entity.AddComponent(component);

			root.AddChild(entity);
		}

		var system = engine.Systems.Add<TestVariableFamilySystem>();

		system.Priority = Random.Next(int.MinValue, int.MaxValue);
		system.Sleep(Random.NextBool());
		system.IgnoreSleep = Random.NextBool();
		system.IsUpdating = Random.NextBool();
		system.TimeStep = timeSteps[Random.Next(timeSteps.Length)];

		var json = AtlasSerializer.Serialize(system);
		var clone = AtlasSerializer.Deserialize<ISystem>(json);

		Assert.That(system.Serialize() == clone.Serialize());
	}

	#region Helpers
	private void GetRootAndClone(out IEntity root, out IEntity clone)
	{
		root = new AtlasEntity(true);
		root.AddComponent<AtlasEngine>();

		AddChildren(root, Random, Random.Next(6));

		var json = AtlasSerializer.Serialize(root);
		clone = AtlasSerializer.Deserialize<IEntity>(json);
	}

	private void AddChildren(IEntity entity, Random random, int depth)
	{
		entity.AutoDispose = Random.NextBool();
		entity.Sleep(Random.NextBool());
		entity.SelfSleep(Random.NextBool());

		if(random.NextBool())
			entity.AddComponent<TestComponent>();

		if(depth <= 0)
			return;

		for(int i = random.Next(6); i > 0; --i)
			AddChildren(entity.AddChild(new AtlasEntity()), random, --depth);
	}
	#endregion
}