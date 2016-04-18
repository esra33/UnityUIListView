using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using ScrollableListView;
using System;

public class DummyAdapter : AbstractScrollViewAdapter
{
    [SerializeField]
    private Text text;

    public override void Setup(object item)
    {
        if(null == item)
        {
            this.text.text = "-1";
        }

        int value = (int)item;
        this.text.text = value.ToString();
    }
}
