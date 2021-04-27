using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyCountingSet<T> : IEnumerable
{
    public int Count { get { return objects.Count; } }

    readonly int startingMemoryCount;

    List<T> objects;
    List<int> counts;

    public MyCountingSet(int startingMemory)
    {
        objects = new List<T>();
        counts = new List<int>();
        startingMemoryCount = startingMemory;
    }

    public bool Add(T newObj)
    {
        if (objects.Contains(newObj))
        {
            int objectIndex = objects.IndexOf(newObj);
            counts[objectIndex] = startingMemoryCount;

            return false;
        }

        objects.Add(newObj);
        counts.Add(startingMemoryCount);

        return true;
    }

    public void CountDown()
    {
        for (int i = 0; i < counts.Count; i++)
        {
            if (--counts[i] <= 0)
            {
                objects.RemoveAt(i);
                counts.RemoveAt(i);
            }
        }
    }

    public void Clear()
    {
        objects.Clear();
        counts.Clear();
    }

    public void Clear(Func<List<T>, bool> OnClear)
    {
        OnClear(objects);

        objects.Clear();
        counts.Clear();
    }

    public void CullNulls()
    {
        for (int i = 0; i < counts.Count; i++)
        {
            if (objects[i] == null)
            {
                objects.RemoveAt(i);
                counts.RemoveAt(i);
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public Enumerator GetEnumerator()
    {
        return new Enumerator(objects.ToArray());
    }

    public class Enumerator : IEnumerator
    {
        public T[] objects;
        int position = -1;

        public Enumerator(T[] objs) { objects = objs; }

        public bool MoveNext()
        {
            position++;
            return (position < objects.Length);
        }
        public void Reset() => position = -1;

        object IEnumerator.Current { get { return Current; } }
        public T Current
        {
            get
            {
                try { return objects[position]; }
                catch (IndexOutOfRangeException) { throw new InvalidOperationException(); }
            }
        }
    }
}
