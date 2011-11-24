﻿namespace NanoMessageBus.RabbitChannel
{
	using System;
	using RabbitMQ.Client;
	using RabbitMQ.Client.Events;

	public class RabbitChannel : IMessagingChannel
	{
		public virtual ChannelMessage CurrentMessage { get; private set; }
		public virtual IChannelTransaction CurrentTransaction { get; private set; }

		public virtual void BeginReceive(Action<IDeliveryContext> callback)
		{
			if (callback == null)
				throw new ArgumentNullException("callback");

			this.ThrowWhenDisposed();
			this.ThrowWhenSubscriptionExists();

			// TODO: wrap up the exceptions on the following calls if the channel is unavailable
			this.subscription = this.subscriptionFactory();
			this.subscription.BeginReceive<BasicDeliverEventArgs>(this.configuration.ReceiveTimeout, msg =>
				this.BeginReceive(msg, callback));
		}
		protected virtual void BeginReceive<T>(T message, Action<IDeliveryContext> callback) where T : class
		{
			this.CurrentMessage = null; // TODO: convert from BasicDeliverEventArgs

			using (this.CurrentTransaction = new RabbitTransaction(this, this.configuration.TransactionType))
				this.TryReceive(message, callback);
		}
		protected virtual void TryReceive<T>(T message, Action<IDeliveryContext> callback) where T : class
		{
			// TODO: on serialization failure, immediately forward to poison message exchange and ack/commit
			try
			{
				// TODO: *after* callback:
				// 1. clear failure count for message (global per app or at least shared per channel group)
				// 2. clear serialization cache for message (global per app or at least shared per channel group)
				callback(this);
			}
			catch (ChannelConnectionException)
			{
				throw;
			}
			catch
			{
				// TODO: increment failure count; if it exceeds configured amount, forward to poison message exchange (along with serialization info)
				// adding message back to in-memory queue means another channel (within the same channel group)
				// can pick it up for processing, therefore failure/serialization caches must be shared
				this.subscription.RetryMessage(message); // TODO: if channel is unavailable
			}
		}

		public virtual void Send(ChannelEnvelope envelope)
		{
			// TODO: convert then channel.BasicPublish() to each destination

			this.ThrowWhenDisposed();
		}

		public virtual void AcknowledgeMessage()
		{
			this.ThrowWhenDisposed();
			this.ThrowWhenSubscriptionMissing();
			if (this.transactionType != RabbitTransactionType.None)
				this.subscription.AcknowledgeMessage(); // TODO: wrap exception if channel unavailable
		}
		public virtual void CommitTransaction()
		{
			this.ThrowWhenDisposed();
			if (this.transactionType == RabbitTransactionType.Full)
				this.channel.TxCommit(); // TODO: wrap exception if channel unavailable
		}
		public virtual void RollbackTransaction()
		{
			this.ThrowWhenDisposed();
			if (this.transactionType == RabbitTransactionType.Full)
			    this.channel.TxRollback(); // TODO: wrap exception if channel unavailable
		}

		protected virtual void ThrowWhenDisposed()
		{
			if (this.disposed)
				throw new ObjectDisposedException("RabbitChannel");
		}
		protected virtual void ThrowWhenSubscriptionExists()
		{
			if (this.subscription != null)
				throw new InvalidOperationException("The channel already has a receive callback.");
		}
		protected virtual void ThrowWhenSubscriptionMissing()
		{
			if (this.subscription == null)
				throw new InvalidOperationException("The channel must first be opened for receive.");
		}

		public RabbitChannel(
			IModel channel,
			RabbitChannelGroupConfiguration configuration,
			Func<RabbitSubscription> subscriptionFactory) : this()
		{
			this.channel = channel;
			this.configuration = configuration;
			this.transactionType = configuration.TransactionType;
			this.subscriptionFactory = subscriptionFactory;

			if (this.transactionType == RabbitTransactionType.Full)
				this.channel.TxSelect();

			if (this.configuration.ChannelBuffer > 0)
				this.channel.BasicQos(0, (ushort)this.configuration.ChannelBuffer, false);
		}
		protected RabbitChannel() { }
		~RabbitChannel()
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

			lock (this.locker)
			{
				if (this.disposed)
					return;

				this.disposed = true;

				if (this.subscription != null)
					this.subscription.Dispose();

				this.channel.Dispose();
			}
		}

		private readonly object locker = new object();
		private readonly IModel channel;
		private readonly RabbitChannelGroupConfiguration configuration;
		private readonly RabbitTransactionType transactionType;
		private readonly Func<RabbitSubscription> subscriptionFactory;
		private RabbitSubscription subscription;
		private bool disposed;
	}
}