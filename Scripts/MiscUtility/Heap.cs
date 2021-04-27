using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Heap<T> where T : IHeapItem<T>
{
    T[] items;
    public int Count { get; private set; }

    public Heap(int maxHeapSize) => items = new T[maxHeapSize];

    public void Add(T item)
    {
        if (Count == items.Length)
        {
            Debug.Log("Attempting to add: " + item.ToString() + ", exceeded maxHeapSize");
            return;
        }
        item.HeapIndex = Count;
        items[Count] = item;
        SortUp(item);
        Count++;
    }

    public T RemoveFirst()
    {
        T firstItem = items[0];
        Count--;

        items[0] = items[Count];
        items[0].HeapIndex = 0;
        SortDown(items[0]);

        return firstItem;
    }

    public T GetItem(int index) => items[index];

    public void UpdateItem(T item) => SortUp(item);

    public bool Contains(T item) => Equals(items[item.HeapIndex], item);

    void SortDown(T item)
    {
        while (true)
        {
            int childIndexLeft = item.HeapIndex * 2 + 1;
            int childIndexRight = item.HeapIndex * 2 + 2;
            int swapIndex = 0;

            if (childIndexLeft < Count)
            {
                swapIndex = childIndexLeft;

                if (childIndexRight < Count)
                    if (items[childIndexLeft].CompareTo(items[childIndexRight]) < 0)
                        swapIndex = childIndexRight;

                if (item.CompareTo(items[swapIndex]) < 0)
                    Swap(item, items[swapIndex]);
                else return;
            }
            else return;
        }
    }

    void SortUp(T item)
    {
        int parentIndex = (item.HeapIndex - 1) / 2;
        T parentItem = items[parentIndex];

        while (true)
        {
            parentItem = items[parentIndex];

            if (item.CompareTo(parentItem) > 0)
                Swap(item, parentItem);
            else break;

            parentIndex = (item.HeapIndex - 1) / 2;
        }
    }

    void Swap(T itemA, T itemB)
    {
        items[itemA.HeapIndex] = itemB;
        items[itemB.HeapIndex] = itemA;

        int temp = itemA.HeapIndex;
        itemA.HeapIndex = itemB.HeapIndex;
        itemB.HeapIndex = temp;
    }
}

public interface IHeapItem<T> : IComparable<T>
{
    int HeapIndex { get; set; }
}