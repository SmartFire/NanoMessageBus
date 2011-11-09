namespace NanoMessageBus
{
	using System;
	using System.Collections.Generic;
	using System.Runtime.Serialization;

	/// <summary>
	/// The primary message envelope used to hold the metadata and payload necessary to route the message to all intended recipients.
	/// </summary>
	[DataContract]
	[Serializable]
	public class EnvelopeMessage
	{
		[DataMember(Order = 1, EmitDefaultValue = false, IsRequired = false)]
		private readonly Guid messageId;

		[DataMember(Order = 2, EmitDefaultValue = false, IsRequired = false)]
		private readonly Guid correlationId;

		[DataMember(Order = 3, EmitDefaultValue = false, IsRequired = false)]
		private readonly Uri returnAddress;

		[IgnoreDataMember]
		private readonly TimeSpan timeToLive;

		[IgnoreDataMember]
		private readonly bool persistent;

		[DataMember(Order = 4, EmitDefaultValue = false, IsRequired = false)]
		private readonly IDictionary<string, string> headers;

		[DataMember(Order = 5, EmitDefaultValue = false, IsRequired = false)]
		private readonly ICollection<object> logicalMessages;

		/// <summary>
		/// Initializes a new instance of the EnvelopeMessage class.
		/// </summary>
		protected EnvelopeMessage()
		{
		}

		/// <summary>
		/// Initializes a new instance of the EnvelopeMessage class.
		/// </summary>
		/// <param name="messageId">The value which uniquely identifies the envelope message.</param>
		/// <param name="correlationId">The value which associates the message with a larger conversation.</param>
		/// <param name="returnAddress">The address to which all replies should be directed.</param>
		/// <param name="timeToLive">The maximum amount of time the message will live prior to successful receipt.</param>
		/// <param name="persistent">A value indicating whether the message is durably stored.</param>
		/// <param name="headers">The message headers which contain additional metadata about the logical messages.</param>
		/// <param name="logicalMessages">The collection of dispatched logical messages.</param>
		public EnvelopeMessage(
			Guid messageId,
			Guid correlationId,
			Uri returnAddress,
			TimeSpan timeToLive,
			bool persistent,
			IDictionary<string, string> headers,
			ICollection<object> logicalMessages)
		{
			if (timeToLive.TotalMilliseconds <= 0)
				throw new ArgumentException("The message TTL must be positive.");

			this.messageId = messageId;
			this.correlationId = correlationId;
			this.returnAddress = returnAddress;
			this.timeToLive = timeToLive;
			this.persistent = persistent;
			this.headers = headers ?? new Dictionary<string, string>();
			this.logicalMessages = logicalMessages;
		}

		/// <summary>
		/// Gets the value which uniquely identifies the envelope message.
		/// </summary>
		public Guid MessageId
		{
			get { return this.messageId; }
		}

		/// <summary>
		/// Gets the value which associates the message with a larger conversation.
		/// </summary>
		public Guid CorrelationId
		{
			get { return this.correlationId; }
		}

		/// <summary>
		/// Gets the address to which all replies should be directed.
		/// </summary>
		public Uri ReturnAddress
		{
			get { return this.returnAddress; }
		}

		/// <summary>
		/// Gets the maximum amount of time the message will live prior to successful receipt.
		/// </summary>
		public TimeSpan TimeToLive
		{
			get { return this.timeToLive; }
		}

		/// <summary>
		/// Gets a value indicating whether the message is durably stored.
		/// </summary>
		public bool Persistent
		{
			get { return this.persistent; }
		}

		/// <summary>
		/// Gets the message headers which contain additional metadata about the logical messages.
		/// </summary>
		public IDictionary<string, string> Headers
		{
			get { return this.headers; }
		}

		/// <summary>
		/// Gets the collection of dispatched logical messages.
		/// </summary>
		public ICollection<object> LogicalMessages
		{
			get { return this.logicalMessages; }
		}
	}
}