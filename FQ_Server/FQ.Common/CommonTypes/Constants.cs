using System;
using System.Collections.Generic;
using System.Text;

namespace CommonTypes
{
    public static class Constants
    {
        public static DateTime POSTGRES_DATETIME_MINVALUE = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc); //postgres timestamp minvalue
    }
}
