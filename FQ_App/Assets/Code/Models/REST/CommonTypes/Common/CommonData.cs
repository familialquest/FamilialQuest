using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Code.Models.REST.CommonTypes.Common
{
    public static class CommonData
    {
        public static int requestTimeOut = 30;
        public static DateTime dateTime_FQDB_MinValue = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc); //postgres timestamp minvalue
    }
}
