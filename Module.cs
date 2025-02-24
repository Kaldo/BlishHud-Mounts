﻿using Blish_HUD;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Blish_HUD.Input;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.ObjectModel;
using Manlaan.Mounts.Controls;
using System.Collections.Generic;
using Gw2Sharp.Models;

namespace Manlaan.Mounts {
    [Export(typeof(Blish_HUD.Modules.Module))]
    public class Module : Blish_HUD.Modules.Module {
        private static readonly Logger Logger = Logger.GetLogger<Module>();

        #region Service Managers
        internal SettingsManager SettingsManager => this.ModuleParameters.SettingsManager;
        internal ContentsManager ContentsManager => this.ModuleParameters.ContentsManager;
        internal DirectoriesManager DirectoriesManager => this.ModuleParameters.DirectoriesManager;
        internal Gw2ApiManager Gw2ApiManager => this.ModuleParameters.Gw2ApiManager;
        #endregion

        internal static Collection<Mount> _mounts;
        internal static List<Mount> _availableOrderedMounts => _mounts.Where(m => m.IsAvailable && m.RadialCategory.Value == (int)Mount.RadialCategoryEnum.Primary).OrderBy(m => m.OrderSetting.Value).ToList();
        internal static List<Mount> _availableOrderedSecondaryMounts => _mounts.Where(m => m.IsAvailable && m.RadialCategory.Value == (int)Mount.RadialCategoryEnum.Secondary).OrderBy(m => m.OrderSetting.Value).ToList();
        internal static List<Mount> _availableOrderedTertiaryMounts => _mounts.Where(m => m.IsAvailable && m.RadialCategory.Value == (int)Mount.RadialCategoryEnum.Tertiary).OrderBy(m => m.OrderSetting.Value).ToList();

        public static int[] _mountOrder = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 };
        public static string[] _mountDisplay = new string[] { "Transparent", "Solid", "SolidText" };
        public static string[] _mountBehaviour = new string[] { "DefaultMount", "Radial" };
        public static string[] _mountOrientation = new string[] { "Horizontal", "Vertical" };
        public static string[] _mountRadialCenterMountBehavior = new string[] { "None", "Default", "LastUsed" };

        public static SettingEntry<string> _settingDefaultMountChoice;
        public static SettingEntry<string> _settingDefaultWaterMountChoice;
        public static SettingEntry<KeyBinding> _settingDefaultMountBinding;
        public static SettingEntry<KeyBinding> _settingDefaultSecondaryRadialBinding;
        public static SettingEntry<KeyBinding> _settingDefaultTertiaryRadialBinding;
        public static SettingEntry<bool> _settingDisplayMountQueueing;
        public static SettingEntry<string> _settingDefaultMountBehaviour;
        public static SettingEntry<bool> _settingMountRadialSpawnAtMouse;
        public static SettingEntry<float> _settingMountRadialRadiusModifier;
        public static SettingEntry<float> _settingMountRadialStartAngle;
        public static SettingEntry<float> _settingMountRadialIconSizeModifier;
        public static SettingEntry<float> _settingMountRadialIconOpacity;
        public static SettingEntry<string> _settingMountRadialCenterMountBehavior;
        public static SettingEntry<bool> _settingMountRadialRemoveCenterMount;
        public static SettingEntry<KeyBinding> _settingMountRadialToggleActionCameraKeyBinding;

        public static SettingEntry<string> _settingDisplay;
        public static SettingEntry<bool> _settingDisplayCornerIcons;
        public static SettingEntry<bool> _settingDisplayManualIcons;
        public static SettingEntry<string> _settingOrientation;
        private SettingEntry<Point> _settingLoc;
        public static SettingEntry<bool> _settingDrag;
        public static SettingEntry<int> _settingImgWidth;
        public static SettingEntry<float> _settingOpacity;

#pragma warning disable CS0618 // Type or member is obsolete
        private WindowTab windowTab;
#pragma warning restore CS0618 // Type or member is obsolete
        private Panel _mountPanel;
        private DrawRadial _radial;
        private DrawRadial _secondaryRadial;
        private DrawRadial _tertiaryRadial;
        private LoadingSpinner _queueingSpinner;
        private Helper _helper;
        private TextureCache _textureCache;

        private bool _dragging;
        private Point _dragStart = Point.Zero;

        [ImportingConstructor]
        public Module([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters)
        {
            _helper = new Helper();
        }

        protected override void Initialize()
        {
            GameService.Gw2Mumble.PlayerCharacter.IsInCombatChanged += async (sender, e) => await HandleCombatChangeAsync(sender, e);

#pragma warning disable CS0618 // Type or member is obsolete
            windowTab = new WindowTab("Mounts", ContentsManager.GetTexture("514394-grey.png"));
#pragma warning restore CS0618 // Type or member is obsolete
        }


        /*
         * Migrate from seperate settings from MountDisplay
         * MountDisplay => "Transparent", "Solid", "SolidText"
         *
         */
        private void MigrateDisplaySettings()
        {
            if (_settingDisplay.Value.Contains("Corner") || _settingDisplay.Value.Contains("Manual"))
            {
                _settingDisplayCornerIcons.Value = _settingDisplay.Value.Contains("Corner");
                _settingDisplayManualIcons.Value = _settingDisplay.Value.Contains("Solid");

                if (_settingDisplay.Value.Contains("Text"))
                {
                    _settingDisplay.Value = "SolidText";
                }
                else if (_settingDisplay.Value.Contains("Solid"))
                {
                    _settingDisplay.Value = "Solid";
                }
                else if (_settingDisplay.Value.Contains("Transparent"))
                {
                    _settingDisplay.Value = "Transparent";
                }
            }
        }

        protected override void DefineSettings(SettingCollection settings)
        {
            _mounts = new Collection<Mount>
            {
                new Raptor(settings, _helper),
                new Springer(settings, _helper),
                new Skimmer(settings, _helper),
                new Jackal(settings, _helper),
                new Griffon(settings, _helper),
                new RollerBeetle(settings, _helper),
                new Warclaw(settings, _helper),
                new Skyscale(settings, _helper),
                new SiegeTurtle(settings, _helper),
                new FishingRod(settings, _helper),
                new Skiff(settings, _helper),
                new PersonalWaypoint(settings, _helper),
                new Chair(settings, _helper),
                new MusicalInstrument(settings, _helper),
                new HeldItem(settings, _helper),
                new Toy(settings, _helper),
                new Tonic(settings, _helper)
            };

            _settingDefaultMountBinding = settings.DefineSetting("DefaultMountBinding", new KeyBinding(Keys.None), () => Strings.Setting_DefaultMountBinding, () => "");
            _settingDefaultMountBinding.Value.Enabled = true;
            _settingDefaultMountBinding.Value.Activated += async delegate { await DoDefaultMountActionAsync(Mount.RadialCategoryEnum.Primary); };

            _settingDefaultSecondaryRadialBinding = settings.DefineSetting("DefaultSecondaryRadialBinding", new KeyBinding(Keys.None), () => Strings.Setting_DefaultSecondaryRadialBinding, () => "");
            _settingDefaultSecondaryRadialBinding.Value.Enabled = true;
            _settingDefaultSecondaryRadialBinding.Value.Activated += async delegate { await DoDefaultMountActionAsync(Mount.RadialCategoryEnum.Secondary); };

            _settingDefaultTertiaryRadialBinding = settings.DefineSetting("DefaultTertiaryRadialBinding", new KeyBinding(Keys.None), () => Strings.Setting_DefaultTertiaryRadialBinding, () => "");
            _settingDefaultTertiaryRadialBinding.Value.Enabled = true;
            _settingDefaultTertiaryRadialBinding.Value.Activated += async delegate { await DoDefaultMountActionAsync(Mount.RadialCategoryEnum.Tertiary); };

            _settingDefaultMountChoice = settings.DefineSetting("DefaultMountChoice", "Disabled", () => Strings.Setting_DefaultMountChoice, () => "");
            _settingDefaultWaterMountChoice = settings.DefineSetting("DefaultWaterMountChoice", "Disabled", () => Strings.Setting_DefaultWaterMountChoice, () => "");
            _settingDefaultMountBehaviour = settings.DefineSetting("DefaultMountBehaviour", "Radial", () => Strings.Setting_DefaultMountBehaviour, () => "");
            _settingDisplayMountQueueing = settings.DefineSetting("DisplayMountQueueing", false, () => Strings.Setting_DisplayMountQueueing, () => "");
            _settingMountRadialSpawnAtMouse = settings.DefineSetting("MountRadialSpawnAtMouse", false, () => Strings.Setting_MountRadialSpawnAtMouse, () => "");
            _settingMountRadialIconSizeModifier = settings.DefineSetting("MountRadialIconSizeModifier", 0.5f, () => Strings.Setting_MountRadialIconSizeModifier, () => "");
            _settingMountRadialIconSizeModifier.SetRange(0.05f, 1f);
            _settingMountRadialRadiusModifier = settings.DefineSetting("MountRadialRadiusModifier", 0.5f, () => Strings.Setting_MountRadialRadiusModifier, () => "");
            _settingMountRadialRadiusModifier.SetRange(0.2f, 1f);
            _settingMountRadialStartAngle = settings.DefineSetting("MountRadialStartAngle", 0.0f, () => Strings.Setting_MountRadialStartAngle, () => "");
            _settingMountRadialStartAngle.SetRange(0.0f, 1.0f);
            _settingMountRadialIconOpacity = settings.DefineSetting("MountRadialIconOpacity", 0.5f, () => Strings.Setting_MountRadialIconOpacity, () => "");
            _settingMountRadialIconOpacity.SetRange(0.05f, 1f);
            _settingMountRadialCenterMountBehavior = settings.DefineSetting("MountRadialCenterMountBehavior", "Default", () => Strings.Setting_MountRadialCenterMountBehavior, () => "");
            _settingMountRadialRemoveCenterMount = settings.DefineSetting("MountRadialRemoveCenterMount", true, () => Strings.Setting_MountRadialRemoveCenterMount, () => "");
            _settingMountRadialToggleActionCameraKeyBinding = settings.DefineSetting("MountRadialToggleActionCameraKeyBinding", new KeyBinding(Keys.F10), () => Strings.Setting_MountRadialToggleActionCameraKeyBinding, () => "");

            _settingDisplay = settings.DefineSetting("MountDisplay", "Transparent", () => Strings.Setting_MountDisplay, () => "");
            _settingDisplayCornerIcons = settings.DefineSetting("MountDisplayCornerIcons", false, () => Strings.Setting_MountDisplayCornerIcons, () => "");
            _settingDisplayManualIcons = settings.DefineSetting("MountDisplayManualIcons", false, () => Strings.Setting_MountDisplayManualIcons, () => "");
            _settingOrientation = settings.DefineSetting("Orientation", "Horizontal", () => Strings.Setting_Orientation, () => "");
            _settingLoc = settings.DefineSetting("MountLoc", new Point(100, 100), () => Strings.Setting_MountLoc, () => "");
            _settingDrag = settings.DefineSetting("MountDrag", false, () => Strings.Setting_MountDrag, () => "");
            _settingImgWidth = settings.DefineSetting("MountImgWidth", 50, () => Strings.Setting_MountImgWidth, () => "");
            _settingImgWidth.SetRange(0, 200);
            _settingOpacity = settings.DefineSetting("MountOpacity", 1.0f, () => Strings.Setting_MountOpacity, () => "");
            _settingOpacity.SetRange(0f, 1f);

            MigrateDisplaySettings();

            foreach (Mount m in _mounts)
            {
                m.OrderSetting.SettingChanged += UpdateSettings;
                m.KeybindingSetting.SettingChanged += UpdateSettings;
            }
            _settingDefaultMountChoice.SettingChanged += UpdateSettings;
            _settingDefaultWaterMountChoice.SettingChanged += UpdateSettings;
            _settingDisplayMountQueueing.SettingChanged += UpdateSettings;
            _settingMountRadialSpawnAtMouse.SettingChanged += UpdateSettings;
            _settingMountRadialIconSizeModifier.SettingChanged += UpdateSettings;
            _settingMountRadialRadiusModifier.SettingChanged += UpdateSettings;
            _settingMountRadialStartAngle.SettingChanged += UpdateSettings;
            _settingMountRadialCenterMountBehavior.SettingChanged += UpdateSettings;
            _settingMountRadialIconOpacity.SettingChanged += UpdateSettings;
            _settingMountRadialRemoveCenterMount.SettingChanged += UpdateSettings;

            _settingDisplay.SettingChanged += UpdateSettings;
            _settingDisplayCornerIcons.SettingChanged += UpdateSettings;
            _settingDisplayManualIcons.SettingChanged += UpdateSettings;
            _settingOrientation.SettingChanged += UpdateSettings;
            _settingLoc.SettingChanged += UpdateSettings;
            _settingDrag.SettingChanged += UpdateSettings;
            _settingImgWidth.SettingChanged += UpdateSettings;
            _settingOpacity.SettingChanged += UpdateSettings;

        }

        public override IView GetSettingsView()
        {
            return new Views.DummySettingsView(ContentsManager);
        }

        protected override void OnModuleLoaded(EventArgs e)
        {
            _textureCache = new TextureCache(ContentsManager);
            DrawUI();
            GameService.Overlay.BlishHudWindow.AddTab(windowTab, () => new Views.SettingsView(ContentsManager));

            // Base handler must be called
            base.OnModuleLoaded(e);
        }

        protected override void Update(GameTime gameTime)
        {
            if (_mountPanel != null)
            {
                if (GameService.GameIntegration.Gw2Instance.IsInGame && !GameService.Gw2Mumble.UI.IsMapOpen)
                {
                    _mountPanel.Show();
                }
                else
                {
                    _mountPanel.Hide();
                }
                if (_dragging)
                {
                    var nOffset = InputService.Input.Mouse.Position - _dragStart;
                    _mountPanel.Location += nOffset;

                    _dragStart = InputService.Input.Mouse.Position;
                }
            }

            if (_settingDisplayMountQueueing.Value && _mounts.Any(m => m.QueuedTimestamp != null))
            {
                _queueingSpinner?.Show();
            }

            if (_radial.Visible && !_settingDefaultMountBinding.Value.IsTriggering)
            {
                _radial.Hide();
            }
            if (_secondaryRadial.Visible && !_settingDefaultSecondaryRadialBinding.Value.IsTriggering)
            {
                _secondaryRadial.Hide();
            }
            if (_tertiaryRadial.Visible && !_settingDefaultTertiaryRadialBinding.Value.IsTriggering)
            {
                _tertiaryRadial.Hide();
            }
        }

        /// <inheritdoc />
        protected override void Unload()
        {
            _mountPanel?.Dispose();
            _radial?.Dispose();
            _secondaryRadial?.Dispose();
            _tertiaryRadial?.Dispose();

            foreach (Mount m in _mounts)
            {
                m.OrderSetting.SettingChanged -= UpdateSettings;
                m.KeybindingSetting.SettingChanged -= UpdateSettings;
                m.DisposeCornerIcon();
            }

            _settingDefaultMountChoice.SettingChanged -= UpdateSettings;
            _settingDefaultWaterMountChoice.SettingChanged -= UpdateSettings;
            _settingDisplayMountQueueing.SettingChanged -= UpdateSettings;
            _settingMountRadialSpawnAtMouse.SettingChanged += UpdateSettings;
            _settingMountRadialIconSizeModifier.SettingChanged -= UpdateSettings;
            _settingMountRadialRadiusModifier.SettingChanged -= UpdateSettings;
            _settingMountRadialStartAngle.SettingChanged -= UpdateSettings;
            _settingMountRadialCenterMountBehavior.SettingChanged -= UpdateSettings;
            _settingMountRadialIconOpacity.SettingChanged -= UpdateSettings;
            _settingMountRadialRemoveCenterMount.SettingChanged -= UpdateSettings;

            _settingDisplay.SettingChanged -= UpdateSettings;
            _settingDisplayCornerIcons.SettingChanged -= UpdateSettings;
            _settingDisplayManualIcons.SettingChanged -= UpdateSettings;
            _settingOrientation.SettingChanged -= UpdateSettings;
            _settingLoc.SettingChanged -= UpdateSettings;
            _settingDrag.SettingChanged -= UpdateSettings;
            _settingImgWidth.SettingChanged -= UpdateSettings;
            _settingOpacity.SettingChanged -= UpdateSettings;

            GameService.Overlay.BlishHudWindow.RemoveTab(windowTab);
        }

        private void UpdateSettings(object sender = null, ValueChangedEventArgs<string> e = null)
        {
            DrawUI();
        }
        private void UpdateSettings(object sender = null, ValueChangedEventArgs<KeyBinding> e = null)
        {
            DrawUI();
        }
        private void UpdateSettings(object sender = null, ValueChangedEventArgs<Point> e = null)
        {
            DrawUI();
        }
        private void UpdateSettings(object sender = null, ValueChangedEventArgs<bool> e = null)
        {
            DrawUI();
        }
        private void UpdateSettings(object sender = null, ValueChangedEventArgs<float> e = null)
        {
            DrawUI();
        }
        private void UpdateSettings(object sender = null, ValueChangedEventArgs<int> e = null)
        {
            DrawUI();
        }

        internal void DrawManualIcons()
        {
            int curX = 0;
            int curY = 0;
            int totalMounts = 0;

            _mountPanel = new Panel()
            {
                Parent = GameService.Graphics.SpriteScreen,
                Location = _settingLoc.Value,
                Size = new Point(_settingImgWidth.Value * 8, _settingImgWidth.Value * 8),
            };

            foreach (Mount mount in _availableOrderedMounts)
            {
                Texture2D img = _textureCache.GetImgFile(mount.ImageFileName);
                Image _btnMount = new Image
                {
                    Parent = _mountPanel,
                    Texture = img,
                    Size = new Point(_settingImgWidth.Value, _settingImgWidth.Value),
                    Location = new Point(curX, curY),
                    Opacity = _settingOpacity.Value,
                    BasicTooltipText = mount.DisplayName
                };
                _btnMount.LeftMouseButtonPressed += async delegate { await mount.DoMountAction(); };

                if (_settingOrientation.Value.Equals("Horizontal"))
                    curX += _settingImgWidth.Value;
                else
                    curY += _settingImgWidth.Value;

                totalMounts++;
            }

            if (_settingDrag.Value)
            {
                Panel dragBox = new Panel()
                {
                    Parent = _mountPanel,
                    Location = new Point(0, 0),
                    Size = new Point(_settingImgWidth.Value / 2, _settingImgWidth.Value / 2),
                    BackgroundColor = Color.White,
                    ZIndex = 10,
                };
                dragBox.LeftMouseButtonPressed += delegate
                {
                    _dragging = true;
                    _dragStart = InputService.Input.Mouse.Position;
                };
                dragBox.LeftMouseButtonReleased += delegate
                {
                    _dragging = false;
                    _settingLoc.Value = _mountPanel.Location;
                };
            }

            if (_settingOrientation.Value.Equals("Horizontal"))
            {
                _mountPanel.Size = new Point(_settingImgWidth.Value * totalMounts, _settingImgWidth.Value);
            }
            else
            {
                _mountPanel.Size = new Point(_settingImgWidth.Value, _settingImgWidth.Value * totalMounts);
            }

        }
        private void DrawCornerIcons()
        {
            foreach (Mount mount in _availableOrderedMounts)
            {
                mount.CreateCornerIcon(_textureCache.GetImgFile(mount.ImageFileName));
            }

        }

        private void DrawUI()
        {
            _mountPanel?.Dispose();
            foreach (Mount mount in _mounts)
            {
                mount.DisposeCornerIcon();
            }

            if (_settingDisplayCornerIcons.Value)
                DrawCornerIcons();
            if (_settingDisplayManualIcons.Value)
                DrawManualIcons();

            _queueingSpinner?.Dispose();
            _queueingSpinner = new LoadingSpinner();
            _queueingSpinner.Location = new Point(GameService.Graphics.SpriteScreen.Width / 2 + 400, GameService.Graphics.SpriteScreen.Height - _queueingSpinner.Height - 25);
            _queueingSpinner.Parent = GameService.Graphics.SpriteScreen;
            _queueingSpinner.Hide();

            _radial?.Dispose();
            _radial = new DrawRadial(_helper, _textureCache, Mount.RadialCategoryEnum.Primary);
            _radial.Parent = GameService.Graphics.SpriteScreen;

            _secondaryRadial?.Dispose();
            _secondaryRadial = new DrawRadial(_helper, _textureCache, Mount.RadialCategoryEnum.Secondary);
            _secondaryRadial.Parent = GameService.Graphics.SpriteScreen;

            _tertiaryRadial?.Dispose();
            _tertiaryRadial = new DrawRadial(_helper, _textureCache, Mount.RadialCategoryEnum.Tertiary);
            _tertiaryRadial.Parent = GameService.Graphics.SpriteScreen;
        }

        private async Task DoDefaultMountActionAsync(Mount.RadialCategoryEnum radialCategory)
        {
            if (_helper.IsKeybindBeingTriggered())
            {
                Logger.Debug("DoDefaultMountActionAsync IsKeybindBeingTriggered");
                return;
            }
            Logger.Debug("DoDefaultMountActionAsync entered");
            if (GameService.Gw2Mumble.PlayerCharacter.CurrentMount != MountType.None)
            {
                await (_availableOrderedMounts.FirstOrDefault()?.DoUnmountAction() ?? Task.CompletedTask);
                Logger.Debug("DoDefaultMountActionAsync dismounted");
                return;
            }

            var instantMount = _helper.GetInstantMount();
            if (instantMount != null)
            {
                await instantMount.DoMountAction();
                Logger.Debug("DoDefaultMountActionAsync instantmount");
                return;
            }

            var defaultMount = _helper.GetDefaultMount();
            if (defaultMount != null && GameService.Input.Mouse.CameraDragging)
            {
                await (defaultMount?.DoMountAction() ?? Task.CompletedTask);
                Logger.Debug("DoDefaultMountActionAsync CameraDragging defaultmount");
                return;
            }

            switch (_settingDefaultMountBehaviour.Value)
            {
                case "DefaultMount":
                    await (defaultMount?.DoMountAction() ?? Task.CompletedTask);
                    Logger.Debug("DoDefaultMountActionAsync DefaultMountBehaviour defaultmount");
                    break;
                case "Radial":
                    switch (radialCategory)
                    {
                        case Mount.RadialCategoryEnum.Secondary:
                            _secondaryRadial.Show();
                            break;
                        case Mount.RadialCategoryEnum.Tertiary:
                            _tertiaryRadial.Show();
                            break;
                        default:
                        case Mount.RadialCategoryEnum.Primary:
                            _radial.Show();
                            break;
                    }
                    
                    Logger.Debug("DoDefaultMountActionAsync DefaultMountBehaviour radial");
                    break;
            }
            return;
        }

        private async Task HandleCombatChangeAsync(object sender, ValueEventArgs<bool> e)
        {
            if (!e.Value)
            {
                await (_mounts.Where(m => m.QueuedTimestamp != null).OrderByDescending(m => m.QueuedTimestamp).FirstOrDefault()?.DoMountAction() ?? Task.CompletedTask);
                foreach (var mount in _mounts)
                {
                    mount.QueuedTimestamp = null;
                }
                _queueingSpinner?.Hide();
            }
        }
    }
}
