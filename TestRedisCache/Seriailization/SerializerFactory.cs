using System;
using System.IO;

namespace TestRedisCache.Serialization
{
  internal class SerializerFactory
  {
    public static ISerializer<T> GetSerializer<T>(Stream xiStream)
    {
      if (typeof(ISerializableToStream).IsAssignableFrom(typeof(T)))
      {
        return new CustomSerializer<T>(xiStream);
      }
      
      return new BinaryFormatterSerializer<T>(xiStream);
    }
  }
}