using System;
using System.Runtime.Serialization;

namespace TestRedisCache
{
    ///==========================================================================
    /// Class : InvalidOperationException
    /// 
    /// <summary>
    ///   An exception which results from an invalid operation. Such operations
    ///   are a result of incorrect code, or incorrect usage of an object and 
    ///   should *never* occur at runtime. I.e. they are design-time errors which
    ///   are not picked up at compile-time
    /// </summary>
    ///==========================================================================
    [Serializable]
    public class InvalidOperationException : System.InvalidOperationException
    {
        #region Constructors

        ///========================================================================
        /// Class : InvalidOperationException
        /// 
        /// <summary>
        ///   InvalidOperationException constructor
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
        public InvalidOperationException(
          string xiQualifier,
          string xiSource,
          string xiDescription) : base(xiDescription)
        {
            mSource = xiSource;
            mQualifier = xiQualifier;
            mDescription = xiDescription;
        }

        ///========================================================================
        /// Class : InvalidOperationException
        /// 
        /// <summary>
        ///   InvalidOperationException constructor
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
        public InvalidOperationException(
          string xiQualifier,
          string xiSource,
          string xiDescription,
          System.Exception xiInnerException) :
          base(xiDescription, xiInnerException)
        {
            mSource = xiSource;
            mQualifier = xiQualifier;
            mDescription = xiDescription;
        }

        #endregion

        #region ISerializable implementation

        ///========================================================================
        /// Constructor
        /// 
        /// <summary>
        ///   Deserialization constructor
        /// </summary>
        ///========================================================================
        protected InvalidOperationException(SerializationInfo xiInfo, StreamingContext xiContext)
          : base(xiInfo, xiContext)
        {
            mQualifier = xiInfo.GetString("mQualifier");
            mDescription = xiInfo.GetString("mDescription");
            mSource = xiInfo.GetString("mSource");
            mDetail = xiInfo.GetString("mDetail");
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
            xiInfo.AddValue("mQualifier", mQualifier);
            xiInfo.AddValue("mDescription", mDescription);
            xiInfo.AddValue("mSource", mSource);
            xiInfo.AddValue("mDetail", mDetail);
        }

        #endregion

        #region Obsolete constructors

        public InvalidOperationException(
          string xiMessage,
          params object[] xiArgs) : base(string.Format(xiMessage, xiArgs))
        {
            mDescription = string.Format(xiMessage, xiArgs);
        }

        #endregion

        //=========================================================================
        // Public properties
        //=========================================================================
        public virtual int Number
        {
            get
            {
                return 58002;//Constants.SWT.ERRORS.INVALIDOPERATION;
            }
        }

        public virtual string Qualifier
        {
            get
            {
                return mQualifier;
            }
        }

        public virtual string Description
        {
            get
            {
                return mDescription;
            }
        }

        public override string Message
        {
            get
            {
                return Description + "\n\n" + mDetail;
            }
        }

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
        private string mQualifier = "";
        private string mDescription = "";
        private string mSource = "";
        private string mDetail = "";
    }
}
