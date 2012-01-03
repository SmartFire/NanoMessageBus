﻿namespace NanoMessageBus
{
	using System;

	public class DefaultChannelGroup : IChannelGroup
	{
		public virtual bool DispatchOnly
		{
			get { return this.configuration.DispatchOnly; }
		}

		public virtual void Initialize()
		{
			lock (this.locker)
			{
				if (this.initialized)
					return;

				this.TryInitialize();
				this.initialized = true;
			}
		}
		protected virtual void TryInitialize()
		{
			try
			{
				using (this.connector.Connect(this.configuration.GroupName))
				{
					// we have established a connection and are able to perform work				
				}
			}
			catch (ChannelConnectionException)
			{
				this.ReestablishConnection();
			}
		}
		protected virtual void ReestablishConnection()
		{
			try
			{
				using (this.connector.Connect(this.configuration.GroupName))
				{
					// we have established a connection and are able to perform work				
				}
			}
			catch (ChannelConnectionException)
			{
			}
		}

		public virtual void BeginDispatch(ChannelEnvelope envelope, Action<IChannelTransaction> completed)
		{
			if (envelope == null)
				throw new ArgumentNullException("envelope");

			if (completed == null)
				throw new ArgumentNullException("completed");

			this.ThrowWhenDisposed();
			this.ThrowWhenUninitialized();
			this.ThrowWhenFullDuplex();
			
			using (var channel = this.connector.Connect(this.configuration.GroupName))
				channel.Send(envelope); // TODO: add threading
		}
		public virtual void BeginReceive(Action<IDeliveryContext> callback)
		{
			if (callback == null)
				throw new ArgumentNullException("callback");

			this.ThrowWhenDisposed();
			this.ThrowWhenUninitialized();
			this.ThrowWhenAlreadyReceiving();
			this.ThrowWhenDispatchOnly();

			lock (this.locker)
				this.receiving = this.TryReceive(callback);
		}
		protected virtual bool TryReceive(Action<IDeliveryContext> callback)
		{
			try
			{
				for (var i = 0; i < this.configuration.MinWorkers; i++)
				{
					using (var channel = this.connector.Connect(this.configuration.GroupName))
					{
						channel.Receive(callback);
					}
				}

				return true;
			}
			catch (ChannelConnectionException)
			{
				return false;
			}
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
		protected virtual void ThrowWhenDispatchOnly()
		{
			if (this.configuration.DispatchOnly)
				throw new InvalidOperationException("Dispatch-only channel groups cannot receive messages.");
		}
		protected virtual void ThrowWhenAlreadyReceiving()
		{
			if (this.receiving)
				throw new InvalidOperationException("A callback has already been provided.");
		}

		public DefaultChannelGroup(
			IChannelConnector connector, IChannelGroupConfiguration configuration, IWorkerGroup workers)
		{
			this.connector = connector;
			this.configuration = configuration;
			this.workers = workers;
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

		private static readonly TimeSpan[] Timeouts =
		{
			TimeSpan.FromSeconds(0),
			TimeSpan.FromSeconds(1),
			TimeSpan.FromSeconds(2),
			TimeSpan.FromSeconds(4),
			TimeSpan.FromSeconds(8),
			TimeSpan.FromSeconds(16),
			TimeSpan.FromSeconds(32)
		};
		private readonly object locker = new object();
		private readonly IChannelConnector connector;
		private readonly IChannelGroupConfiguration configuration;
		private readonly IWorkerGroup workers;
		private bool receiving;
		private bool initialized;
		private bool disposed;
	}
}