using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace grab_vaccine.model
{
    public class Area
    {
        public String Name { set; get; }
        public String Value { set; get; }

        public List<Area> Children { set; get; }
    }
}
