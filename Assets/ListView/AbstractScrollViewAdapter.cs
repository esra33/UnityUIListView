using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

namespace ScrollableListView
{
    public abstract class AbstractScrollViewAdapter : MonoBehaviour
    {
        protected ListView context;

        /// <summary>
        /// Custom setup of the components of this adapter
        /// </summary>
        /// <param name="item"></param>
        public abstract void Setup(System.Object item);

        /// <summary>
        /// Disposes of the current cell sending it to the 
        /// view list object pool
        /// </summary>
        public void Dispose()
        {
            if(null != this.context)
            {
                this.context.DisposeAdapter(this);
            }
        }

        /// <summary>
        /// Calculates the dimensions of this adapter item by 
        /// adding all the internal AABB rects until we have
        /// the leftmost corner and the size of the rect
        /// </summary>
        /// <returns></returns>
        internal Rect CalculateDimensions()
        {
            return this.CalculateDimensionsFor((RectTransform)this.transform);
        }

        internal void SetContext(ListView context)
        {
            this.context = context;
        }

        /// <summary>
        /// Calculates the dimensions for a specific rect transform
        /// and does so recursively for all the children of the transform
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private Rect CalculateDimensionsFor(RectTransform t)
        {
            Rect baseSize = new Rect(t.rect);
            
            foreach(RectTransform child in t)
            {
                baseSize = this.Union(baseSize, CalculateDimensionsFor(child));
            }         

            return baseSize;
        }
        
        /// <summary>
        /// Unites two rects to create a bigger one that encompases both
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private Rect Union(Rect a, Rect b)
        {
            Rect result = new Rect(a);
            result.x = Mathf.Min(result.x, b.x);
            result.y = Mathf.Min(result.y, b.y);
            result.width = Mathf.Max(result.width, b.width);
            result.height = Mathf.Max(result.height, b.height);
            return result;
        }
    }
}