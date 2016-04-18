using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace ScrollableListView
{
    [ExecuteInEditMode]
    public class ListView : ScrollRect
    {
        /// <summary>
        /// The data source to use
        /// </summary>
        [SerializeField]
        private AbstractScrollViewDataSource dataSource;

        /// <summary>
        /// An adaptor that represents one item in the data source
        /// </summary>
        [SerializeField]
        private AbstractScrollViewAdapter baseAdapter;

        [SerializeField, Tooltip("Any cells that intercept thie area designed by this transform will be rendered, otherwise they will be deactivated and ignored")]
        private RectTransform contentBuffer;

        [SerializeField, Tooltip("If specified then system will use the specified vertical layout with some changes, if not then system will create one")]
        private VerticalLayoutGroup verticalLayout;

        /// <summary>
        /// Keeps track of the items in the list
        /// </summary>
        private int itemCount;

        /// <summary>
        /// Cached size of the adapter
        /// </summary>
        private Rect adapterSize;

        /// <summary>
        /// Object Pool Pattern, we don't destroy an adapter we just recycle it
        /// </summary>
        [SerializeField, HideInInspector]
        private GameObject adaptersPoolContainer;

        /// <summary>
        /// Contains the anchors generated for this list
        /// </summary>
        [SerializeField, HideInInspector]
        private List<RectTransform> anchors = new List<RectTransform>();

        /// <summary>
        /// Recycles and removes an adapter returning it to the pool
        /// </summary>
        /// <param name="adapter">the adapter to return to the pool</param>
        internal void DisposeAdapter(AbstractScrollViewAdapter adapter)
        {
            if (null == adapter)
            {
                return;
            }

            adapter.gameObject.SetActive(false);
            adapter.transform.SetParent(this.adaptersPoolContainer.transform);
        }
        
        protected override void Start()
        {
            base.Start();

            if(Application.isPlaying)
            {
                this.TearDown();
                this.Setup();

                // Refresh adapters in list
                this.StartCoroutine(this.ExecuteAfterFrame(this.RefreshAdapters));
            }
        }
        
        private void Update()
        {
            this.RefreshAdapters();
        }
        
        /// <summary>
        /// Initializes the system and creates the base elements based on the given settings
        /// </summary>
        protected void Setup()
        {
            if (null == this.dataSource || null == this.baseAdapter || null == this.contentBuffer || null == this.verticalLayout)
            {
                this.itemCount = 0;
                return;
            }

            // Add layout components
            this.SetupVerticalLayout();

            // Create pool
            this.CreateAdaptersPoolContainer();

            // Calculate the dimensions of the adaptor 
            this.adapterSize = this.baseAdapter.CalculateDimensions();
            this.itemCount = this.dataSource.CountItems();

            // Change the content buffer width to adjust for the adapter width
            this.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, this.adapterSize.width);
            this.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, this.adapterSize.height * itemCount);

            this.contentBuffer.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, this.adapterSize.width);

            // Fill content with adapter anchors and use them as trackers
            GameObject anchor = this.CreateAdapterAnchor();
            this.AddAnchorToList(anchor);
            for (int i = 1; i < this.itemCount; ++i)
            {
                GameObject anchorClone = Instantiate(anchor, Vector3.zero, Quaternion.identity) as GameObject;
                this.AddAnchorToList(anchorClone);
            }
        }

        /// <summary>
        /// Removes all anchors and adapter instances from the system
        /// </summary>
        protected void TearDown()
        {
#if UNITY_EDITOR
            if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
            {
                this.TearDownOnPlay();
            }
            else
            {
                if (null != this.adaptersPoolContainer)
                {
                    DestroyImmediate(this.adaptersPoolContainer.gameObject);
                    this.adaptersPoolContainer = null;
                }

                foreach (RectTransform transform in this.anchors)
                {
                    if (null != transform && null != transform.gameObject)
                    {
                        DestroyImmediate(transform.gameObject);
                    }
                }

                this.anchors.Clear();
            }
#else
           this.TearDownOnPlay();
#endif
        }

        /// <summary>
        /// Specific variant for when in play mode
        /// </summary>
        private void TearDownOnPlay()
        {
            if (null != this.adaptersPoolContainer)
            {
                Destroy(this.adaptersPoolContainer.gameObject);
                this.adaptersPoolContainer = null;
            }

            foreach (RectTransform transform in this.anchors)
            {
                if (null != transform && null != transform.gameObject)
                {
                    Destroy(transform.gameObject);
                }
            }

            this.anchors.Clear();
        }

        /// <summary>
        /// Checks if an achour is inside of the buffer area
        /// </summary>
        /// <param name="anchor"></param>
        /// <returns></returns>
        private bool EvaluateAdapterAtPosition(RectTransform anchor)
        {
            Rect anchorRect = new Rect(this.adapterSize);
            anchorRect.center = anchor.position;

            Rect contentBufferRect = new Rect(this.contentBuffer.rect);
            contentBufferRect.center = this.contentBuffer.position;

            return anchorRect.Overlaps(contentBufferRect);
        }

        /// <summary>
        /// Adds an anchor to the current list of anchors
        /// </summary>
        /// <param name="anchor"></param>
        private void AddAnchorToList(GameObject anchor)
        {
            RectTransform anchorTransform = anchor.transform as RectTransform;
            anchorTransform.SetParent(this.verticalLayout.transform);
            this.anchors.Add(anchorTransform);
        }

        /// <summary>
        /// Checks what anchors are visible and adds adaptors to them if they don't already have them
        /// </summary>
        private void RefreshAdapters()
        {
            for (int i = 0, max = Mathf.Min(this.itemCount, this.anchors.Count); i < max; ++i)
            {
                RectTransform anchor = this.anchors[i];
                bool isAnchorActive = this.EvaluateAdapterAtPosition(anchor);
                bool isAnchorSetup = anchor.childCount > 0;
                if (isAnchorActive && isAnchorSetup || !isAnchorActive && !isAnchorSetup)
                {
                    continue;
                }

                if (isAnchorActive && !isAnchorSetup)
                {
                    AbstractScrollViewAdapter adapter = this.GetAdapter();
                    adapter.transform.SetParent(anchor);
                    adapter.Setup(this.dataSource.GetItem(i));
                    adapter.gameObject.SetActive(true);
                    continue;
                }

                if(!isAnchorActive && isAnchorSetup)
                {
                    AbstractScrollViewAdapter adapter = anchor.GetComponentInChildren<AbstractScrollViewAdapter>() as AbstractScrollViewAdapter;
                    if(null != adapter)
                    {
                        this.DisposeAdapter(adapter);
                    }
                }
            }
        }

        /// <summary>
        /// Creates a new anchor based on the dimensions of the original adapter
        /// </summary>
        /// <returns></returns>
        private GameObject CreateAdapterAnchor()
        {
            GameObject anchor = new GameObject(string.Format("{0}ListViewAnchor", this.gameObject.name));

            // Configure anchor so that it is the size of the adapter
            LayoutElement layoutElement = anchor.AddComponent<LayoutElement>() as LayoutElement;
            layoutElement.minHeight = this.adapterSize.height;
            layoutElement.minWidth = this.adapterSize.width;
            layoutElement.flexibleHeight = 0.0f;
            layoutElement.flexibleWidth = 0.0f;

            // Add a vertical layout group to ensure the adapter fits the anchor
            anchor.AddComponent<VerticalLayoutGroup>();

            return anchor;
        }

        /// <summary>
        /// Checks if an adapter is available from the recycled list otherwise creates an instance of one
        /// </summary>
        /// <returns></returns>
        private AbstractScrollViewAdapter GetAdapter()
        {
            if (this.adaptersPoolContainer.transform.childCount > 0)
            {
                // Manually find a child adaptor if possible (GetComponentInChildren doesn't work)
                foreach (Transform child in this.adaptersPoolContainer.transform)
                {
                    if (null != child)
                    {
                        AbstractScrollViewAdapter childAdapter = child.GetComponent<AbstractScrollViewAdapter>() as AbstractScrollViewAdapter;
                        if (null != childAdapter)
                        {
                            return childAdapter;
                        }
                    }
                }
            }

            AbstractScrollViewAdapter adapter = Instantiate<AbstractScrollViewAdapter>(this.baseAdapter) as AbstractScrollViewAdapter;
            adapter.gameObject.SetActive(false);
            adapter.transform.SetParent(this.adaptersPoolContainer.transform);
            adapter.SetContext(this);

            return adapter;
        }

        /// <summary>
        /// Creates a container for the recycled adapters
        /// </summary>
        private void CreateAdaptersPoolContainer()
        {
            if(null != this.adaptersPoolContainer)
            {
                return;
            }

            this.adaptersPoolContainer = new GameObject("CellsPool");
            this.adaptersPoolContainer.transform.SetParent(this.transform);
            this.adaptersPoolContainer.transform.localPosition = Vector3.zero;
        }

        /// <summary>
        /// Setups a vertical layout. Future development should allow us to do horizontal and grid
        /// </summary>
        private void SetupVerticalLayout()
        {
            if (null == this.verticalLayout)
            {
                GameObject verticalLayoutGO = new GameObject("VerticalLayout");
                this.verticalLayout = verticalLayoutGO.AddComponent<VerticalLayoutGroup>() as VerticalLayoutGroup;
            }

            RectTransform verticalLayoutTransform = this.verticalLayout.transform as RectTransform;

            // Setup location
            verticalLayoutTransform.SetParent(this.content);
            verticalLayoutTransform.anchorMin = Vector2.zero;
            verticalLayoutTransform.anchorMax = Vector3.one;
            verticalLayoutTransform.pivot = new Vector2(0.0f, 1.0f);
            verticalLayoutTransform.anchoredPosition = Vector2.zero;

            // Setup configuration
            this.verticalLayout.childForceExpandHeight = false;
            this.verticalLayout.childForceExpandWidth = false;
            this.verticalLayout.childAlignment = TextAnchor.UpperLeft;
        }

        private IEnumerator ExecuteAfterFrame(Action method)
        {
            yield return new WaitForEndOfFrame();
            if (null != method)
            {
                method();
            }
            yield break;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Gizmos displaying the buffer area vs the adapters
        /// </summary>
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(this.contentBuffer.position, this.contentBuffer.rect.size);

            for (int i = 0; i < this.anchors.Count; ++i)
            {
                if(this.anchors[i].childCount > 0)
                {
                    Gizmos.color = Color.green;
                }
                else
                {
                    Gizmos.color = Color.blue;
                }

                RectTransform anchor = this.anchors[i];
                Gizmos.DrawWireCube(anchor.position, anchor.rect.size);
            }
        }

        /// <summary>
        /// Makes sure the one gets an idea of how it looks on editor before play
        /// </summary>
        protected override void OnValidate()
        {
            if(UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            UnityEditor.EditorApplication.delayCall = () =>
            {
                this.TearDown();
                UnityEditor.EditorApplication.delayCall = () =>
                {
                    this.Setup();
                    UnityEditor.EditorApplication.delayCall = () =>
                    {
                        this.RefreshAdapters();
                    };
                };
            };

            UnityEditor.EditorUtility.SetDirty(this.gameObject);

            base.OnValidate();
        }
    }
#endif
}