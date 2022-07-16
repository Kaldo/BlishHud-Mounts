//using Blish_HUD;
//using Blish_HUD.Modules.Managers;
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
//    public class Radial {

//        private static readonly Logger Logger = Logger.GetLogger<Module>();

//        #region Service Managers
//        internal SettingsManager SettingsManager => this.ModuleParameters.SettingsManager;
//        internal ContentsManager ContentsManager => this.ModuleParameters.ContentsManager;
//        internal DirectoriesManager DirectoriesManager => this.ModuleParameters.DirectoriesManager;
//        internal Gw2ApiManager Gw2ApiManager => this.ModuleParameters.Gw2ApiManager;
//        #endregion

//        internal static Collection<Mount> _mounts;
//        internal static List<Mount> _availableOrderedMounts => _mounts.Where(m => m.IsAvailable).OrderBy(m => m.OrderSetting.Value).ToList();

//        public static int[] _mountOrder = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
//        public static string[] _mountDisplay = new string[] { "Transparent", "Solid", "SolidText" };
//        public static string[] _mountBehaviour = new string[] { "DefaultMount", "Radial" };
//        public static string[] _mountOrientation = new string[] { "Horizontal", "Vertical" };
//        public static string[] _mountRadialCenterMountBehavior = new string[] { "None", "Default", "LastUsed" };

//        private static RadialSettings _radialSettings;

//        public static SettingEntry<string> _settingDefaultMountChoice;
//        public static SettingEntry<string> _settingDefaultWaterMountChoice;
//        public static SettingEntry<KeyBinding> _settingDefaultMountBinding;
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

//#pragma warning disable CS0618 // Type or member is obsolete
//        private WindowTab windowTab;
//#pragma warning restore CS0618 // Type or member is obsolete
//        private Panel _mountPanel;
//        private DrawRadial _radial;
//        private LoadingSpinner _queueingSpinner;
//        private Helper _helper;
//        private TextureCache _textureCache;

//        private bool _dragging;
//        private Point _dragStart = Point.Zero;

//        protected void Update(GameTime gameTime)
//        {
//            if (_mountPanel != null)
//            {
//                if (GameService.GameIntegration.Gw2Instance.IsInGame && !GameService.Gw2Mumble.UI.IsMapOpen)
//                {
//                    _mountPanel.Show();
//                }
//                else
//                {
//                    _mountPanel.Hide();
//                }
//                if (_dragging)
//                {
//                    var nOffset = InputService.Input.Mouse.Position - _dragStart;
//                    _mountPanel.Location += nOffset;

//                    _dragStart = InputService.Input.Mouse.Position;
//                }
//            }

//            if (_settingDisplayMountQueueing.Value && _mounts.Any(m => m.QueuedTimestamp != null))
//            {
//                _queueingSpinner?.Show();
//            }

//            if (_radial.Visible && !_settingDefaultMountBinding.Value.IsTriggering)
//            {
//                _radial.Hide();
//            }
//        }

//        private void DrawUI()
//        {
//            _mountPanel?.Dispose();
//            foreach (Mount mount in _mounts)
//            {
//                mount.DisposeCornerIcon();
//            }

//            if (_settingDisplayCornerIcons.Value)
//                DrawCornerIcons();
//            if (_settingDisplayManualIcons.Value)
//                DrawManualIcons();

//            _queueingSpinner?.Dispose();
//            _queueingSpinner = new LoadingSpinner();
//            _queueingSpinner.Location = new Point(GameService.Graphics.SpriteScreen.Width / 2 + 400, GameService.Graphics.SpriteScreen.Height - _queueingSpinner.Height - 25);
//            _queueingSpinner.Parent = GameService.Graphics.SpriteScreen;
//            _queueingSpinner.Hide();

//            _radial?.Dispose();
//            _radial = new DrawRadial(_helper, _textureCache);
//            _radial.Parent = GameService.Graphics.SpriteScreen;
//        }

//        private void UpdateSettings(object sender = null, ValueChangedEventArgs<string> e = null)
//        {
//            DrawUI();
//        }
//        private void UpdateSettings(object sender = null, ValueChangedEventArgs<KeyBinding> e = null)
//        {
//            DrawUI();
//        }
//        private void UpdateSettings(object sender = null, ValueChangedEventArgs<Point> e = null)
//        {
//            DrawUI();
//        }
//        private void UpdateSettings(object sender = null, ValueChangedEventArgs<bool> e = null)
//        {
//            DrawUI();
//        }
//        private void UpdateSettings(object sender = null, ValueChangedEventArgs<float> e = null)
//        {
//            DrawUI();
//        }
//        private void UpdateSettings(object sender = null, ValueChangedEventArgs<int> e = null)
//        {
//            DrawUI();
//        }

//        protected void Unload()
//        {
//            _mountPanel?.Dispose();
//            _radial?.Dispose();

//            foreach (Mount m in _mounts)
//            {
//                m.OrderSetting.SettingChanged -= UpdateSettings;
//                m.KeybindingSetting.SettingChanged -= UpdateSettings;
//                m.DisposeCornerIcon();
//            }

//            _settingDefaultMountChoice.SettingChanged -= UpdateSettings;
//            _settingDefaultWaterMountChoice.SettingChanged -= UpdateSettings;
//            _settingDisplayMountQueueing.SettingChanged -= UpdateSettings;
//            _settingMountRadialSpawnAtMouse.SettingChanged += UpdateSettings;
//            _settingMountRadialIconSizeModifier.SettingChanged -= UpdateSettings;
//            _settingMountRadialRadiusModifier.SettingChanged -= UpdateSettings;
//            _settingMountRadialStartAngle.SettingChanged -= UpdateSettings;
//            _settingMountRadialCenterMountBehavior.SettingChanged -= UpdateSettings;
//            _settingMountRadialIconOpacity.SettingChanged -= UpdateSettings;
//            _settingMountRadialRemoveCenterMount.SettingChanged -= UpdateSettings;

//            _settingDisplay.SettingChanged -= UpdateSettings;
//            _settingDisplayCornerIcons.SettingChanged -= UpdateSettings;
//            _settingDisplayManualIcons.SettingChanged -= UpdateSettings;
//            _settingOrientation.SettingChanged -= UpdateSettings;
//            _settingLoc.SettingChanged -= UpdateSettings;
//            _settingDrag.SettingChanged -= UpdateSettings;
//            _settingImgWidth.SettingChanged -= UpdateSettings;
//            _settingOpacity.SettingChanged -= UpdateSettings;

//            GameService.Overlay.BlishHudWindow.RemoveTab(windowTab);
//        }

//    }
//}
