using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace TestRedisCache.Serialization
{
  internal static class SerializationGuidCache
  {
    private static readonly Dictionary<Type, Guid> mGuidsByType = new Dictionary<Type, Guid>();
    private static readonly Dictionary<Guid, Type> mTypesByGuid = new Dictionary<Guid, Type>();

    public static Guid GetSerializationGuid(this Type xiType)
    {
      Guid lGuid;

      lock (mGuidsByType)
      {
        if (!mGuidsByType.TryGetValue(xiType, out lGuid))
        {
          lGuid = GetGuidFromAttribute(xiType);
          mGuidsByType[xiType] = lGuid;
        }
      }

      return lGuid;
    }

    private static Guid GetGuidFromAttribute(Type xiType)
    {
      var lGuidAttribute = xiType.GetCustomAttributes(typeof (SerializationGuidAttribute), false).FirstOrDefault();

      if (lGuidAttribute == null)
      {
        throw new SerializationException(string.Format("Type {0} has no SerializationGuidAttribute", xiType));
      }

      return ((SerializationGuidAttribute) lGuidAttribute).Guid;
    }

    public static Type GetType(Guid xiGuid)
    {
      Type lType;

      lock (mTypesByGuid)
      {
        if (!mTypesByGuid.TryGetValue(xiGuid, out lType))
        {
          RepopulateTypesByGuid();

          if (!mTypesByGuid.TryGetValue(xiGuid, out lType))
          {
            throw new SerializationException(
              string.Format(
                "Unable to find an ISerializableToString type with GUID {0} in any loaded assembly. Loaded assemblies: {1}",
                xiGuid,
                string.Join(",", GetLoadedAssemblies().Select(assembly => assembly.FullName).ToArray())));
          }
        }
      }

      return lType;
    }

    private static void RepopulateTypesByGuid()
    {
      try
      {
        foreach (var lSerializableType in GetLoadedAssemblies().SelectMany<Assembly, Type>(GetSerializableTypes))
        {
          Guid lGuid = GetGuidFromAttribute(lSerializableType);
          mTypesByGuid[lGuid] = lSerializableType;
        }
      }
      catch(ReflectionTypeLoadException exception)
      {
        var lStringBuilder = new StringBuilder();
        lStringBuilder.AppendLine(exception.Message);
        lStringBuilder.AppendLine("LoaderExceptions:");
        foreach (var lLoaderException in exception.LoaderExceptions)
        {
          lStringBuilder.AppendLine(lLoaderException.Message);
        }

        throw new TestRedisCache.SystemException(lStringBuilder.ToString());
      }
    }

    private static IEnumerable<Assembly> GetLoadedAssemblies()
    {
      return AppDomain.CurrentDomain.GetAssemblies();
    }

    private static IEnumerable<Type> GetSerializableTypes(Assembly xiAssembly)
    {
      try
      {
        return xiAssembly.GetTypes().Where(type => type.IsClass && !type.IsAbstract && typeof (ISerializableToStream).IsAssignableFrom(type));
      }
      catch (ReflectionTypeLoadException lLoadException)
      {
        return lLoadException.Types.Where(type => type != null).Where(type => type.IsClass && !type.IsAbstract && typeof(ISerializableToStream).IsAssignableFrom(type));
      }
    }
  }
}