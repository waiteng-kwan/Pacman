using UnityEngine;
using UnityEngine.UI;

namespace Utils
{
    public enum eRectTransformAnchorPresets
    {
        Center = 0,
        TopLeft,
        Top,
        TopRight,
        Left,
        Right,
        BottomLeft,
        Bottom,
        BottomRight,

        FullStretch,
        HorStretch_Top,
        HorStretch_Middle,
        HorStretch_Bottom,
        VertStretch_Left,
        VertStretch_Middle,
        VertStretch_Right,
    }

    public static class RectTransformUtils 
    {
        private static Vector3[] worldCorners = new Vector3[4];

        public static void SetUIAnchorPresets(RectTransform rectTransform, 
            eRectTransformAnchorPresets preset)
        {
            switch (preset)
            {
                case eRectTransformAnchorPresets.Center:
                    rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                    rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                    rectTransform.pivot = new Vector2(0.5f, 0.5f);
                    break;

                case eRectTransformAnchorPresets.TopLeft:
                    rectTransform.anchorMin = new Vector2(0, 1);
                    rectTransform.anchorMax = new Vector2(0, 1);
                    rectTransform.pivot = new Vector2(0, 1);
                    break;

                case eRectTransformAnchorPresets.Top:
                    rectTransform.anchorMin = new Vector2(0.5f, 1);
                    rectTransform.anchorMax = new Vector2(0.5f, 1);
                    rectTransform.pivot = new Vector2(0.5f, 1);
                    break;

                case eRectTransformAnchorPresets.TopRight:
                    rectTransform.anchorMin = new Vector2(1, 1);
                    rectTransform.anchorMax = new Vector2(1, 1);
                    rectTransform.pivot = new Vector2(1, 1);
                    break;

                case eRectTransformAnchorPresets.Left:
                    rectTransform.anchorMin = new Vector2(0, 0.5f);
                    rectTransform.anchorMax = new Vector2(0, 0.5f);
                    rectTransform.pivot = new Vector2(0, 0.5f);
                    break;

                case eRectTransformAnchorPresets.Right:
                    rectTransform.anchorMin = new Vector2(1, 0.5f);
                    rectTransform.anchorMax = new Vector2(1, 0.5f);
                    rectTransform.pivot = new Vector2(1, 0.5f);
                    break;

                case eRectTransformAnchorPresets.BottomLeft:
                    rectTransform.anchorMin = Vector2.zero;
                    rectTransform.anchorMax = Vector2.zero;
                    rectTransform.pivot = Vector2.zero;
                    break;

                case eRectTransformAnchorPresets.Bottom:
                    rectTransform.anchorMin = new Vector2(0.5f, 0);
                    rectTransform.anchorMax = new Vector2(0.5f, 0);
                    rectTransform.pivot = new Vector2(0.5f, 0);
                    break;

                case eRectTransformAnchorPresets.BottomRight:
                    rectTransform.anchorMin = new Vector2(1, 0);
                    rectTransform.anchorMax = new Vector2(1, 0);
                    rectTransform.pivot = new Vector2(1, 0);
                    break;

                case eRectTransformAnchorPresets.FullStretch:
                    rectTransform.anchorMin = new Vector2(0, 0);
                    rectTransform.anchorMax = new Vector2(1, 1);
                    rectTransform.pivot = new Vector2(0.5f, 0.5f);
                    break;
                case eRectTransformAnchorPresets.HorStretch_Top:
                    rectTransform.anchorMin = new Vector2(0, 1);
                    rectTransform.anchorMax = new Vector2(1, 1);
                    rectTransform.pivot = new Vector2(0.5f, 1);
                    break;
                case eRectTransformAnchorPresets.HorStretch_Middle:
                    rectTransform.anchorMin = new Vector2(1, 0.5f);
                    rectTransform.anchorMax = new Vector2(1, 0.5f);
                    rectTransform.pivot = new Vector2(0.5f, 0.5f);
                    break;
                case eRectTransformAnchorPresets.HorStretch_Bottom:
                    rectTransform.anchorMin = new Vector2(0, 0);
                    rectTransform.anchorMax = new Vector2(1, 0);
                    rectTransform.pivot = new Vector2(0.5f, 0);
                    break;
                case eRectTransformAnchorPresets.VertStretch_Left:
                    rectTransform.anchorMin = new Vector2(0, 0);
                    rectTransform.anchorMax = new Vector2(0, 1);
                    rectTransform.pivot = new Vector2(0, 0.5f);
                    break;
                case eRectTransformAnchorPresets.VertStretch_Middle:
                    rectTransform.anchorMin = new Vector2(0.5f, 0);
                    rectTransform.anchorMax = new Vector2(0.5f, 1);
                    rectTransform.pivot = new Vector2(0.5f, 0.5f);
                    break;
                case eRectTransformAnchorPresets.VertStretch_Right:
                    rectTransform.anchorMin = new Vector2(1, 0);
                    rectTransform.anchorMax = new Vector2(1, 1);
                    rectTransform.pivot = new Vector2(1, 0.5f);
                    break;
            }
        }

        public static void RefreshContentFitter(RectTransform transform)
        {
            if (transform == null || !transform.gameObject.activeSelf)
            {
                return;
            }

            for (int i = 0; i < transform.childCount; ++i)
            {
                if (transform.GetChild(i).TryGetComponent(out RectTransform childRectTransform))
                {
                    RefreshContentFitter(childRectTransform);
                }
            }

            if (transform.TryGetComponent(out LayoutGroup layoutGroup))
            {
                layoutGroup.SetLayoutHorizontal();
                layoutGroup.SetLayoutVertical();
            }
            if (transform.TryGetComponent(out ContentSizeFitter contentSizeFitter))
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(transform);
            }
        }

        public static void RefreshContentFitter(LayoutGroup[] layoutGroups)
        {
            if (layoutGroups == null)
                return;

            for (int i = layoutGroups.Length - 1; i >= 0; --i)
            {
                layoutGroups[i].SetLayoutHorizontal();
                layoutGroups[i].SetLayoutVertical();

                if (layoutGroups[i].gameObject.TryGetComponent(out ContentSizeFitter contentSizeFitter))
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroups[i].GetComponent<RectTransform>());
                }
            }
        }
    }
}