using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using SiteServer.Plugin;

namespace SS.Filter.Core
{
    public static class Utils
    {
        public static bool EqualsIgnoreCase(string a, string b)
        {
            if (a == b) return true;
            if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b)) return false;
            return string.Equals(a.Trim().ToLower(), b.Trim().ToLower());
        }

        public static DateTime ToDateTime(string dateTimeStr)
        {
            return ToDateTime(dateTimeStr, DateTime.Now);
        }

        private static DateTime ToDateTime(string dateTimeStr, DateTime defaultValue)
        {
            var datetime = defaultValue;
            if (!string.IsNullOrEmpty(dateTimeStr))
            {
                if (!DateTime.TryParse(dateTimeStr.Trim(), out datetime))
                {
                    datetime = defaultValue;
                }
                return datetime;
            }
            if (datetime <= DateTime.MinValue)
            {
                datetime = DateTime.Now;
            }
            return datetime;
        }

        public static bool ToBool(string boolStr)
        {
            bool boolean;
            if (!bool.TryParse(boolStr?.Trim(), out boolean))
            {
                boolean = false;
            }
            return boolean;
        }

        public static int ToInt(string intStr)
        {
            int i;
            if (!int.TryParse(intStr?.Trim(), out i))
            {
                i = 0;
            }
            return i;
        }

        public static string GetMessageHtml(string message, bool isSuccess)
        {
            return isSuccess
                ? $@"<div class=""alert alert-success"" role=""alert"">{message}</div>"
                : $@"<div class=""alert alert-danger"" role=""alert"">{message}</div>";
        }

        public static string ReplaceNewline(string inputString, string replacement)
        {
            if (string.IsNullOrEmpty(inputString)) return string.Empty;
            var retVal = new StringBuilder();
            inputString = inputString.Trim();
            foreach (var t in inputString)
            {
                switch (t)
                {
                    case '\n':
                        retVal.Append(replacement);
                        break;
                    case '\r':
                        break;
                    default:
                        retVal.Append(t);
                        break;
                }
            }
            return retVal.ToString();
        }

        public static List<int> StringCollectionToIntList(string collection)
        {
            var list = new List<int>();
            if (!string.IsNullOrEmpty(collection))
            {
                var array = collection.Split(',');
                foreach (var s in array)
                {
                    int i;
                    int.TryParse(s.Trim(), out i);
                    list.Add(i);
                }
            }
            return list;
        }
    }
}
