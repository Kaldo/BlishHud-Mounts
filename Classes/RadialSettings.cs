//using Blish_HUD;
//using Blish_HUD.Modules;
//using Blish_HUD.Modules.Managers;
//using Blish_HUD.Settings;
//using Blish_HUD.Input;
//using Blish_HUD.Controls;
//using Blish_HUD.Graphics.UI;

//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Input;
//using Microsoft.Xna.Framework.Graphics;
//using System;
//using System.ComponentModel.Composition;
//using System.Threading.Tasks;
//using System.Linq;
//using System.Collections.ObjectModel;
//using Manlaan.Mounts.Controls;
//using System.Collections.Generic;
//using Gw2Sharp.Models;

//namespace Manlaan.Mounts.Classes {
//    public class RadialSettings {

//        internal static Collection<Mount> _mounts;
//        internal static List<Mount> _availableOrderedMounts => _mounts.Where(m => m.IsAvailable).OrderBy(m => m.OrderSetting.Value).ToList();

//        public static int[] _mountOrder = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
//        public static string[] _mountDisplay = new string[] { "Transparent", "Solid", "SolidText" };
//        public static string[] _mountBehaviour = new string[] { "DefaultMount", "Radial" };
//        public static string[] _mountOrientation = new string[] { "Horizontal", "Vertical" };
//        public static string[] _mountRadialCenterMountBehavior = new string[] { "None", "Default", "LastUsed" };

//        public static SettingEntry<string> _settingDefaultMountChoice;
//        public static SettingEntry<string> _settingDefaultWaterMountChoice;
//        public static SettingEntry<KeyBinding> _settingDefaultMountBinding;
//        //public static SettingEntry<KeyBinding> _settingDefaultMountBinding;
//        public static SettingEntry<bool> _settingDisplayMountQueueing;
//        public static SettingEntry<string> _settingDefaultMountBehaviour;
//        public static SettingEntry<bool> _settingMountRadialSpawnAtMouse;
//        public static SettingEntry<float> _settingMountRadialRadiusModifier;
//        public static SettingEntry<float> _settingMountRadialStartAngle;
//        public static SettingEntry<float> _settingMountRadialIconSizeModifier;
//        public static SettingEntry<float> _settingMountRadialIconOpacity;
//        public static SettingEntry<string> _settingMountRadialCenterMountBehavior;
//        public static SettingEntry<bool> _settingMountRadialRemoveCenterMount;
//        public static SettingEntry<KeyBinding> _settingMountRadialToggleActionCameraKeyBinding;

//        public static SettingEntry<string> _settingDisplay;
//        public static SettingEntry<bool> _settingDisplayCornerIcons;
//        public static SettingEntry<bool> _settingDisplayManualIcons;
//        public static SettingEntry<string> _settingOrientation;
//        private SettingEntry<Point> _settingLoc;
//        public static SettingEntry<bool> _settingDrag;
//        public static SettingEntry<int> _settingImgWidth;
//        public static SettingEntry<float> _settingOpacity;

//        public void DefineSettings(SettingCollection settings, Helper _helper)
//        {
//            _mounts = new Collection<Mount>
//            {
//                new Raptor(settings, _helper),
//                new Springer(settings, _helper),
//                new Skimmer(settings, _helper),
//                new Jackal(settings, _helper),
//                new Griffon(settings, _helper),
//                new RollerBeetle(settings, _helper),
//                new Warclaw(settings, _helper),
//                new Skyscale(settings, _helper),
//                new SiegeTurtle(settings, _helper),
//                new Skiff(settings, _helper),
//                new FishingRod(settings, _helper),
//                new PersonalWaypoint(settings, _helper)
//            };

//            _settingDefaultMountBinding = settings.DefineSetting("DefaultMountBinding", new KeyBinding(Keys.None), () => Strings.Setting_DefaultMountBinding, () => "");
//            _settingDefaultMountBinding.Value.Enabled = true;
//            _settingDefaultMountBinding.Value.Activated += async delegate { await DoDefaultMountActionAsync(); };
//            _settingDefaultMountChoice = settings.DefineSetting("DefaultMountChoice", "Disabled", () => Strings.Setting_DefaultMountChoice, () => "");
//            _settingDefaultWaterMountChoice = settings.DefineSetting("DefaultWaterMountChoice", "Disabled", () => Strings.Setting_DefaultWaterMountChoice, () => "");
//            _settingDefaultMountBehaviour = settings.DefineSetting("DefaultMountBehaviour", "Radial", () => Strings.Setting_DefaultMountBehaviour, () => "");
//            _settingDisplayMountQueueing = settings.DefineSetting("DisplayMountQueueing", false, () => Strings.Setting_DisplayMountQueueing, () => "");
//            _settingMountRadialSpawnAtMouse = settings.DefineSetting("MountRadialSpawnAtMouse", false, () => Strings.Setting_MountRadialSpawnAtMouse, () => "");
//            _settingMountRadialIconSizeModifier = settings.DefineSetting("MountRadialIconSizeModifier", 0.5f, () => Strings.Setting_MountRadialIconSizeModifier, () => "");
//            _settingMountRadialIconSizeModifier.SetRange(0.05f, 1f);
//            _settingMountRadialRadiusModifier = settings.DefineSetting("MountRadialRadiusModifier", 0.5f, () => Strings.Setting_MountRadialRadiusModifier, () => "");
//            _settingMountRadialRadiusModifier.SetRange(0.2f, 1f);
//            _settingMountRadialStartAngle = settings.DefineSetting("MountRadialStartAngle", 0.0f, () => Strings.Setting_MountRadialStartAngle, () => "");
//            _settingMountRadialStartAngle.SetRange(0.0f, 1.0f);
//            _settingMountRadialIconOpacity = settings.DefineSetting("MountRadialIconOpacity", 0.5f, () => Strings.Setting_MountRadialIconOpacity, () => "");
//            _settingMountRadialIconOpacity.SetRange(0.05f, 1f);
//            _settingMountRadialCenterMountBehavior = settings.DefineSetting("MountRadialCenterMountBehavior", "Default", () => Strings.Setting_MountRadialCenterMountBehavior, () => "");
//            _settingMountRadialRemoveCenterMount = settings.DefineSetting("MountRadialRemoveCenterMount", true, () => Strings.Setting_MountRadialRemoveCenterMount, () => "");
//            _settingMountRadialToggleActionCameraKeyBinding = settings.DefineSetting("MountRadialToggleActionCameraKeyBinding", new KeyBinding(Keys.F10), () => Strings.Setting_MountRadialToggleActionCameraKeyBinding, () => "");

//            _settingDisplay = settings.DefineSetting("MountDisplay", "Transparent", () => Strings.Setting_MountDisplay, () => "");
//            _settingDisplayCornerIcons = settings.DefineSetting("MountDisplayCornerIcons", false, () => Strings.Setting_MountDisplayCornerIcons, () => "");
//            _settingDisplayManualIcons = settings.DefineSetting("MountDisplayManualIcons", false, () => Strings.Setting_MountDisplayManualIcons, () => "");
//            _settingOrientation = settings.DefineSetting("Orientation", "Horizontal", () => Strings.Setting_Orientation, () => "");
//            _settingLoc = settings.DefineSetting("MountLoc", new Point(100, 100), () => Strings.Setting_MountLoc, () => "");
//            _settingDrag = settings.DefineSetting("MountDrag", false, () => Strings.Setting_MountDrag, () => "");
//            _settingImgWidth = settings.DefineSetting("MountImgWidth", 50, () => Strings.Setting_MountImgWidth, () => "");
//            _settingImgWidth.SetRange(0, 200);
//            _settingOpacity = settings.DefineSetting("MountOpacity", 1.0f, () => Strings.Setting_MountOpacity, () => "");
//            _settingOpacity.SetRange(0f, 1f);
//        }

//        private async Task DoDefaultMountActionAsync()
//        {

//        }

//    }
//}
