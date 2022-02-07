using System.Collections;
using System.Collections.Generic;

namespace TutoTOONS
{
    public class AdLocation
    {
        public static AdLocationKeyword KEYWORD_START = new AdLocationKeyword("start");
        public static AdLocationKeyword KEYWORD_BETWEEN_SCENES = new AdLocationKeyword("between_scenes");
        public static AdLocationKeyword KEYWORD_MOVIE_THEATER = new AdLocationKeyword("movie_theater");
        public static AdLocationKeyword KEYWORD_SCENE_BANNER = new AdLocationKeyword("scene_banner");
        public static AdLocationKeyword KEYWORD_BRANCH_END = new AdLocationKeyword("forced");
        public static AdLocationKeyword KEYWORD_PURCHASE_BRANCH = new AdLocationKeyword("purchase_branch");
        public static AdLocationKeyword KEYWORD_PURCHASE_UNLOCK_ALL = new AdLocationKeyword("purchase_unlock_all");
        public static AdLocationKeyword KEYWORD_PURCHASE_TIME_LIMIT = new AdLocationKeyword("purchase_time_limit");
        public static AdLocationKeyword KEYWORD_DRESS_UP_ITEM = new AdLocationKeyword("dressup_item");
        public static AdLocationKeyword KEYWORD_DECORATE_ITEM = new AdLocationKeyword("decorate_item");
        public static AdLocationKeyword KEYWORD_DRAWING_COLOR = new AdLocationKeyword("drawing_color");
        public static AdLocationKeyword KEYWORD_DRAWING_ITEM = new AdLocationKeyword("drawing_item");
        public static AdLocationKeyword KEYWORD_MAIN_MAP_PANEL = new AdLocationKeyword("main_map_panel");
        public static AdLocationKeyword KEYWORD_MAP_PANEL = new AdLocationKeyword("map_panel");
        public static AdLocationKeyword KEYWORD_START_PANEL = new AdLocationKeyword("start_panel");
        public static AdLocationKeyword KEYWORD_PURCHASE_LIVE = new AdLocationKeyword("purchase_live");
        public static AdLocationKeyword KEYWORD_INCREASE_STATUS = new AdLocationKeyword("increase_status");
        public static AdLocationKeyword KEYWORD_DRAG_ITEM = new AdLocationKeyword("drag_and_drop_item");
        public static AdLocationKeyword KEYWORD_REWARD_COINS = new AdLocationKeyword("reward_coins");
        public static AdLocationKeyword KEYWORD_DOUBLE_COINS = new AdLocationKeyword("double_coins");
        public static AdLocationKeyword KEYWORD_TOOL_UNLOCK = new AdLocationKeyword("tool_unlock");
        public static AdLocationKeyword KEYWORD_NOT_ENOUGH_COINS = new AdLocationKeyword("not_enough_coins");
        public static AdLocationKeyword KEYWORD_TIMED_GIFT = new AdLocationKeyword("timed_gift");
        public static AdLocationKeyword KEYWORD_CUSTOM_MORE_APPS = new AdLocationKeyword("custom_more_apps");
        public static AdLocationKeyword KEYWORD_WATCH_AD_BUTTON = new AdLocationKeyword("watch_ad_button");
        public static AdLocationKeyword KEYWORD_BRANCH_START_END = new AdLocationKeyword("branch_start_end");
        public static AdLocationKeyword KEYWORD_DEBUG_CONSOLE = new AdLocationKeyword("debug_console"); // This location is used only for ads which is called from debug console


        public static AdLocationKeyword[] forced_locations = { KEYWORD_START, KEYWORD_BETWEEN_SCENES, KEYWORD_BRANCH_END, KEYWORD_BRANCH_START_END };
        public static AdLocationKeyword[] soft_locations = { KEYWORD_START_PANEL, KEYWORD_MAIN_MAP_PANEL, KEYWORD_MAP_PANEL, KEYWORD_MOVIE_THEATER };
        public static AdLocationKeyword[] all_locations = { KEYWORD_START, KEYWORD_BETWEEN_SCENES, KEYWORD_MOVIE_THEATER, KEYWORD_SCENE_BANNER,  KEYWORD_BRANCH_END,  KEYWORD_PURCHASE_BRANCH,
                 KEYWORD_PURCHASE_UNLOCK_ALL,  KEYWORD_PURCHASE_TIME_LIMIT, KEYWORD_DRESS_UP_ITEM, KEYWORD_DECORATE_ITEM, KEYWORD_DRAWING_COLOR, KEYWORD_DRAWING_ITEM, KEYWORD_MAIN_MAP_PANEL,
                 KEYWORD_MAP_PANEL,  KEYWORD_START_PANEL,  KEYWORD_PURCHASE_LIVE,  KEYWORD_INCREASE_STATUS,  KEYWORD_DRAG_ITEM,  KEYWORD_REWARD_COINS,  KEYWORD_DOUBLE_COINS,  KEYWORD_TOOL_UNLOCK,
                 KEYWORD_NOT_ENOUGH_COINS,  KEYWORD_TIMED_GIFT, KEYWORD_CUSTOM_MORE_APPS, KEYWORD_WATCH_AD_BUTTON,  KEYWORD_BRANCH_START_END };

        public AdLocationKeyword keyword;
        public int location_id;
        public int campaign_id;
        public int ad_id;
        public double fill_rate;
        public int priority;
        public AdData ad;

        public static bool IsLocationForced(string _location)
        {
            for (int i = 0; i < forced_locations.Length; i++)
            {
                if (forced_locations[i].getKeyword().Equals(_location))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsLocationSoft(string _location)
        {
            for (int i = 0; i < soft_locations.Length; i++)
            {
                if (soft_locations[i].getKeyword().Equals(_location))
                {
                    return true;
                }
            }
            return false;
        }
    }

    public class AdLocationKeyword
    {
        private string keyword {get;}

        public AdLocationKeyword(string _keyword)
        {
            keyword = _keyword;
        }

        public string getKeyword()
        {
            return keyword;
        }

    }
}
