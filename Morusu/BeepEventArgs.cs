using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Morusu
{
    public class BeepEventArgs : EventArgs
    {
        public BeepType Type
        {
            set; get;
        }
    }

    public enum BeepType
    {
        FirstDah,
        FirstDit,
        SqueezeDah,
        SqueezeDit,
        OnlyDah,
        OnlyDit
    }
}

