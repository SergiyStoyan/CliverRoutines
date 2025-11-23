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
    public class Newtonsoft_Encrypted<T> : Encrypted<T> where T : class
    {
        /// <summary>
        /// (!)The default constructor is used by the deserializer.
        /// </summary>
        public Newtonsoft_Encrypted() : base() { }

        public Newtonsoft_Encrypted(T value) : base(value) { }

        [JsonProperty]//forces serialization for private 
        override protected string _Value { get; set; } = null;

        /// <summary>
        /// Decrypted value to be used in the custom code.
        /// </summary>
        [JsonIgnore]
        override public T Value
        {
            get
            {
                return base.Value;
            }
            set
            {
                base.Value = value;
            }
        }
    }
}