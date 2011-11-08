#pragma warning disable 169
// ReSharper disable InconsistentNaming

namespace NanoMessageBus.Serialization.ProtocolBuffers.UnitTests
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Runtime.Serialization;
	using Machine.Specifications;

	[Subject("ProtocolBufferSerializer")]
	public class when_serializing_an_object
	{
		private const int InputValue = 1234;
		private static readonly Stream OutputStream = new MemoryStream();
		private static readonly ISerializeMessages Serializer = new ProtocolBufferSerializer(InputValue.GetType());

		private Because of = () => Serializer.Serialize(OutputStream, 0);

		private It should_write_the_serialized_bytes_to_the_output_stream_provided = () =>
		                                                                             OutputStream.Length.ShouldBeGreaterThan(0);
	}

	[Subject("ProtocolBufferSerializer")]
	public class when_serializing_an_unregistered_type
	{
		private static readonly ISerializeMessages Serializer = new ProtocolBufferSerializer();
		private static readonly Stream TempStream = new MemoryStream();
		private static Exception exception;

		private Because of = () =>
		                     exception = Catch.Exception(() => Serializer.Serialize(TempStream, new SimpleMessage()));

		private It should_throw_a_SerializationException = () =>
		                                                   exception.ShouldBeOfType(typeof(SerializationException));
	}

	[Subject("ProtocolBufferSerializer")]
	public class when_serializing_and_then_deserializing_a_primitive_type
	{
		private const int InputValue = 12345;
		private static readonly Stream TempStream = new MemoryStream();
		private static readonly ISerializeMessages Serializer = new ProtocolBufferSerializer(InputValue.GetType());
		private static int outputValue;

		private Establish context = () =>
		{
			Serializer.Serialize(TempStream, InputValue);
			TempStream.Position = 0;
		};

		private Because of = () =>
		                     outputValue = (int)Serializer.Deserialize(TempStream);

		private It should_deserialize_the_primitive_type_correctly = () =>
		                                                             outputValue.ShouldEqual(InputValue);
	}

	[Subject("ProtocolBufferSerializer")]
	public class when_serializing_and_then_deserializing_a_complex_type
	{
		private static readonly Dictionary<string, string> InputValue = new Dictionary<string, string>();
		private static readonly Stream TempStream = new MemoryStream();
		private static readonly ISerializeMessages Serializer = new ProtocolBufferSerializer(InputValue.GetType());
		private static Dictionary<string, string> outputValue;

		private Establish context = () =>
		{
			InputValue["a"] = "1";
			InputValue["b"] = "2";

			Serializer.Serialize(TempStream, InputValue);
			TempStream.Position = 0;
		};

		private Because of = () =>
		                     outputValue = (Dictionary<string, string>)Serializer.Deserialize(TempStream);

		private It should_deserialize_back_to_the_same_type = () =>
		                                                      outputValue.ShouldBeOfType(InputValue.GetType());

		private It should_deserialize_the_contents_of_the_complex_type = () =>
		                                                                 outputValue.Keys.Count.ShouldEqual(
		                                                                 	InputValue.Keys.Count);
	}

	[Subject("ProtocolBufferSerializer")]
	public class when_serializing_and_then_deserializing_a_simple_message
	{
		private static readonly SimpleMessage InputValue = new SimpleMessage
		{
			Value = "Hello, World!"
		};
		private static readonly Stream TempStream = new MemoryStream();
		private static readonly ISerializeMessages Serializer = new ProtocolBufferSerializer(InputValue.GetType());
		private static SimpleMessage outputValue;

		private Establish context = () =>
		{
			Serializer.Serialize(TempStream, InputValue);
			TempStream.Position = 0;
		};

		private Because of = () =>
		                     outputValue = (SimpleMessage)Serializer.Deserialize(TempStream);

		private It should_deserialize_back_to_the_same_type = () =>
		                                                      outputValue.ShouldBeOfType(InputValue.GetType());

		private It should_deserialize_the_contents_of_the_simple_message = () =>
		                                                                   outputValue.Value.ShouldEqual(InputValue.Value);
	}

	[Subject("ProtocolBufferSerializer")]
	public class when_serializing_and_then_deserializing_a_collection_of_simple_messages
	{
		private static readonly IList<SimpleMessage> InputValue =
			new List<SimpleMessage>
			{
				new SimpleMessage
				{
					Value = "Hello, World!"
				}
			};
		private static readonly Stream TempStream = new MemoryStream();
		private static readonly ISerializeMessages Serializer = new ProtocolBufferSerializer(InputValue.GetType());
		private static IList<SimpleMessage> outputValue;

		private Establish context = () =>
		{
			Serializer.Serialize(TempStream, InputValue);
			TempStream.Position = 0;
		};

		private Because of = () =>
		                     outputValue = (IList<SimpleMessage>)Serializer.Deserialize(TempStream);

		private It should_deserialize_back_to_a_collection = () =>
		                                                     outputValue.ShouldNotBeNull();

		private It should_deserialize_the_contents_of_the_simple_message = () =>
		                                                                   outputValue[0].Value.ShouldEqual(
		                                                                   	InputValue[0].Value);
	}

	[Subject("ProtocolBufferSerializer")]
	public class when_serializing_and_then_deserializing_an_EnvelopeMessage
	{
		private static readonly EnvelopeMessage InputValue = new EnvelopeMessage(
			Guid.NewGuid(),
			new Uri("msmq://localhost/MyQueue"),
			TimeSpan.Zero,
			true,
			null,
			new object[] { });
		private static readonly Stream TempStream = new MemoryStream();
		private static readonly ISerializeMessages Serializer = new ProtocolBufferSerializer(InputValue.GetType());
		private static EnvelopeMessage outputValue;

		private Establish context = () =>
		{
			Serializer.Serialize(TempStream, InputValue);
			TempStream.Position = 0;
		};

		private Because of = () =>
		                     outputValue = (EnvelopeMessage)Serializer.Deserialize(TempStream);

		private It should_deserialize_back_to_the_same_type = () =>
		                                                      outputValue.ShouldBeOfType(InputValue.GetType());

		private It should_deserialize_the_contents_of_the_complex_type = () =>
		                                                                 outputValue.MessageId.ShouldEqual(
		                                                                 	InputValue.MessageId);
	}

	[Subject("ProtocolBufferSerializer")]
	public class when_an_EnvelopeMessage_contains_several_different_logical_message_types
	{
		private static readonly EnvelopeMessage InputValue =
			new EnvelopeMessage(
				Guid.NewGuid(),
				new Uri("msmq://localhost/myqueue"),
				TimeSpan.Zero,
				true,
				null,
				new object[] { 12345, Guid.NewGuid(), "whatever" });

		private static readonly Stream TempStream = new MemoryStream();
		private static readonly ISerializeMessages Serializer = new ProtocolBufferSerializer(
			InputValue.GetType(),
			InputValue.LogicalMessages.ToList()[0].GetType(),
			InputValue.LogicalMessages.ToList()[1].GetType(),
			InputValue.LogicalMessages.ToList()[2].GetType());
		private static EnvelopeMessage outputValue;

		private Establish context = () =>
		{
			Serializer.Serialize(TempStream, InputValue);
			TempStream.Position = 0;
		};

		private Because of = () =>
		                     outputValue = (EnvelopeMessage)Serializer.Deserialize(TempStream);

		private It should_deserialize_the_contents_of_each_logical_message_properly = () =>
		{
			var input = InputValue.LogicalMessages.ToList();
			var output = outputValue.LogicalMessages.ToList();
			for (var i = 0; i < input.Count; i++)
				output[i].ShouldEqual(input[i]);
		};
	}
}

// ReSharper restore InconsistentNaming
#pragma warning restore 169