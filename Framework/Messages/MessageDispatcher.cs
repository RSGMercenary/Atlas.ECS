﻿using Atlas.Framework.Signals;
using System;
using System.Collections.Generic;

namespace Atlas.Framework.Messages
{
	public abstract class MessageDispatcher : IMessageDispatcher
	{
		private Dictionary<Type, SignalBase> messages = new Dictionary<Type, SignalBase>();

		public virtual void Message<TMessage>(TMessage message)
			where TMessage : IMessage
		{
			message.CurrentMessenger = this;
			//Pass around message internally...
			Messaging(message);
			//...before dispatching externally.
			var type = typeof(TMessage);
			if(messages.ContainsKey(type))
				((Signal<TMessage>)messages[type]).Dispatch(message);
		}

		protected virtual void Messaging(IMessage message)
		{

		}

		public void AddListener<TMessage>(Action<TMessage> listener)
			where TMessage : IMessage
		{
			AddListenerSlot(listener, 0);
		}

		public void AddListener<TMessage>(Action<TMessage> listener, int priority)
			where TMessage : IMessage
		{
			AddListenerSlot(listener, priority);
		}

		protected ISlotBase AddListenerSlot<TMessage>(Action<TMessage> listener, int priority)
			where TMessage : IMessage
		{
			var type = typeof(TMessage);
			if(!messages.ContainsKey(type))
				messages.Add(type, GetSignal<TMessage>());
			return messages[type].Add(listener, priority);
		}

		protected virtual Signal<TMessage> GetSignal<TMessage>()
			where TMessage : IMessage
		{
			return new Signal<TMessage>();
		}

		public void RemoveListener<TMessage>(Action<TMessage> listener)
			where TMessage : IMessage
		{
			var type = typeof(TMessage);
			if(!messages.ContainsKey(type))
				return;
			var signal = messages[type];
			signal.Remove(listener);
			if(signal.Slots.Count > 0)
				return;
			messages.Remove(type);
			signal.Dispose();
		}
	}
}