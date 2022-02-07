using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace TutoTOONS
{
    public class AdPreferences 
    {
        public enum BannerSize {SMALL, LARGE, SMART};
        public enum BannerPosition {TOP, BOTTOM};

        public BannerSize banner_size { get; set; }
        public BannerPosition banner_position { get; set; }
    }
}