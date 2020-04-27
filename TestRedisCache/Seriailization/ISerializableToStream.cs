using System.IO;

namespace TestRedisCache.Serialization
{
  ///==========================================================================
  /// Property : ISerializableToStream
  /// 
  /// <summary>
  ///   Marks a class that can serialize itself to a stream, and deserialize
  ///   itself back again.
  /// 
  ///   NOTE: The class must also implement a constructor taking a Stream
  ///   to perform the deserialization.
  /// </summary>
  ///==========================================================================
  public interface ISerializableToStream
  {
    void Serialize(Stream xiStream);
  }
}