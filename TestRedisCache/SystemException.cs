using System;
using System.Runtime.Serialization;

namespace TestRedisCache
{
  ///==========================================================================
  /// Class : SystemException
  /// 
  /// <summary>
  ///   A system exception is a fatal exception that is not expected to occur.
  ///   For example, a system exception would be produced if the system could
  ///   not connect to the database
  /// </summary>
  ///==========================================================================
  [Serializable]
  public class SystemException : TestRedisCache.Exception
  {
    ///========================================================================
    /// Class : SWT.SystemException
    /// 
    /// <summary>
    ///   SWT.SystemException constructor
    /// </summary>    
    /// <param name="xiDescription">
    ///   The error description
    /// </param>
    ///========================================================================
    public SystemException(string xiDescription) : base(xiDescription)
    {
    }

    ///========================================================================
    /// Class : SWT.SystemException
    /// 
    /// <summary>
    ///   SWT.SystemException constructor
    /// </summary>
    /// <param name="xiNumber">
    ///   The error number
    /// </param>   /
    /// <param name="xiDescription">
    ///   The error description
    /// </param>
    ///========================================================================
    public SystemException(int xiNumber, string xiDescription) : base(xiNumber, xiDescription)
    {
    }

    ///========================================================================
    /// Class : SWT.SystemException
    /// 
    /// <summary>
    ///   SWT.SystemException constructor
    /// </summary>
    /// <param name="xiNumber">
    ///   The error number
    /// </param>
    /// <param name="xiQualifier">
    ///   The error qualifier
    /// </param>
    /// <param name="xiDescription">
    ///   The error description
    /// </param>
    ///========================================================================
    public SystemException(int xiNumber, string xiQualifier, string xiSource, string xiDescription) :
      base(xiNumber, xiQualifier, xiSource, xiDescription)
    {
    }
    
    ///========================================================================
    /// Class : SWT.SystemException
    /// 
    /// <summary>
    ///   SWT.SystemException constructor
    /// </summary>
    /// <param name="xiDescription"></param>
    /// <param name="xiInnerException"></param>
    ///========================================================================
    public SystemException(string xiDescription, System.Exception xiInnerException) :
      base(xiDescription, xiInnerException)
    {
    }

    public SystemException(
      int xiNumber,
      string xiDescription,
      System.Exception xiInnerException) : base(xiNumber, xiDescription, xiInnerException)
    {
    }

    ///========================================================================
    /// Constructor
    /// 
    /// <summary>
    ///   Deserialization constructor
    /// </summary>
    ///========================================================================
    protected SystemException(SerializationInfo xiInfo, StreamingContext xiContext) 
      : base(xiInfo, xiContext) 
    {
    }
  }
}
