//********************************************************************************************
//Author: Sergiy Stoyan
//        s.y.stoyan@gmail.com, sergiy.stoyan@outlook.com, stoyan@cliversoft.com
//        http://www.cliversoft.com
//********************************************************************************************

using System;
using Newtonsoft.Json;
using System.Text;

namespace Cliver
{
    /// <summary>
    /// A field/property of this type is implicitly encrypted when it is a member of a Settings class.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Encrypted<T> where T : class
    {
        /// <summary>
        /// (!)The default constructor is used by the deserializer.
        /// </summary>
        public Encrypted()
        {
        }

        public Encrypted(T value)
        {
            Value = value;
        }

        //public Encrypted(StringEndec endec, T value)
        //{
        //    Value = value;
        //    Initialize(endec);
        //}

        /// <summary>
        /// Encypted value. It must not be called from the custom code.
        /// </summary>
        [JsonProperty]//forces serialization for private 
        string _Value { get; set; } = null;

        /// <summary>
        /// Decrypted value to be used in the custom code.
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public T Value
        {
            get
            {
                if (_Value == null)
                    return null;
                //try
                //{
                return Endec.Decrypt<T>(_Value);
                //}
                //catch (Exception e)
                //{
                //    Log.Error("Could not decrypt _Value.", e);
                //    return null;
                //}
            }
            set
            {
                if (value == null)
                    _Value = null;
                else
                    _Value = Endec.Encrypt(value);
            }
        }

        public StringEndec Endec//as a public, it can be used to initialize new Endec instances
        {
            //private set
            //{
            //    if (value == null)
            //        throw new Exception("Endec is NULL which cannot be set.");
            //    if (_endec != null)
            //        throw new Exception("Endec instance is already set and cannot be re-set.");
            //    _endec = value;
            //}
            get
            {
                if (_endec != null)
                    return _endec;
                if (defaultEndec != null)
                {
                    _endec = defaultEndec;
                    return _endec;
                }
                throw new Exception("Endec instance is not set. It can be done by either Initialize() or InitializeDefault() of Cliver.Encrypted class.");
            }
        }
        StringEndec _endec;

        /// <summary>
        /// It must be called before the first Value use (if InitializeDefault() was not called before).
        /// </summary>
        /// <param name="endec"></param>
        /// <exception cref="Exception"></exception>
        public void Initialize(StringEndec endec)
        {
            if (endec == null)
                throw new Exception("Endec is NULL which cannot be set.");
            if (_endec != null)
                throw new Exception("Endec instance is already set and cannot be re-set.");
            _endec = endec;
        }

        /// <summary>
        /// Defines Endec for the whole type.
        /// </summary>
        /// <param name="endec"></param>
        /// <exception cref="Exception"></exception>
        static public void InitializeDefault(StringEndec endec)
        {
            if (endec == null)
                throw new Exception("Endec is NULL which cannot be set.");
            if (defaultEndec != null)
                throw new Exception("Default Endec instance is already set and cannot be re-set.");
            defaultEndec = endec;
        }
        static StringEndec defaultEndec;
    }
}