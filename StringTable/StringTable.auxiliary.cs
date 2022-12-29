//********************************************************************************************
//Author: Sergiy Stoyan
//        systoyan@gmail.com
//        http://www.cliversoft.com
//********************************************************************************************
using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Drawing;

namespace Cliver
{
    public abstract partial class StringTable
    {
        public void Read(IList<IList<object>> valuess, Modes mode)
        {
            int y = 0;
            read(mode, () => { return valuess[y++].Select(a => a.ToString()).ToList(); });
        }
    }
}