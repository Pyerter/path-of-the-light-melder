using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ListUtility
{
    public static void InsertIntoSortedList<T>(List<T> list, T element, IComparer<T> comparer)
    {
        int index = list.BinarySearch(element, comparer);
        if (index < 0) index = ~index;
        list.Insert(index, element);
    }

    public static bool TryInsertIntoSortedList<T>(List<T> list, T element, IComparer<T> comparer)
    {
        int index = list.BinarySearch(element, comparer);
        if (index < 0)
        {
            index = ~index;
            list.Insert(index, element);
            return true;
        }
        return false;
    }

    public static bool TryPopFromSortedList<T>(List<T> list, T target, IComparer<T> comparer, out T popped)
    {
        int index = list.BinarySearch(target, comparer);
        if (index < 0)
        {
            popped = default;
            return false;
        }
        popped = list[index];
        list.RemoveAt(index);
        return true;
    }

    public static bool TryGetIndexInSortedList<T>(List<T> list, T element, IComparer<T> comparer, out int index)
    {
        index = list.BinarySearch(element, comparer);
        return index >= 0;
    }

    public static bool SortedListContains<T>(List<T> list, T element, IComparer<T> comparer)
    {
        int index = list.BinarySearch(element, comparer);
        return index >= 0;
    }
}
