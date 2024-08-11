﻿using Atlas.ECS.Entities;
using Newtonsoft.Json;

namespace Atlas.ECS.Serialization;

public static class AtlasSerializer
{
	//DO NOT MAKE THIS A METHOD!!
	//Newtonsoft does some weird caching when it's a method.
	//Breakpoints then won't be hit when debugging.
	private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
	{
		ContractResolver = new AtlasContractResolver(),
		NullValueHandling = NullValueHandling.Ignore,
		ObjectCreationHandling = ObjectCreationHandling.Replace,
		ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
		PreserveReferencesHandling = PreserveReferencesHandling.Objects,
		TypeNameHandling = TypeNameHandling.Objects,
	};

	public static string Serialize(this IEntity entity, Formatting formatting = Formatting.None, int maxDepth = -1)
	{
		(Settings.ContractResolver as AtlasContractResolver).MaxDepth = maxDepth;
		return Serialize(entity as ISerialize, formatting);
	}

	public static string Serialize(this ISerialize value, Formatting formatting = Formatting.None)
	{
		(Settings.ContractResolver as AtlasContractResolver).Instance = value;
		return JsonConvert.SerializeObject(value, formatting, Settings);
	}

	public static T Deserialize<T>(string json) where T : ISerialize
	{
		return JsonConvert.DeserializeObject<T>(json, Settings);
	}
}