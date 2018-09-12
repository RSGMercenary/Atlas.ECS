﻿namespace Atlas.Framework.Messages
{
	public class Message
	{
		private IMessageDispatcher messenger;

		public IMessageDispatcher Messenger
		{
			get { return messenger; }
			set { messenger = messenger ?? value; }
		}

		public IMessageDispatcher CurrentMessenger { get; set; }

		public bool AtMessenger
		{
			get { return (bool)messenger?.Equals(CurrentMessenger); }
		}
	}

	public class Message<TMessenger> : Message, IMessage<TMessenger>
		where TMessenger : IMessageDispatcher
	{
		public Message()
		{

		}

		public new TMessenger Messenger
		{
			get { return (TMessenger)base.Messenger; }
			set { base.Messenger = value; }
		}

		public new TMessenger CurrentMessenger
		{
			get { return (TMessenger)base.CurrentMessenger; }
			set { base.CurrentMessenger = value; }
		}
	}
}