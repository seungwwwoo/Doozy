// Copyright (c) 2015 - 2023 Doozy Entertainment. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

//.........................
//.....Generated Class.....
//.........................
//.......Do not edit.......
//.........................

using System.Collections.Generic;
// ReSharper disable All
namespace Doozy.Runtime.UIManager.Containers
{
    public partial class UIView
    {
        public static IEnumerable<UIView> GetViews(UIViewId.Common id) => GetViews(nameof(UIViewId.Common), id.ToString());
        public static void Show(UIViewId.Common id, bool instant = false) => Show(nameof(UIViewId.Common), id.ToString(), instant);
        public static void Hide(UIViewId.Common id, bool instant = false) => Hide(nameof(UIViewId.Common), id.ToString(), instant);

        public static IEnumerable<UIView> GetViews(UIViewId.Game id) => GetViews(nameof(UIViewId.Game), id.ToString());
        public static void Show(UIViewId.Game id, bool instant = false) => Show(nameof(UIViewId.Game), id.ToString(), instant);
        public static void Hide(UIViewId.Game id, bool instant = false) => Hide(nameof(UIViewId.Game), id.ToString(), instant);

        public static IEnumerable<UIView> GetViews(UIViewId.IFYOU id) => GetViews(nameof(UIViewId.IFYOU), id.ToString());
        public static void Show(UIViewId.IFYOU id, bool instant = false) => Show(nameof(UIViewId.IFYOU), id.ToString(), instant);
        public static void Hide(UIViewId.IFYOU id, bool instant = false) => Hide(nameof(UIViewId.IFYOU), id.ToString(), instant);
    }
}

namespace Doozy.Runtime.UIManager
{
    public partial class UIViewId
    {
        public enum Common
        {
            EnterLoading,
            EpisodeStart,
            GameSceneLoading,
            LiveRenderer,
            LobbyLoading,
            Mail,
            MainFake,
            ProjectList,
            Setting,
            StarShop,
            SystemLoading,
            TEST,
            Top
        }

        public enum Game
        {
            EpisodeEnd,
            InGame,
            InGameLoading
        }

        public enum IFYOU
        {
            Ability,
            Adventure,
            Beginning,
            Costume,
            Custom,
            Data,
            DLC,
            DLCDetail,
            DLCMemory,
            Ending,
            EndingSelection,
            Entrance,
            EpisodeList,
            Gallery,
            IllustDetail,
            Intro,
            Introduce,
            ItemList,
            Language,
            Lobby,
            Main,
            Memory,
            Mission,
            Navigation,
            Search,
            Shop,
            SoundDetail,
            SpecialEpisode,
            StoryLoading,
            StoryLobby,
            Talk,
            Timer,
            Title
        }    
    }
}
