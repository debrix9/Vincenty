using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace vincenty
{
    internal class utilsFuncs
    {

        public double flattening()
        {
            double f = (vincenty.constants.a - vincenty.constants.b) / vincenty.constants.a;
            return f;
        }
    }
}
