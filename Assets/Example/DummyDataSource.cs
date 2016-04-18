using UnityEngine;
using System;
using System.Collections;
using ScrollableListView;

public class DummyDataSource : AbstractScrollViewDataSource
{
    public int[] items;

    public override int CountItems()
    {
        if (null == this.items)
        {
            return 0;
        }

        return this.items.Length;
    }

    public override object GetItem(int idx)
    {
        if (null == this.items || idx < 0 || idx >= this.items.Length)
        {
            return null;
        }

        return (object)this.items[idx];
    }
}
