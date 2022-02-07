using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TutoTOONS
{
    public class StartSceneController : MonoBehaviour
    {
        void Start()
        {
            LoadSubscriptionPanel();
        }

        private void LoadSubscriptionPanel()
        {
            GameObject _subscription_panel = Resources.Load("SubscriptionPanel") as GameObject;

            if (_subscription_panel != null)
            {
                Instantiate(_subscription_panel).transform.parent = transform.parent;
            }
        }
    }
}
