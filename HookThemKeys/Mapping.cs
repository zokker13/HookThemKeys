using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace HookThemKeys
{
    public class Mapping
    {
        public Keys Source { get; set; }
        public Keys Target { get; set; }
        public bool Hook { get; set; }

        public Mapping()
        {

        }

        public Mapping(Keys source, Keys target, bool hook = true)
        {
            Source = source;
            Target = target;
            Hook = hook;
        }
    }
}
