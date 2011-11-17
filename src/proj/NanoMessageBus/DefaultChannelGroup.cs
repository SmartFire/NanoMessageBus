﻿namespace NanoMessageBus
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	public class DefaultChannelGroup : IChannelGroup
	{
		public virtual void Initialize()
		{
			this.initialized = true;
		}

		public virtual void BeginDispatch(ChannelMessage message, IEnumerable<Uri> recipients)
		{
			this.Dispatch(message, recipients, false);
		}
		public virtual void Dispatch(ChannelMessage message, IEnumerable<Uri> recipients)
		{
			this.Dispatch(message, recipients, true);
		}
		protected virtual void Dispatch(ChannelMessage message, IEnumerable<Uri> recipients, bool sync)
		{
			if (message == null)
				throw new ArgumentNullException("message");
			if (recipients == null)
				throw new ArgumentNullException("recipients");

			var list = recipients.Where(x => x != null).ToArray();
			if (list.Length == 0)
				throw new ArgumentException("No recipients specified.", "recipients");

			this.ThrowWhenDisposed();
			this.ThrowWhenUninitialized();
			this.ThrowWhenFullDuplex();
		}

		public virtual void BeginReceive(Action<IMessagingChannel> callback)
		{
			if (callback == null)
				throw new ArgumentNullException("callback");

			this.ThrowWhenDisposed();
			this.ThrowWhenUninitialized();
			this.ThrowWhenReceiving();

			if (this.configuration.DispatchOnly)
				return; // no-op

			this.receiving = true; // TODO: hand the callback to the channels
		}

		protected virtual void ThrowWhenDisposed()
		{
			if (this.disposed)
				throw new ObjectDisposedException(typeof(DefaultMessagingHost).Name);
		}
		protected virtual void ThrowWhenUninitialized()
		{
			if (!this.initialized)
				throw new InvalidOperationException("The host has not been initialized.");
		}
		protected virtual void ThrowWhenFullDuplex()
		{
			if (!this.configuration.DispatchOnly)
				throw new InvalidOperationException("Dispatch can only be performed using a dispatch-only channel group.");
		}
		protected virtual void ThrowWhenReceiving()
		{
			if (this.receiving)
				throw new InvalidOperationException("A callback has already been provided.");
		}

		public DefaultChannelGroup(IChannelConnector connector, IChannelConfiguration configuration)
		{
			this.connector = connector;
			this.configuration = configuration;
		}
		~DefaultChannelGroup()
		{
			this.Dispose(false);
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}
		protected virtual void Dispose(bool disposing)
		{
			if (!disposing)
				return;

			this.disposed = true;
		}

		private readonly IChannelConnector connector;
		private readonly IChannelConfiguration configuration;
		private bool receiving;
		private bool initialized;
		private bool disposed;
	}
}