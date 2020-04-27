using System;
using System.Runtime.Serialization;

namespace TestRedisCache
{
  ///==========================================================================
  /// Class : SWT.Exception
  /// 
  /// <summary>
  ///   General exception class for use within the framework. In general
  ///   the class should be derived from to all catching of more specialized
  ///   exceptions
  /// </summary>
  ///==========================================================================
  [Serializable]
  public class Exception : System.Exception
  {
    #region Constructors

    ///========================================================================
    /// Class : SWT.Exception
    /// 
    /// <summary>
    ///   SWT.Exception constructor
    /// </summary>
    /// <param name="xiNumber">
    ///   The error number
    /// </param>
    /// <param name="xiSource">
    ///   The error source
    /// </param>
    /// <param name="xiQualifier">
    ///   The error qualifier
    /// </param>
    /// <param name="xiDescription">
    ///   The error description
    /// </param>
    ///========================================================================
    public Exception(
      int    xiNumber, 
      string xiQualifier,
      string xiSource,
      string xiDescription)
    {
      mNumber      = xiNumber;
      mSource    = xiSource;
      mQualifier   = xiQualifier;
      mDescription = xiDescription;
    }

    ///========================================================================
    /// Class : SWT.Exception
    /// 
    /// <summary>
    ///   SWT.Exception constructor
    /// </summary>
    /// <param name="xiNumber">
    ///   The error number
    /// </param>
    /// <param name="xiSource">
    ///   The error source
    /// </param>
    /// <param name="xiDescription">
    ///   The error description
    /// </param>
    ///========================================================================
    public Exception(
      int    xiNumber, 
      string xiSource,
      string xiDescription)
    {
      mNumber      = xiNumber;
      mSource    = xiSource;
      mDescription = xiDescription;
    }

    ///========================================================================
    /// Class : SWT.Exception
    /// 
    /// <summary>
    ///   SWT.Exception constructor
    /// </summary>
    /// <param name="xiNumber">
    ///   The error number
    /// </param>
    /// <param name="xiSource">
    ///   The error Source
    /// </param>
    /// <param name="xiQualifier">
    ///   The error qualifier
    /// </param>
    /// <param name="xiDescription">
    ///   The error description
    /// </param>
    ///========================================================================
    public Exception(
      int              xiNumber, 
      string           xiQualifier,
      string           xiSource,
      string           xiDescription,
      System.Exception xiInnerException) :
      base("", xiInnerException)
    {
      mNumber      = xiNumber;
      mSource    = xiSource;
      mQualifier   = xiQualifier;
      mDescription = xiDescription;
    }

    ///========================================================================
    /// Class : SWT.Exception
    /// 
    /// <summary>
    ///   SWT.Exception constructor
    /// </summary>
    /// <param name="xiNumber">
    ///   The error number
    /// </param>
    /// <param name="xiSource">
    ///   The error source
    /// </param>
    /// <param name="xiDescription">
    ///   The error description
    /// </param>
    ///========================================================================
    public Exception(
      int              xiNumber, 
      string           xiSource,
      string           xiDescription,
      System.Exception xiInnerException) :
      base("", xiInnerException)
    {
      mNumber      = xiNumber;
      mSource    = xiSource;
      mDescription = xiDescription;
    }

    #endregion

    #region Obsolete Constructors

    ///========================================================================
    /// Class : SWT.Exception
    /// 
    /// <summary>
    ///   SWT.Exception constructor
    /// </summary>    
    /// <param name="xiDescription">
    ///   The error description
    /// </param>
    ///========================================================================
    public Exception(string xiDescription)
    {      
      mDescription = xiDescription;
    }

    ///========================================================================
    /// Class : SWT.Exception
    /// 
    /// <summary>
    ///   SWT.Exception constructor
    /// </summary>
    /// <param name="xiNumber">
    ///   The error number
    /// </param>   /
    /// <param name="xiDescription">
    ///   The error description
    /// </param>
    ///========================================================================
    public Exception(int xiNumber, string xiDescription)
    {
      mNumber      = xiNumber;      
      mDescription = xiDescription;
    }

    ///========================================================================
    /// Class : SWT.Exception
    /// 
    /// <summary>
    ///   SWT.Exception constructor
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
    public Exception(int xiNumber, int xiQualifier, string xiDescription)
    {
      mNumber      = xiNumber;
      mQualifier   = xiQualifier.ToString();
      mDescription = xiDescription;
    }

    ///========================================================================
    /// Class : SWT.Exception
    /// 
    /// <summary>
    ///   SWT.Exception constructor
    /// </summary>
    /// <param name="xiDescription"></param>
    /// <param name="xiInnerException"></param>
    ///========================================================================
    public Exception(string xiDescription, System.Exception xiInnerException) :
      base("", xiInnerException)
    {
      mDescription = xiDescription;
    }

    ///========================================================================
    /// Class : SWT.Exception
    /// 
    /// <summary>
    ///   SWT.Exception constructor
    /// </summary>
    /// <param name="xiDescription"></param>
    /// <param name="xiInnerException"></param>
    ///========================================================================
    public Exception(int xiNumber, string xiDescription, System.Exception xiInnerException) :
      base("", xiInnerException)
    {
      mDescription = xiDescription;
      mNumber      = xiNumber;
    }

    ///========================================================================
    /// Class : SWT.Exception
    ///
    /// <summary>
    ///   SWT.Exception default constructor
    /// </summary>    
    ///========================================================================
    public Exception()
    {      
    }

    #endregion

    #region ISerializable implementation

    ///========================================================================
    /// Constructor
    /// 
    /// <summary>
    ///   Deserialization constructor
    ///   
    ///   This is required. We didn't make the decision to implement 
    ///   ISerializable ourselves, the base .net exception class did so for us
    /// </summary>
    ///========================================================================
    protected Exception(SerializationInfo xiInfo, StreamingContext xiContext) 
      : base(xiInfo, xiContext) 
    {
      mNumber      = xiInfo.GetInt32("mNumber");
      mQualifier   = xiInfo.GetString("mQualifier");
      mDescription = xiInfo.GetString("mDescription");
      mSource      = xiInfo.GetString("mSource");
      mDetail      = xiInfo.GetString("mDetail");
    }

    ///========================================================================
    /// Method : GetObjectData
    /// 
    /// <summary>
    ///   Serializes the object
    ///   
    ///   This is required. We didn't make the decision to implement 
    ///   ISerializable ourselves, the base .net exception class did so for us
    /// </summary>
    ///========================================================================
    public override void GetObjectData(SerializationInfo xiInfo, StreamingContext xiContext) 
    {
      base.GetObjectData(xiInfo, xiContext);
      xiInfo.AddValue("mNumber", mNumber);
      xiInfo.AddValue("mQualifier", mQualifier);
      xiInfo.AddValue("mDescription", mDescription);      
      xiInfo.AddValue("mSource", mSource);
      xiInfo.AddValue("mDetail", mDetail);
    }  

    #endregion

    ///========================================================================
    /// Method : GetEwsMessage
    /// 
    /// <summary>
    ///   Returns a long-description of the error with any associated details
    ///   for EWS reporting. The default implementation just returns the 
    ///   exception's standard message, but this can be overridden by derived
    ///   classes to provide more useful diagnostics
    /// </summary>
    ///========================================================================
    public virtual string GetEwsMessage()
    {
      return this.Message;
    }

    //=========================================================================
    // Public properties
    //=========================================================================
    public virtual int Number
    {
      get
      {
        return mNumber;
      }      
    }

    ///========================================================================
    /// Property : Qualifier
    /// 
    /// <summary>
    /// 	Optional qualifier to distinguish different errors with the same 
    /// 	number - e.g. a 404 exception might use the page name as a qualifier
    /// </summary>
    ///========================================================================
    public virtual string Qualifier
    {
      get
      {
        return mQualifier;
      }      
    }

    ///========================================================================
    /// Property : Description
    /// 
    /// <summary>
    /// 	A short description of the error. This should be limited to a single
    /// 	sentence or so, any longer text should be stored in the Detail 
    /// 	property
    /// </summary>
    ///========================================================================
    public virtual string Description
    {
      get
      {
        return mDescription;
      }      
    }

    ///========================================================================
    /// Property : Message
    /// 
    /// <summary>
    /// 	A more complete description of the error - this overrides the base
    /// 	member and includes both the description and detail properties
    /// 	
    /// 	Derived classes should override the Description and Detail properties
    /// 	seperately, and not change the Message property
    /// </summary>
    ///========================================================================
    public override string Message
    {
      get
      {
        return Description + "\r\n\r\n" + Detail;
      }
    }

    ///========================================================================
    /// Property : Detail
    /// 
    /// <summary>
    /// 	A more detailed description of the error. 
    /// </summary>
    ///========================================================================
    public virtual string Detail
    {
      get
      {
        return mDetail;
      }
    }

    ///========================================================================
    /// Property : Source
    /// 
    /// <summary>
    /// 	The code area where the message was generated e.g. "SqlXml" for 
    /// 	any errors in code in the SWT.SqlXml namespace 
    /// </summary>
    ///========================================================================
    public override string Source
    {
      get
      {
        return mSource;
      }
    }

    ///========================================================================
    /// Method : AppendDetail
    /// 
    /// <summary>
    /// 	Allows additional information to be added to the exception after
    /// 	construction, while ensuring the original details are preserved
    /// </summary>
    ///========================================================================
    public void AppendDetail(string xiAddedDetail)
    {
      mDetail += "\n\n" + xiAddedDetail;
    }

    //=========================================================================
    // Private (internal) members
    //=========================================================================
    private int     mNumber      = -1;
    private string  mQualifier   = "";
    private string  mDescription = "";
    private string  mSource      = "";
    private string  mDetail      = "";
  }
}
