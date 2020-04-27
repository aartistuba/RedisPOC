using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;

namespace TestRedisCache.Serialization
{
  internal class BinaryFormatterSerializer<T> : ISerializer<T>
  {
    private readonly Stream mStream;

    public BinaryFormatterSerializer(Stream xiStream)
    {
      mStream = xiStream;
    }

    ///========================================================================
    /// <remarks>
    /// Uses Simple type specification, so the serialized objects are not
    /// bound to a specific version of the assembly. Note that if the 
    /// object is deserialized into a different assembly's type then there
    /// is a risk of binary incompatabilities between different versions of
    /// the object - if this is an issue then the object must implement 
    /// ISerializable directly and cope with all cases.
    /// </remarks>
    ///========================================================================
    public void Serialize(T xiObjectToSerialize)
    {
      mStream.WriteByte((byte) (xiObjectToSerialize == null ? 0 : 1));

      if (xiObjectToSerialize == null)
      {
        return;
      }

      var lFormatter = new BinaryFormatter
        {
          AssemblyFormat = FormatterAssemblyStyle.Simple
        };
      lFormatter.Serialize(mStream, xiObjectToSerialize);
    }

    public T Deserialize()
    {
      bool lIsNull = mStream.ReadByte() == 0;

      if (lIsNull)
      {
        return default(T);
      }

            var lFormatter = new BinaryFormatter();
      //{
      //  //Binder = new Utils.VersionlessAssemblyBinder()
      //};
      try
      {
        object lRet = lFormatter.Deserialize(mStream);
        if (!(lRet is T))
        {
          throw new InvalidOperationException(
            "Serialised object was of type {0}, not expected type {1}",
            lRet.GetType().AssemblyQualifiedName,
            typeof(T).AssemblyQualifiedName);
        }
        return (T) lRet;
      }
      catch (SerializationException e)
      {
        throw new InvalidOperationException(
          null,
          "Deserialize",
          string.Format("Serialization Exception for object of type {0}", typeof(T).AssemblyQualifiedName),
          e
          );
      }
    }
  }
}