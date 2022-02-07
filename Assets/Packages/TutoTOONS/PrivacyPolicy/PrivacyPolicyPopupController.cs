using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TutoTOONS
{
    public class PrivacyPolicyPopupController : MonoBehaviour
    {
        public AudioSource audioPlayer;
        public AudioClip intro;

        private GameObject background;
        private GameObject main;
        private GameObject btn_close;
        private GameObject btn_try_again;


        void Start()
        {
            DontDestroyOnLoad(this);
            background = GameObject.Find("PrivacyPolicyPopupBackground");
            main = GameObject.Find("PrivacyPolicyPopupMain");
            btn_close = GameObject.Find("PrivacyPolicyPopupBtnClose");
            btn_try_again = GameObject.Find("PrivacyPolicyPopupBtnTryAgain");

            background.AddComponent<PrivacyPolicyCallbacks>();
            main.AddComponent<PrivacyPolicyCallbacks>();
            btn_close.AddComponent<PrivacyPolicyCallbacks>();
            btn_try_again.AddComponent<PrivacyPolicyCallbacks>();

            background.GetComponent<PrivacyPolicyCallbacks>().action_callback = OnBackgroundClicked;
            main.GetComponent<PrivacyPolicyCallbacks>().action_callback = OnMainClicked;
            btn_close.GetComponent<PrivacyPolicyCallbacks>().action_callback = OnCloseClicked;
            btn_try_again.GetComponent<PrivacyPolicyCallbacks>().action_callback = OnTryAgainClicked;

            btn_try_again.GetComponent<PrivacyPolicyCallbacks>().anim_completed_callback = OnTryAgainAnimEnd;
            btn_close.GetComponent<PrivacyPolicyCallbacks>().anim_completed_callback = OnCloseAnimEnd;
        }

        private void OnBackgroundClicked()
        {
            Destroy(this.gameObject);
        }

        private void OnMainClicked()
        {
            //
        }

        private void OnCloseClicked()
        {
            btn_close.GetComponent<Animation>().Play();
            btn_close.GetComponent<AudioSource>().Play();
        }

        private void OnTryAgainClicked()
        {
            btn_try_again.GetComponent<Animation>().Play();
            btn_try_again.GetComponent<AudioSource>().Play();
        }

        private void OnTryAgainAnimEnd()
        {
            ///TODOif (Application.internetReachability != NetworkReachability.NotReachable)
            if(true)
            {
                PrivacyPolicyController.Show();
                Destroy(this.gameObject);
            }
        }
        
        private void OnCloseAnimEnd()
        {
            Destroy(this.gameObject);
        }

        void Update()
        {

        }

    }
}
   
