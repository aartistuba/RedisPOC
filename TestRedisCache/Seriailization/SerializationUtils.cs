using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace TestRedisCache.Serialization
{
  public static class SerializationUtils
  {
    public static string SerializeToString<T>(T xiObject)
    {
      return Convert.ToBase64String(SerializeToByteArray(xiObject));
    }

    public static T DeserializeFromString<T>(string xiSerialized)
    {
      return DeserializeFromByteArray<T>(Convert.FromBase64String(xiSerialized));
    }

    public static byte[] SerializeToByteArray<T>(T xiObject)
    {
      using (var lMemoryStream = new MemoryStream())
      {
        SerializeToStream(lMemoryStream, xiObject);
        return lMemoryStream.ToArray();
      }
    }

    public static void SerializeToStream<T>(Stream xiStream, T xiObject)
    {
      SerializerFactory.GetSerializer<T>(xiStream).Serialize(xiObject);
    }

    public static T DeserializeFromByteArray<T>(byte[] xiBytes)
    {
      using (var lMemoryStream = new MemoryStream(xiBytes))
      {
        return DeserializeFromStream<T>(lMemoryStream);
      }
    }

    public static T DeserializeFromStream<T>(Stream xiStream)
    {
      return SerializerFactory.GetSerializer<T>(xiStream).Deserialize();
    }

    public static T CloneViaSerialization<T>(T xiObj)
    {
      var lSerializer = new BinaryFormatter();

      using (var lBuff = new MemoryStream())
      {
        lSerializer.Serialize(lBuff, xiObj);
        lBuff.Seek(0, SeekOrigin.Begin);
        var lRet = lSerializer.Deserialize(lBuff);
        return (T)lRet;
      }
    }
  }
}