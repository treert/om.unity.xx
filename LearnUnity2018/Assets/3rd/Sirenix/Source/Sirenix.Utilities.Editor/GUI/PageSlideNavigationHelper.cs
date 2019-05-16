#if UNITY_EDITOR
//-----------------------------------------------------------------------// <copyright file="PageSlideNavigationHelper.cs" company="Sirenix IVS"> // Copyright (c) Sirenix IVS. All rights reserved.// </copyright>//-----------------------------------------------------------------------
//-----------------------------------------------------------------------
// <copyright file="PageSlideNavigationHelper.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.Utilities.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public class SlidePageNavigationHelper<T>
    {
        private List<Page> pages;
        private GUITabGroup tabGroup;
        private Page prev;

        public SlidePageNavigationHelper()
        {
            this.tabGroup = new GUITabGroup();
            this.tabGroup.AnimationSpeed = 4;
            this.pages = new List<Page>();
        }

        public IEnumerable<Page> EnumeratePages
        {
            get
            {
                bool doPrev = true;

                for (int i = Math.Max(0, this.pages.Count - 3); i < this.pages.Count; i++)
                {
                    var p = this.pages[i];
                    if (p == this.prev)
                    {
                        doPrev = false;
                    }
                    yield return p;
                }

                if (this.prev != null && doPrev)
                {
                    yield return this.prev;
                }
            }
        }

        public void PushPage(T obj, string name)
        {
            var tab = this.tabGroup.RegisterTab(name);
            var page = new Page(obj, tab, name);
            this.pages.Add(page);
            this.tabGroup.GoToPage(page.Tab);
            this.prev = null;
        }

        public void NavigateBack()
        {
            if (this.IsOnFirstPage)
            {
                return;
            }

            this.prev = this.pages.Last();
            this.pages.RemoveAt(this.pages.Count - 1);
            this.tabGroup.GoToPage(this.pages[this.pages.Count - 1].Tab);
        }

        public void NavigateBack(int index)
        {
            if (this.IsOnFirstPage)
            {
                return;
            }

            this.prev = this.pages.Last();
            this.pages.SetLength(index);
            this.tabGroup.GoToPage(this.pages[this.pages.Count - 1].Tab);
        }

        public void DrawPageNavigation(Rect rect)
        {
            var leftBtnRect = rect.AlignLeft(rect.height * 1.3f);
            GUIHelper.PushGUIEnabled(!this.IsOnFirstPage);
            if (GUI.Button(leftBtnRect, GUIContent.none, GUIStyle.none))
            {
                this.NavigateBack();
            }
            EditorIcons.TriangleLeft.Draw(leftBtnRect, 19);
            GUIHelper.PopGUIEnabled();

            rect.xMin += rect.height;

            var totalLength = 0;
            for (int i = this.pages.Count - 1; i >= 0; i--)
            {
                var p = this.pages[i];
                if (!p.TitleWidth.HasValue) p.TitleWidth = (int)SirenixGUIStyles.BoldLabel.CalcSize(new GUIContent(p.Name)).x + 7;
                totalLength += p.TitleWidth.Value;
            }

            rect.width -= 10;

            var cut = rect.xMin;
            if (totalLength > rect.width)
            {
                rect.xMin -= totalLength - rect.width;
            }

            for (int i = 0; i < this.pages.Count; i++)
            {
                var p = this.pages[i];
                if (!p.TitleWidth.HasValue) p.TitleWidth = (int)SirenixGUIStyles.BoldLabel.CalcSize(new GUIContent(p.Name)).x + 7;
                rect.width = p.TitleWidth.Value;

                var btnRect = rect;
                btnRect.width -= 6;
                btnRect.xMin = Mathf.Max(cut, btnRect.xMin);
                //var hover = btnRect.Contains(Event.current.mousePosition);
                //var active = i == this.pages.Count - 1;

                if (GUI.Button(btnRect, p.Name, SirenixGUIStyles.LabelCentered))
                {
                    this.NavigateBack(i + 1);
                }
                if (i != this.pages.Count - 1)
                {
                    var lblRect = btnRect.AlignRight(10);
                    lblRect.x += 10;
                    lblRect.xMin = Mathf.Max(cut, lblRect.xMin);
                    GUI.Label(lblRect, "/", SirenixGUIStyles.LabelCentered);
                }
                rect.x += rect.width;
            }
        }

        public bool IsOnFirstPage
        {
            get { return this.pages.Count <= 1; }
        }

        public void BeginGroup()
        {
            this.tabGroup.BeginGroup(false, GUIStyle.none);
        }

        public void EndGroup()
        {
            this.tabGroup.EndGroup();
        }

        public class Page
        {
            public T Value;
            public string Name;
            internal int? TitleWidth;
            internal GUITabPage Tab;

            public bool BeginPage()
            {
                return this.Tab.BeginPage();
            }

            public void EndPage()
            {
                this.Tab.EndPage();
            }

            public Page(T @object, GUITabPage tab, string name)
            {
                this.Value = @object;
                this.Name = name;
                this.Tab = tab;
            }
        }
    }
}
#endif