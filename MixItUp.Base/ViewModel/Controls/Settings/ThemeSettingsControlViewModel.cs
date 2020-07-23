﻿using MixItUp.Base.ViewModel.Controls.Settings.Generic;
using MixItUp.Base.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace MixItUp.Base.ViewModel.Controls.Settings
{
    public class ThemeViewModel : UIViewModelBase
    {
        public string Key { get; set; }

        public string Name { get; set; }

        public ThemeViewModel(string key, string name)
        {
            this.Key = key;
            this.Name = name;
        }

        public override string ToString() { return this.Name; }
    }

    public class ThemeSettingsControlViewModel : UIViewModelBase
    {
        public List<string> AvailableBackgroundColors { get; set; } = new List<string>() { "Light", "Dark" };

        public Dictionary<string, string> FullThemes { get; set; } = new Dictionary<string, string>() { { string.Empty, MixItUp.Base.Resources.None }, { "1YearAnniversary", "1 Year Anniversary" },
            { "Mixer", "Mixer" }, { "Twitch", "Twitch" }, { "Atl3msPlexify", "Atl3m's Plexify" }, { "AwkwardTysonAmericana", "AwkwardTyson - Americana" },
            { "AzhtralsCosmicFire", "Azhtral's Cosmic Fire" }, { "BlueLeprechaunTV", "BlueLeprechaunTV" }, { "DustysPurplePotion", "Dusty's Purple Potion" }, { "InsertCoinTheater", "Insert Coin Theater" },
            { "NibblesCarrotPatch", "Nibbles' Carrot Patch" }, { "StarkContrast", "Stark Contrast" }, { "TacosAfterDark", "Tacos After Dark" }, { "TeamBoom", "Team Boom" } };

        public GenericColorComboBoxSettingsOptionControlViewModel ColorScheme { get; set; }
        public GenericComboBoxSettingsOptionControlViewModel<string> BackgroundColor { get; set; }

        public GenericComboBoxSettingsOptionControlViewModel<ThemeViewModel> FullTheme { get; set; }

        public ThemeSettingsControlViewModel()
        {
            this.ColorScheme = new GenericColorComboBoxSettingsOptionControlViewModel(MixItUp.Base.Resources.ColorScheme, ChannelSession.AppSettings.ColorScheme,
                (value) =>
                {
                    if (value != null)
                    {
                        if (!value.Name.Equals(ChannelSession.AppSettings.ColorScheme))
                        {
                            ChannelSession.AppSettings.SettingsChangeRestartRequired = true;
                        }
                        ChannelSession.AppSettings.ColorScheme = value.Name;
                    }
                });

            this.BackgroundColor = new GenericComboBoxSettingsOptionControlViewModel<string>(MixItUp.Base.Resources.BackgroundColor, AvailableBackgroundColors, ChannelSession.AppSettings.BackgroundColor,
                (value) =>
                {
                    if (!value.Equals(ChannelSession.AppSettings.BackgroundColor))
                    {
                        ChannelSession.AppSettings.SettingsChangeRestartRequired = true;
                    }
                    ChannelSession.AppSettings.BackgroundColor = value;
                });

            List<ThemeViewModel> themes = new List<ThemeViewModel>();
            foreach (var kvp in this.FullThemes)
            {
                themes.Add(new ThemeViewModel(kvp.Key, kvp.Value));
            }
            this.FullTheme = new GenericComboBoxSettingsOptionControlViewModel<ThemeViewModel>(MixItUp.Base.Resources.FullTheme, themes, themes.FirstOrDefault(t => t.Key.Equals(ChannelSession.AppSettings.FullThemeName)),
                (value) =>
                {
                    if (value != null)
                    {
                        if (!value.Key.Equals(ChannelSession.AppSettings.FullThemeName))
                        {
                            ChannelSession.AppSettings.SettingsChangeRestartRequired = true;
                        }
                        ChannelSession.AppSettings.FullThemeName = value.Key;
                    }
                });
        }
    }
}
