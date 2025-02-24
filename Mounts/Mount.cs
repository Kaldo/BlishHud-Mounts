﻿using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Blish_HUD.Settings;
using Gw2Sharp.Models;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Threading.Tasks;

namespace Manlaan.Mounts {
    public abstract class Mount {
        protected readonly Helper _helper;

        public enum RadialCategoryEnum {
            Primary = 1,
            Secondary = 2,
            Tertiary = 3
        }

        public Mount(SettingCollection settingCollection, Helper helper,
            string name, string displayName, string imageFileName,
            bool isUnderwaterMount, bool isWvWMount, int defaultOrderSetting)
        {
            _helper = helper;
            Name = name;
            DisplayName = displayName;
            ImageFileName = imageFileName;
            IsWaterMount = isUnderwaterMount;
            IsWvWMount = isWvWMount;
            OrderSetting = settingCollection.DefineSetting($"Mount{name}Order2", defaultOrderSetting, () => $"{displayName} Order", () => "");
            RadialCategory = settingCollection.DefineSetting($"Mount{name}RadialCategoryBinding", (int)RadialCategoryEnum.Primary, () => $"{displayName} Binding", () => "");
            KeybindingSetting = settingCollection.DefineSetting($"Mount{name}Binding", new KeyBinding(Keys.None), () => $"{displayName} Binding", () => "");
        }

        public string Name { get; private set; }
        public string DisplayName { get; private set; }
        public string ImageFileName { get; private set; }
        public DateTime? QueuedTimestamp { get; internal set; }
        public DateTime? LastUsedTimestamp { get; internal set; }
        public bool IsWaterMount { get; private set; }
        public bool IsWvWMount { get; private set; }


        public SettingEntry<int> OrderSetting { get; private set; }
        public SettingEntry<int> RadialCategory { get; set; }
        public SettingEntry<KeyBinding> KeybindingSetting { get; private set; }
        public CornerIcon CornerIcon { get; private set; }
        public bool IsAvailable => OrderSetting.Value != 0 && IsKeybindSet;
        public bool IsKeybindSet => KeybindingSetting.Value.ModifierKeys != ModifierKeys.None || KeybindingSetting.Value.PrimaryKey != Keys.None;

        public async Task DoUnmountAction()
        {
            await _helper.TriggerKeybind(KeybindingSetting);
        }

        public virtual async Task DoMountAction()
        {
            if (GameService.Gw2Mumble.PlayerCharacter.IsInCombat)
            {
                QueuedTimestamp = DateTime.UtcNow;
                return;
            }

            if (GameService.Gw2Mumble.PlayerCharacter.CurrentMount == MountType.None)
            {
                LastUsedTimestamp = DateTime.UtcNow;
            }

            await _helper.TriggerKeybind(KeybindingSetting);
        }

        public void CreateCornerIcon(Texture2D img)
        {
            CornerIcon?.Dispose();
            CornerIcon = new CornerIcon()
            {
                IconName = DisplayName,
                Icon = img,
                HoverIcon = img,
                Priority = 10
            };
            CornerIcon.Click += async delegate { await DoMountAction(); };
        }

        public void DisposeCornerIcon()
        {
            CornerIcon?.Dispose();
        }
    }

    public class Raptor : Mount {
        public Raptor(SettingCollection settingCollection, Helper helper) : base(settingCollection, helper, "Raptor", "Raptor", "raptor", false, false, 1)
        {
        }
    }

    public class Springer : Mount {
        public Springer(SettingCollection settingCollection, Helper helper) : base(settingCollection, helper, "Springer", "Springer", "springer", false, false, 2)
        {
        }
    }

    public class Skimmer : Mount {
        public Skimmer(SettingCollection settingCollection, Helper helper) : base(settingCollection, helper, "Skimmer", "Skimmer", "skimmer", true, false, 3)
        {
        }
    }

    public class Jackal : Mount {
        public Jackal(SettingCollection settingCollection, Helper helper) : base(settingCollection, helper, "Jackal", "Jackal", "jackal", false, false, 4)
        {
        }
    }

    public class Griffon : Mount {
        public Griffon(SettingCollection settingCollection, Helper helper) : base(settingCollection, helper, "Griffon", "Griffon", "griffon", false, false, 5)
        {
        }
    }

    public class RollerBeetle : Mount {
        public RollerBeetle(SettingCollection settingCollection, Helper helper) : base(settingCollection, helper, "Roller", "Roller Beetle", "roller", false, false, 6)
        {
        }
    }

    public class Warclaw : Mount {
        public Warclaw(SettingCollection settingCollection, Helper helper) : base(settingCollection, helper, "Warclaw", "Warclaw", "warclaw", false, true, 7)
        {
        }
    }

    public class Skyscale : Mount {
        public Skyscale(SettingCollection settingCollection, Helper helper) : base(settingCollection, helper, "Skyscale", "Skyscale", "skyscale", false, false, 8)
        {
        }
    }

    public class SiegeTurtle : Mount {
        public SiegeTurtle(SettingCollection settingCollection, Helper helper) : base(settingCollection, helper, "Turtle", "Siege Turtle", "turtle", true, false, 9)
        {
        }
    }

    public class FishingRod : Mount {
        public FishingRod(SettingCollection settingCollection, Helper helper) : base(settingCollection, helper, "FishingRod", "Fishing Rod", "fishingRod", false, false, 11) { }
    }

    public class Skiff : Mount {
        public Skiff(SettingCollection settingCollection, Helper helper) : base(settingCollection, helper, "Skiff", "Skiff", "skiff", false, false, 10) { }

        public override Task DoMountAction()
        {
            return base.DoMountAction();

            //if (GameService.Gw2Mumble.PlayerCharacter.IsInCombat)
            //{
            //    QueuedTimestamp = DateTime.UtcNow;
            //    return;
            //}

            //if (GameService.Gw2Mumble.PlayerCharacter.CurrentMount == MountType.None)
            //{
            //    LastUsedTimestamp = DateTime.UtcNow;
            //}

            //await _helper.TriggerKeybind(KeybindingSetting);
        }

    }

    public class PersonalWaypoint : Mount {
        public PersonalWaypoint(SettingCollection settingCollection, Helper helper) : base(settingCollection, helper, "PersonalWaypoint", "Personal Waypoint", "personalWaypoint", false, false, 12) { }
    }

    public class Chair : Mount {
        public Chair(SettingCollection settingCollection, Helper helper) : base(settingCollection, helper, "Chair", "Chair", "chair", false, false, 13) { }
    }

    public class MusicalInstrument : Mount {
        public MusicalInstrument(SettingCollection settingCollection, Helper helper) : base(settingCollection, helper, "MusicalInstrument", "Musical Instrument", "musicalInstrument", false, false, 14) { }
    }

    public class HeldItem : Mount {
        public HeldItem(SettingCollection settingCollection, Helper helper) : base(settingCollection, helper, "HeldItem", "Held Item", "heldItem", false, false, 15) { }
    }

    public class Toy : Mount {
        public Toy(SettingCollection settingCollection, Helper helper) : base(settingCollection, helper, "Toy", "Toy", "toy", false, false, 16) { }
    }
    public class Tonic : Mount {
        public Tonic(SettingCollection settingCollection, Helper helper) : base(settingCollection, helper, "Tonic", "Tonic", "tonic", false, false, 17) { }
    }

}
