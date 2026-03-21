using Umbra.SDK.Config;
using Umbra.SDK.Config.Attributes;
using Umbra.SDK.Config.UI.ParameterDrawers;
using Umbra.SDK.Logging;

namespace Umbra.SamplePlugin.Config;

/// <summary>
/// Root configuration record for the sample plugin.
/// Contains the enable toggle, hotkey bindings, and nested settings groups
/// for field-of-view and film grain adjustments.
/// </summary>
[AutoRegisterSettings]
[ConfigRootNode("Sample Plugin v1.0")]
[SettingsPrefix("samplePlugin")]
public record PluginConfig
{
    /// <summary>Gets or sets whether the Sample Plugin is active.</summary>
    [SettingsParameter]
    [DisplayName("Enabled")]
    [Description("Whether the plugin is enabled or not.")]
    public Parameter<bool> IsEnabled { get; set; } = new(true);

    /// <summary>Gets or sets the hotkey used to toggle the plugin on and off.</summary>
    [SettingsParameter]
    [DisplayName("Toggle Hotkey")]
    [Description("The hotkey to toggle the plugin on and off.")]
    [TwoColumnCustomDrawer<TwoColumnHotkeyDrawer>]
    public Parameter<int> ToggleHotkey { get; set; } = new(574); // F3 (ImGuiKey.F3)

    /// <summary>Gets or sets the hotkey used to switch between first-person and third-person view.</summary>
    [SettingsParameter]
    [DisplayName("Switch View Hotkey")]
    [Description("The hotkey to switch between first and third person view.")]
    [TwoColumnCustomDrawer<TwoColumnHotkeyDrawer>]
    public Parameter<int> SwitchViewHotkey { get; set; } = new(575); // F4 (ImGuiKey.F4)

    /// <summary>Gets or sets the field-of-view settings group.</summary>
    [SettingsParameter]
    public FovSettings Fov { get; set; } = new();

    /// <summary>Gets or sets the film grain settings group.</summary>
    [SettingsParameter]
    public FilmGrainSettings FilmGrain { get; set; } = new();

    [SettingsParameter]
    public NestedDrawerTest DrawerTest { get; set; } = new();

    /// <summary>
    /// Logs a diagnostic test message to the REFramework console.
    /// Demonstrates <see cref="ButtonDrawer"/> with a primary style and full-width layout,
    /// and <see cref="OrderAttribute"/> to pin this button above all other root-level settings.
    /// </summary>
    [SettingsParameter]
    [DisplayName("Log Test Message")]
    [Description("Logs a test message to the REFramework console to verify the plugin is active.")]
    [CustomDrawer<ButtonDrawer>]
    [ButtonStyle(ButtonStyle.Primary)]
    [ButtonWidth(-1f)]
    [Order(0)]
    public Parameter<Action> LogTestMessage { get; set; } =
        new(static () => Logger.Info("Sample Plugin is active!"));

    /// <summary>Resets all General settings to their default values.</summary>
    [SettingsParameter]
    [DisplayName("Reset General")]
    [Description("Resets the enabled toggle and hotkey bindings to their default values.")]
    [CustomDrawer<ButtonDrawer>]
    [ButtonStyle(ButtonStyle.Danger)]
    [ButtonWidth(-1f)]
    public Parameter<Action> ResetGeneral { get; init; }

    /// <summary>Initializes a new <see cref="PluginConfig"/> and wires up the reset action.</summary>
    public PluginConfig()
    {
        ResetGeneral = new(() =>
        {
            IsEnabled.Reset();
            ToggleHotkey.Reset();
            SwitchViewHotkey.Reset();
        });
    }

    /// <summary>
    /// Nested settings group that controls the camera field-of-view (FOV) for each
    /// view mode and aiming state combination.
    /// </summary>
    [AutoRegisterSettings]
    [Category("FOV")]
    [SettingsPrefix("fov")]
    [CollapseAsTree]
    public record FovSettings
    {
        private const float _minFov = 10f;
        private const float _maxFov = 180f;

        /// <summary>Gets or sets the FOV angle used in third-person view.</summary>
        [SettingsParameter]
        [DisplayName("3rd Person")]
        [Description("The FOV to use in third person.")]
        [Range(_minFov, _maxFov)]
        [Format("%.1f")]
        public Parameter<float> Tps { get; set; } = new(55f);

        /// <summary>Gets or sets the FOV angle used when aiming down sights in third-person view.</summary>
        /// <remarks>Declared before <see cref="Fps"/> but rendered after it via <c>[Order(2)]</c>.</remarks>
        [SettingsParameter]
        [DisplayName("3rd Person ADS")]
        [Description("The FOV to use when aiming down sights in third person.")]
        [Range(_minFov, _maxFov)]
        [Format("%.1f")]
        [Order(2)]
        public Parameter<float> TpsAds { get; set; } = new(35f);

        /// <summary>Gets or sets the FOV angle used in first-person view.</summary>
        /// <remarks>Declared after <see cref="TpsAds"/> but rendered before it via <c>[Order(1)]</c>.</remarks>
        [SettingsParameter]
        [DisplayName("1st Person")]
        [Description("The FOV to use in first person.")]
        [Range(_minFov, _maxFov)]
        [Format("%.1f")]
        [Order(1)]
        public Parameter<float> Fps { get; set; } = new(55f);

        /// <summary>Gets or sets the FOV angle used when aiming down sights in first-person view.</summary>
        [SettingsParameter]
        [DisplayName("1st Person ADS")]
        [Description("The FOV to use when aiming down sights in first person.")]
        [Range(_minFov, _maxFov)]
        [Format("%.1f")]
        public Parameter<float> FpsAds { get; set; } = new(35f);

        /// <summary>Resets all FOV settings to their default values.</summary>
        [SettingsParameter]
        [DisplayName("Reset FOV")]
        [Description("Resets all field-of-view values to their defaults.")]
        [CustomDrawer<ButtonDrawer>]
        [ButtonStyle(ButtonStyle.Danger)]
        [ButtonWidth(-1f)]
        public Parameter<Action> ResetFov { get; init; }

        /// <summary>Initializes a new <see cref="FovSettings"/> and wires up the reset action.</summary>
        public FovSettings()
        {
            ResetFov = new(() =>
            {
                Tps.Reset();
                Fps.Reset();
                TpsAds.Reset();
                FpsAds.Reset();
            });
        }
    }

    /// <summary>
    /// Nested settings group that controls the camera film grain post-processing effect,
    /// including a master disable toggle and an opacity slider.
    /// </summary>
    [AutoRegisterSettings]
    [Category("Film Grain")]
    [SettingsPrefix("filmGrain")]
    [CollapseAsTree]
    public record FilmGrainSettings
    {
        /// <summary>Gets or sets whether the film grain post-processing effect is disabled entirely.</summary>
        [SettingsParameter]
        [DisplayName("Disable Film Grain")]
        [Description("Whether the film grain effect is disabled or not.")]
        public Parameter<bool> Disabled { get; set; } = new(true);

        /// <summary>
        /// Gets or sets the opacity of the film grain effect.
        /// Hidden in the UI while <see cref="Disabled"/> is <see langword="true"/>.
        /// </summary>
        [SettingsParameter]
        [DisplayName("Opacity")]
        [Description("The opacity of the film grain effect. 0 = no effect, 1 = full effect.")]
        [Range(0f, 1f)]
        [Format("%.2f")]
        [HideIf<bool>(nameof(Disabled), true)]
        public Parameter<float> Opacity { get; set; } = new(.15f);

        /// <summary>Resets all film grain settings to their default values.</summary>
        [SettingsParameter]
        [DisplayName("Reset Film Grain")]
        [Description("Resets the film grain toggle and opacity to their defaults.")]
        [CustomDrawer<ButtonDrawer>]
        [ButtonStyle(ButtonStyle.Danger)]
        [ButtonWidth(-1f)]
        public Parameter<Action> ResetFilmGrain { get; init; }

        /// <summary>Initializes a new <see cref="FilmGrainSettings"/> and wires up the reset action.</summary>
        public FilmGrainSettings()
        {
            ResetFilmGrain = new(() =>
            {
                Disabled.Reset();
                Opacity.Reset();
            });
        }
    }

    [AutoRegisterSettings]
    [Category("Drawer Test")]
    [CollapseAsTree]
    [NestedGroupDrawer<NestedDrawerTestDrawer>]
    public record NestedDrawerTest
    {
        public int Value1 { get; set; } = 123;
        public bool Value2 { get; set; } = true;
        public string Value3 { get; set; } = "Hello, world!";
        public float Value4 { get; set; } = 3.14f;
    }
}
