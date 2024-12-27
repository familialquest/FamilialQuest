using Assets.Code.Models.REST.CommonTypes.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code.Models.REST.CommonTypes
{
    public static class DictUtils
    {
        public static bool TryGetAndParseInt(Dictionary<string, string> dictionary, string key, out int value)
        {
            value = int.MinValue;
            if (dictionary.TryGetValue(key, out string stringValue))
            {
                return int.TryParse(stringValue, out value);
            }
            else
                return false;
        }

        public static bool TryGetAndParseTimeSpan(Dictionary<string, string> dictionary, string key, out TimeSpan value)
        {
            value = TimeSpan.MinValue;
            if (dictionary.TryGetValue(key, out string stringValue))
            {
                return TimeSpan.TryParse(stringValue, out value);
            }
            else
                return false;
        }

        public static bool TryGetAndParseDateTime(Dictionary<string, string> dictionary, string key, out DateTime value)
        {
            value = CommonData.dateTime_FQDB_MinValue;
            if (dictionary.TryGetValue(key, out string stringValue))
            {
                return DateTime.TryParse(stringValue, out value);
            }
            else
                return false;
        }

        public static bool TryGetAndParseGuid(Dictionary<string, string> dictionary, string key, out Guid value)
        {
            value = Guid.Empty;
            if (dictionary.TryGetValue(key, out string stringValue))
            {
                return Guid.TryParse(stringValue, out value);
            }
            else
                return false;
        }

        public static bool TryGetAndParseGuidArray(Dictionary<string, string> dictionary, string key, out Guid[] values)
        {
            values = null;
            List<Guid> list = new List<Guid>();
            if (dictionary.TryGetValue(key, out string stringValue))
            {
                stringValue = stringValue.Substring(1, stringValue.Length - 2); // {{}, {}} -> {}, {}
                var stringValues = stringValue.Split(new char[] { ',' });
                foreach (var s in stringValues)
                {
                    if (Guid.TryParse(s, out Guid value))
                        list.Add(value);
                }
                values = list.ToArray();
                return true;
            }
            else
                return false;
        }
    }
}
