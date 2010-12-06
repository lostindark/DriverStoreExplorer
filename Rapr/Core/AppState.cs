using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Utils;
using System.Security.Principal;

namespace Rapr
{
    public static class AppState
    {
        public static Form MainForm { get; set; }
        public static IDriverStore GetDriverStoreHandler()
        {
            return new PNPUtil();    
        }
        public static bool IsModeReadOnly()
        {
            return false;
        }
    }
}
