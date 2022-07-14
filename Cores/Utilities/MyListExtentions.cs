using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cores.Utilities
{
    public static class MyListExtentions
    {
        public static void Replace<T>(this List<T> list, Predicate<T> oldItemSelector, T newItem)
        {
            //check for different situations here and throw exception
            //if list contains multiple items that match the predicate
            //or check for nullability of list and etc ...
            var oldItemIndex = list.FindIndex(oldItemSelector);
            list[oldItemIndex] = newItem;
        }
    }
}
