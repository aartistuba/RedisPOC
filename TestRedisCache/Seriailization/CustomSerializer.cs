using System;
using System.IO;

namespace TestRedisCache.Serialization
{
  internal class CustomSerializer<T> : ISerializer<T>
  {
    private readonly Stream mStream;

    public CustomSerializer(Stream xiStream)
    {
      mStream = xiStream;
    }

    public void Serialize(T xiObjectToSerialize)
    {
      if (xiObjectToSerialize == null)
      {
        SerializeNull();
        return;
      }

      if (!(xiObjectToSerialize is ISerializableToStream))
      {
        throw new InvalidOperationException(
          "Cannot serialize an object of type {0} using CustomSerializer",
          xiObjectToSerialize.GetType());
      }

      Serialize((ISerializableToStream)xiObjectToSerialize);
    }

    private void SerializeNull()
    {
      byte[] lNullGuid = Guid.Empty.ToByteArray();
      mStream.Write(lNullGuid, 0, lNullGuid.Length);
    }

    private void Serialize(ISerializableToStream xiObjectToSerialize)
    {
      WriteGuidToStream(xiObjectToSerialize);
      xiObjectToSerialize.Serialize(mStream);
      mStream.Flush();
    }

    private void WriteGuidToStream(ISerializableToStream xiObjectToSerialize)
    {
      byte[] lGuidBytes = xiObjectToSerialize.GetType().GetSerializationGuid().ToByteArray();
      mStream.Write(lGuidBytes, 0, lGuidBytes.Length);
    }

    public T Deserialize()
    {
      Guid lTypeMarker = ReadGuidFromStream();

      if (lTypeMarker == Guid.Empty)
      {
        return default(T);
      }

      var lType = GetTypeToDeserialize(lTypeMarker);

      return (T)Activator.CreateInstance(lType, mStream);
    }

    private Guid ReadGuidFromStream()
    {
      var lGuidBytes = new byte[16];
      mStream.Read(lGuidBytes, 0, lGuidBytes.Length);

      var lTypeMarker = new Guid(lGuidBytes);
      return lTypeMarker;
    }

    private static Type GetTypeToDeserialize(Guid xiTypeMarker)
    {
      Type lType = SerializationGuidCache.GetType(xiTypeMarker);

      if (!typeof(T).IsAssignableFrom(lType))
      {
        throw new InvalidOperationException(
          "Serialised object was of type {0}, not expected type {1}",
          lType.AssemblyQualifiedName,
          typeof(T).AssemblyQualifiedName);
      }
      return lType;
    }
  }
}