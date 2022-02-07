using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TutoTOONS
{
    public class PrivacyPolicyCallbacks : MonoBehaviour, IPointerClickHandler
    {
        public Action action_callback;
        public Action anim_completed_callback;

        private Animation anim;
        private bool anim_prev_state;

        void Start()
        {
            anim = GetComponent<Animation>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (action_callback != null)
            {
                action_callback();
            }
        }

        public void OnAnimationEnd()
        {
            if (anim_completed_callback != null)
            {
                anim_completed_callback();
            }
        }

        void Update()
        {
            if (anim != null)
            {
                if (anim_prev_state && !anim.isPlaying)
                {
                    OnAnimationEnd();
                }
                anim_prev_state = anim.isPlaying;
            }
        }

    }
}

