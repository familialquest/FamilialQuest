using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommonLib
{
    public class ActionData
    {
        public string actionName;
        public object postData;

        public ActionData()
        {

        }

        public ActionData(bool empty)
        {
            if (empty)
            {
                actionName = string.Empty;
                postData = string.Empty;
            }
            else
            {
                actionName = null;
                postData = null;
            }
        }
    }
}
