using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkTool.Model
{
    public class Port
    {
        public int PortNumber { get; set; }
        public string Protocol { get; set; }
        public bool IsOpen { get; set; }
        public string Service { get; set; }
    }
}
