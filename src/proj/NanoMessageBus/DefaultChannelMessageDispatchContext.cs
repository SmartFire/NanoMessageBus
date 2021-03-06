﻿namespace NanoMessageBus
{
	using System;
	using System.Collections.Generic;
	using Logging;

	public class DefaultChannelMessageDispatchContext : IDispatchContext
	{
		public virtual int MessageCount
		{
			get { return this.dispatched ? 0 : 1; }
		}
		public virtual int HeaderCount
		{
			get { return 0; }
		}
		public virtual IDispatchContext WithMessage(object message)
		{
			throw new NotSupportedException("The message collection cannot be modified.");
		}
		public virtual IDispatchContext WithMessages(params object[] messages)
		{
			throw new NotSupportedException("The message collection cannot be modified.");
		}
		public virtual IDispatchContext WithCorrelationId(Guid correlationId)
		{
			throw new NotSupportedException("A correlation identifier is already set.");
		}
		public virtual IDispatchContext WithHeader(string key, string value = null)
		{
			throw new NotSupportedException("The headers cannot be modified.");
		}
		public virtual IDispatchContext WithHeaders(IDictionary<string, string> headers)
		{
			throw new NotSupportedException("The headers cannot be modified.");
		}
		public virtual IDispatchContext WithRecipient(Uri recipient)
		{
			if (recipient == null)
				throw new ArgumentNullException("recipient");

			this.recipients.Add(recipient);
			return this;
		}
		public virtual IDispatchContext WithState(object state)
		{
			throw new NotSupportedException("Envelope state cannot be specified.");
		}

		public virtual IChannelTransaction Send(params object[] messages)
		{
			this.ThrowWhenDispatched();
			this.dispatched = true;

			this.channel.Send(new ChannelEnvelope(this.channelMessage, this.recipients, this.channelMessage));
			return this.channel.CurrentTransaction;
		}
		public virtual IChannelTransaction Publish(params object[] messages)
		{
			throw new NotSupportedException("Only send can be invoked.");
		}
		public virtual IChannelTransaction Reply(params object[] messages)
		{
			throw new NotSupportedException("Only send can be invoked.");
		}

		protected virtual void ThrowWhenDispatched()
		{
			if (!this.dispatched)
				return;

			Log.Warn("The set of messages has already been dispatched.");
			throw new InvalidOperationException("The set of messages has already been dispatched.");
		}

		public DefaultChannelMessageDispatchContext(IMessagingChannel channel, ChannelMessage channelMessage)
		{
			if (channel == null)
				throw new ArgumentNullException("channel");

			if (channelMessage == null)
				throw new ArgumentNullException("channelMessage");

			this.channel = channel;
			this.channelMessage = channelMessage;
		}

		private static readonly ILog Log = LogFactory.Build(typeof(DefaultChannelMessageDispatchContext));
		private readonly ICollection<Uri> recipients = new LinkedList<Uri>();
		private readonly IMessagingChannel channel;
		private readonly ChannelMessage channelMessage;
		private bool dispatched;
	}
}