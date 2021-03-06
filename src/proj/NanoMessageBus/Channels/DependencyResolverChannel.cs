﻿namespace NanoMessageBus.Channels
{
	using System;
	using Logging;

	public class DependencyResolverChannel : IMessagingChannel
	{
		public virtual bool Active
		{
			get { return this.CurrentContext.Active; }
		}
		public virtual ChannelMessage CurrentMessage
		{
			get { return this.CurrentContext.CurrentMessage; }
		}
		public virtual IChannelTransaction CurrentTransaction
		{
			get { return this.CurrentContext.CurrentTransaction; }
		}
		public virtual IChannelGroupConfiguration CurrentConfiguration
		{
			get { return this.CurrentContext.CurrentConfiguration; }
		}
		public virtual IDependencyResolver CurrentResolver
		{
			get { return this.currentResolver ?? this.resolver; }
		}
		protected virtual IDeliveryContext CurrentContext
		{
			get { return this.currentContext ?? this.channel; }
		}

		public virtual IDispatchContext PrepareDispatch(object message = null, IMessagingChannel actual = null)
		{
			Log.Debug("Preparing a dispatch");
			return this.CurrentContext.PrepareDispatch(message, actual ?? this);
		}
		public virtual void Send(ChannelEnvelope envelope)
		{
			Log.Verbose("Sending envelope '{0}' through the underlying channel.", envelope.MessageId());
			this.channel.Send(envelope);
		}

		public virtual void BeginShutdown()
		{
			this.channel.BeginShutdown();
		}
		public virtual void Receive(Action<IDeliveryContext> callback)
		{
			this.channel.Receive(context => this.Receive(context, callback));
		}
		protected virtual void Receive(IDeliveryContext context, Action<IDeliveryContext> callback)
		{
			try
			{
				Log.Verbose("Delivery received, attempting to create nested resolver.");
				this.currentContext = context;
				this.currentResolver = this.resolver.CreateNestedResolver();
				callback(this);
			}
			finally
			{
				Log.Verbose("Delivery completed, disposing nested resolver.");
				this.currentResolver.TryDispose();
				this.currentResolver = null;
				this.currentContext = null;
			}
		}

		public DependencyResolverChannel(IMessagingChannel channel, IDependencyResolver resolver)
		{
			if (channel == null)
				throw new ArgumentNullException("channel");

			if (resolver == null)
				throw new ArgumentNullException("resolver");

			this.channel = channel;
			this.resolver = resolver;
		}
		~DependencyResolverChannel()
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

			Log.Verbose("Disposing the underlying channel and resolver.");
			this.channel.TryDispose();
			this.resolver.TryDispose();
		}

		private static readonly ILog Log = LogFactory.Build(typeof(DependencyResolverChannel));
		private readonly IMessagingChannel channel;
		private readonly IDependencyResolver resolver;
		private IDependencyResolver currentResolver;
		private IDeliveryContext currentContext;
	}
}