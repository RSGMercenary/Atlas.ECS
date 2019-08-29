﻿namespace Atlas.Core.Messages
{
	class ChildRemoveMessage<T> : KeyValueMessage<T, int, T>, IChildRemoveMessage<T>
				where T : IMessenger, IHierarchy<T>

	{
		public ChildRemoveMessage(int key, T value) : base(key, value)
		{
		}
	}
}