using UnityEngine;

namespace TutoTOONS.Utils.Debug.Console
{
    public class ActionButton : MonoBehaviour
    {
        public static int actions_couted { get; private set; }
        private RectTransform rect_transform;

        void Start()
        {
            actions_couted = 0;
            rect_transform = transform.GetComponent<RectTransform>();
        }

        public static void Reset()
        {
            actions_couted = 0;
        }

        void Update()
        {
            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                if (RectTransformUtility.RectangleContainsScreenPoint(rect_transform, UnityEngine.Input.mousePosition))
                {
                    actions_couted++;
                }
                else
                {
                    actions_couted = 0;
                }
            }
        }

    }
}
