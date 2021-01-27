using System;
using System.Collections.Generic;
using System.Text;
using Cliver;

namespace Example
{
    class Settings
    {
        /// <summary>
        ///Settings type field can be declared anywhere in the code. It must be public--static to be processed by Config.
        ///Also, it can be declared readonly which is optional because sometimes the logic of the app may require replacing the value.
        /// </summary>
        public static GeneralSettings General;
        public static SmtpSettings Smtp;
    }
}
