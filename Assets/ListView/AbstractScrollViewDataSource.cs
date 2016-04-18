using UnityEngine;
using System;
using System.Collections;

namespace ScrollableListView
{
    public abstract class AbstractScrollViewDataSource : MonoBehaviour
    {
        /// <summary>
        /// Returns the amount of items in the data source
        /// </summary>
        /// <returns></returns>
        public abstract int CountItems();

        /// <summary>
        /// Returns a specific item in the data source
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public abstract System.Object GetItem(int idx);
    }
}
