using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCRI.Utils
{
    static class GeneralUtils
    {
        /// <summary>
        /// Gets a string list from an enum to use it eg. in Comboboxes
        /// </summary>
        /// <param name="enumType"></param>
        /// <returns></returns>
        public static List<string> enumToStringList(Type enumType )
        {
            if (!enumType.IsEnum)
                return null;
            return Enum.GetNames(enumType).ToList();
        }
    }
}
