namespace TestRedisCache.Serialization
{
  internal interface ISerializer<T>
  {
    void Serialize(T xiObjectToSerialize);

    T Deserialize();
  }
}