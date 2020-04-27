using System;

namespace TestRedisCache.Serialization
{
  [AttributeUsage(AttributeTargets.Class, AllowMultiple=false, Inherited=false)]
  public class SerializationGuidAttribute : Attribute
  {
    internal Guid Guid { get; private set; }

    public SerializationGuidAttribute(string xiGuidAsString)
    {
      Guid = new Guid(xiGuidAsString);
    }
  }
}