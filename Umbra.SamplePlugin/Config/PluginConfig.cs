using Umbra.Config;
using Umbra.Config.Attributes;
using Umbra.UI.Config.Drawers;

namespace Umbra.SamplePlugin.Config;

/// <summary>
/// Root configuration record for the sample plugin.
/// Organizes the sample into nested groups for each major persisted parameter type and control
/// style, plus additional nested-type and custom-drawer demonstrations.
/// </summary>
/// <remarks>
/// This config is intentionally broad rather than minimal so the sample plugin can exercise most
/// of Umbra's settings surface in one place: hotkeys, booleans, numeric sliders and drags,
/// strings, enums, action buttons, custom drawers, nested-group drawers, category scoping,
/// type-level metadata fallback, indentation, label margins, ordering, spacing, and visibility
/// predicates.
/// </remarks>
[UmbraAutoRegisterSettings]
[UmbraConfigRootNode("Sample Plugin v2.0")]
[UmbraSettingsPrefix("samplePlugin")]
public record PluginConfig
{
    /// <summary>
    /// Logs a diagnostic message to the REFramework console.
    /// The sample plugin entry point replaces the default no-op delegate with a live logger action
    /// after loading the persisted config instance.
    /// </summary>
    [UmbraSettingsParameter]
    [UmbraDisplayName("Log Test Message")]
    [UmbraDescription("Logs a test message to the REFramework console to verify the sample plugin is active.")]
    [UmbraButtonStyle(ButtonStyle.Primary)]
    [UmbraControlWidth(-1f)]
    [UmbraParameterOrder(0)]
    public Parameter<Action> LogTestMessage { get; init; } = new(static () => { });

    /// <summary>
    /// Resets the entire sample configuration tree to its default values.
    /// </summary>
    [UmbraSettingsParameter]
    [UmbraDisplayName("Reset All Samples")]
    [UmbraDescription("Resets every sample group to its default values.")]
    [UmbraButtonStyle(ButtonStyle.Danger)]
    [UmbraControlWidth(-1f)]
    [UmbraParameterOrder(1)]
    public Parameter<Action> ResetAllSamples { get; init; }

    /// <summary>
    /// Gets or sets the general sample settings, including the enable toggle and hotkey drawers.
    /// </summary>
    [UmbraSettingsParameter]
    [UmbraCategory("General")]
    [UmbraSettingsPrefix("general")]
    [UmbraCollapseAsTree(true)]
    public GeneralSettings General { get; set; } = new();

    /// <summary>
    /// Gets or sets the boolean checkbox samples.
    /// </summary>
    [UmbraSettingsParameter]
    [UmbraCategory("Booleans")]
    [UmbraSettingsPrefix("booleans")]
    [UmbraCollapseAsTree]
    public BooleanSamples Booleans { get; set; } = new();

    /// <summary>
    /// Gets or sets the integer slider and drag samples.
    /// </summary>
    [UmbraSettingsParameter]
    [UmbraCategory("Integers")]
    [UmbraSettingsPrefix("integers")]
    [UmbraCollapseAsTree]
    public IntegerSamples Integers { get; set; } = new();

    /// <summary>
    /// Gets or sets the float slider and drag samples.
    /// </summary>
    [UmbraSettingsParameter]
    [UmbraCategory("Floats")]
    [UmbraSettingsPrefix("floats")]
    [UmbraCollapseAsTree]
    public FloatSamples Floats { get; set; } = new();

    /// <summary>
    /// Gets or sets the double slider and drag samples.
    /// </summary>
    [UmbraSettingsParameter]
    [UmbraCategory("Doubles")]
    [UmbraSettingsPrefix("doubles")]
    [UmbraCollapseAsTree]
    public DoubleSamples Doubles { get; set; } = new();

    /// <summary>
    /// Gets or sets the string single-line and multi-line text samples.
    /// </summary>
    [UmbraSettingsParameter]
    [UmbraCategory("Strings")]
    [UmbraSettingsPrefix("strings")]
    [UmbraCollapseAsTree]
    public StringSamples Strings { get; set; } = new();

    /// <summary>
    /// Gets or sets the enum combo-box samples.
    /// </summary>
    [UmbraSettingsParameter]
    [UmbraCategory("Enums")]
    [UmbraSettingsPrefix("enums")]
    [UmbraCollapseAsTree]
    public EnumSamples Enums { get; set; } = new();

    /// <summary>
    /// Gets or sets the samples covering custom parameter drawers, custom button colors, and
    /// nested-group drawers.
    /// </summary>
    [UmbraSettingsParameter]
    [UmbraCategory("Custom Drawers")]
    [UmbraSettingsPrefix("customDrawers")]
    [UmbraCollapseAsTree]
    public CustomDrawerSamples CustomDrawers { get; set; } = new();

    /// <summary>
    /// Gets or sets the samples focused on nested-group behavior and presentation metadata.
    /// </summary>
    [UmbraSettingsParameter]
    [UmbraCategory("Nested Type Tests")]
    [UmbraSettingsPrefix("nestedTypeTests")]
    [UmbraCollapseAsTree]
    public NestedTypeTests NestedTypes { get; set; } = new();

    /// <summary>
    /// Initializes a new <see cref="PluginConfig"/> and wires the root-level sample actions.
    /// </summary>
    public PluginConfig()
    {
        ResetAllSamples = new(() =>
        {
            General.IsEnabled.Reset();
            General.ToggleHotkey.Reset();
            General.SwitchViewHotkey.Reset();
            General.ShowVerboseLogs.Reset();

            Booleans.EnableOverlay.Reset();
            Booleans.EnableFilmGrain.Reset();
            Booleans.UseExperimentalPipeline.Reset();
            Booleans.RequireRestart.Reset();

            Integers.Sliders.MasterVolume.Reset();
            Integers.Sliders.RetryCount.Reset();
            Integers.Sliders.PaddingPixels.Reset();
            Integers.Drags.HorizontalOffset.Reset();
            Integers.Drags.VerticalOffset.Reset();
            Integers.Drags.PriorityBias.Reset();

            Floats.Sliders.Opacity.Reset();
            Floats.Sliders.Gamma.Reset();
            Floats.Sliders.Exposure.Reset();
            Floats.Drags.MoveSpeed.Reset();
            Floats.Drags.BloomStrength.Reset();
            Floats.Drags.CameraLag.Reset();

            Doubles.Sliders.PrecisionScale.Reset();
            Doubles.Sliders.ZoomFactor.Reset();
            Doubles.Drags.WorldOffset.Reset();
            Doubles.Drags.CalibrationBias.Reset();

            Strings.SingleLine.ProfileName.Reset();
            Strings.SingleLine.ExportDirectory.Reset();
            Strings.SingleLine.SearchFilter.Reset();
            Strings.Multiline.Notes.Reset();
            Strings.Multiline.Changelog.Reset();

            Enums.Quality.Reset();
            Enums.Theme.Reset();
            Enums.Channel.Reset();
            Enums.ShowPreviewTheme.Reset();
            Enums.OptionalTheme.Reset();
            Enums.PreviewTheme.Reset();

            CustomDrawers.VisualMeter.Reset();
            CustomDrawers.AccentButtonClicks.Reset();
            CustomDrawers.PrimaryNestedDrawer.Value1.Reset();
            CustomDrawers.PrimaryNestedDrawer.Value2.Reset();
            CustomDrawers.PrimaryNestedDrawer.Value3.Reset();
            CustomDrawers.PrimaryNestedDrawer.Value4.Reset();
            CustomDrawers.SecondaryNestedDrawer.Value1.Reset();
            CustomDrawers.SecondaryNestedDrawer.Value2.Reset();
            CustomDrawers.SecondaryNestedDrawer.Value3.Reset();
            CustomDrawers.SecondaryNestedDrawer.Value4.Reset();

            NestedTypes.Graphics.Enabled.Reset();
            NestedTypes.Graphics.ShowAdvanced.Reset();
            NestedTypes.Graphics.Intensity.Reset();
            NestedTypes.Graphics.Advanced.Threshold.Reset();
            NestedTypes.Graphics.Advanced.Bias.Reset();
            NestedTypes.Graphics.Advanced.Notes.Reset();
            NestedTypes.Audio.Enabled.Reset();
            NestedTypes.Audio.ShowAdvanced.Reset();
            NestedTypes.Audio.Intensity.Reset();
            NestedTypes.Audio.Advanced.Threshold.Reset();
            NestedTypes.Audio.Advanced.Bias.Reset();
            NestedTypes.Audio.Advanced.Notes.Reset();
            NestedTypes.TypeLevelFallback.SampleValue.Reset();
            NestedTypes.TypeLevelFallback.Notes.Reset();
            NestedTypes.PropertyOverride.SampleValue.Reset();
            NestedTypes.PropertyOverride.Notes.Reset();
            NestedTypes.IndentedLayout.PrimaryScale.Reset();
            NestedTypes.IndentedLayout.SecondaryScale.Reset();
            NestedTypes.IndentedLayout.LayoutNotes.Reset();
        });
    }

    /// <summary>
    /// Sample quality levels used by the enum combo-box demos.
    /// </summary>
    public enum SampleQualityLevel
    {
        Low,
        Medium,
        High,
        Ultra
    }

    /// <summary>
    /// Sample UI themes used by the enum combo-box demos.
    /// </summary>
    public enum SampleTheme
    {
        Classic,
        Neon,
        Minimal,
        HighContrast
    }

    /// <summary>
    /// Sample update channels used by the enum combo-box demos.
    /// </summary>
    public enum SampleChannel
    {
        Stable,
        Preview,
        Nightly
    }

    /// <summary>
    /// General sample settings covering the basic boolean and hotkey controls used by many plugins.
    /// </summary>
    [UmbraAutoRegisterSettings]
    public record GeneralSettings
    {
        /// <summary>Gets or sets whether the sample plugin is enabled.</summary>
        [UmbraSettingsParameter]
        [UmbraDisplayName("Enabled")]
        [UmbraDescription("Whether the sample plugin is active.")]
        public Parameter<bool> IsEnabled { get; set; } = new(true);

        /// <summary>Gets or sets the hotkey that toggles the sample plugin.</summary>
        [UmbraSettingsParameter]
        [UmbraDisplayName("Toggle Hotkey")]
        [UmbraDescription("The hotkey used to toggle the sample plugin on and off.")]
        [UmbraTwoColumnCustomDrawer<TwoColumnHotkeyDrawer>]
        public Parameter<int> ToggleHotkey { get; set; } = new(574);

        /// <summary>Gets or sets the hotkey that switches between demo views.</summary>
        [UmbraSettingsParameter]
        [UmbraDisplayName("Switch View Hotkey")]
        [UmbraDescription("The hotkey used to switch between first-person and third-person demo views.")]
        [UmbraTwoColumnCustomDrawer<TwoColumnHotkeyDrawer>]
        public Parameter<int> SwitchViewHotkey { get; set; } = new(575);

        /// <summary>Gets or sets whether the sample emits extra diagnostic logging.</summary>
        [UmbraSettingsParameter]
        [UmbraDisplayName("Verbose Logs")]
        [UmbraDescription("When enabled, the sample plugin emits extra diagnostic log lines during manual testing.")]
        public Parameter<bool> ShowVerboseLogs { get; set; } = new(false);

        /// <summary>Resets the general sample settings to their defaults.</summary>
        [UmbraSettingsParameter]
        [UmbraDisplayName("Reset General")]
        [UmbraDescription("Resets the sample plugin enable toggle, hotkeys, and verbose logging flag.")]
        [UmbraButtonStyle(ButtonStyle.Danger)]
        [UmbraControlWidth(-1f)]
        public Parameter<Action> ResetGeneral { get; init; }

        /// <summary>Initializes a new <see cref="GeneralSettings"/> and wires the reset action.</summary>
        public GeneralSettings()
        {
            ResetGeneral = new(() =>
            {
                IsEnabled.Reset();
                ToggleHotkey.Reset();
                SwitchViewHotkey.Reset();
                ShowVerboseLogs.Reset();
            });
        }
    }

    /// <summary>
    /// Boolean checkbox samples, including a visibility predicate driven by another boolean value.
    /// </summary>
    [UmbraAutoRegisterSettings]
    public record BooleanSamples
    {
        /// <summary>Gets or sets whether the sample overlay is shown.</summary>
        [UmbraSettingsParameter]
        [UmbraDisplayName("Enable Overlay")]
        [UmbraDescription("Shows or hides the sample overlay elements.")]
        public Parameter<bool> EnableOverlay { get; set; } = new(true);

        /// <summary>Gets or sets whether the film grain demo flag is enabled.</summary>
        [UmbraSettingsParameter]
        [UmbraDisplayName("Enable Film Grain")]
        [UmbraDescription("A second checkbox used to validate basic boolean persistence.")]
        public Parameter<bool> EnableFilmGrain { get; set; } = new(false);

        /// <summary>Gets or sets whether the experimental pipeline path is enabled.</summary>
        [UmbraSettingsParameter]
        [UmbraDisplayName("Experimental Pipeline")]
        [UmbraDescription("Enables an experimental code path and reveals the dependent restart flag below.")]
        [UmbraSpacingBefore]
        public Parameter<bool> UseExperimentalPipeline { get; set; } = new(false);

        /// <summary>
        /// Gets or sets whether a restart is required after changing the experimental pipeline.
        /// Hidden unless <see cref="UseExperimentalPipeline"/> is enabled.
        /// </summary>
        [UmbraSettingsParameter]
        [UmbraDisplayName("Require Restart")]
        [UmbraDescription("Visible only while the experimental pipeline is enabled.")]
        [UmbraHideIf<bool>(nameof(UseExperimentalPipeline), false)]
        [UmbraIndent]
        public Parameter<bool> RequireRestart { get; set; } = new(true);

        /// <summary>Resets the boolean samples to their defaults.</summary>
        [UmbraSettingsParameter]
        [UmbraDisplayName("Reset Booleans")]
        [UmbraDescription("Resets the boolean checkbox samples to their defaults.")]
        [UmbraButtonStyle(ButtonStyle.Danger)]
        [UmbraControlWidth(-1f)]
        public Parameter<Action> ResetBooleans { get; init; }

        /// <summary>Initializes a new <see cref="BooleanSamples"/> and wires the reset action.</summary>
        public BooleanSamples()
        {
            ResetBooleans = new(() =>
            {
                EnableOverlay.Reset();
                EnableFilmGrain.Reset();
                UseExperimentalPipeline.Reset();
                RequireRestart.Reset();
            });
        }
    }

    /// <summary>
    /// Integer samples organized into separate ranged-slider and unconstrained-drag nested groups.
    /// </summary>
    [UmbraAutoRegisterSettings]
    [UmbraLabelMargin(10f)]
    public record IntegerSamples
    {
        /// <summary>Gets or sets the ranged integer slider samples.</summary>
        [UmbraSettingsParameter]
        [UmbraCategory("Sliders")]
        [UmbraSettingsPrefix("sliders")]
        [UmbraCollapseAsTree(true)]
        public IntegerSliderSamples Sliders { get; set; } = new();

        /// <summary>Gets or sets the unconstrained integer drag samples.</summary>
        [UmbraSettingsParameter]
        [UmbraCategory("Drags")]
        [UmbraSettingsPrefix("drags")]
        [UmbraCollapseAsTree]
        public IntegerDragSamples Drags { get; set; } = new();
    }

    /// <summary>
    /// Float samples organized into separate ranged-slider and unconstrained-drag nested groups.
    /// </summary>
    [UmbraAutoRegisterSettings]
    public record FloatSamples
    {
        /// <summary>Gets or sets the ranged float slider samples.</summary>
        [UmbraSettingsParameter]
        [UmbraCategory("Sliders")]
        [UmbraSettingsPrefix("sliders")]
        [UmbraCollapseAsTree(true)]
        public FloatSliderSamples Sliders { get; set; } = new();

        /// <summary>Gets or sets the unconstrained float drag samples.</summary>
        [UmbraSettingsParameter]
        [UmbraCategory("Drags")]
        [UmbraSettingsPrefix("drags")]
        [UmbraCollapseAsTree]
        public FloatDragSamples Drags { get; set; } = new();
    }

    /// <summary>
    /// Double samples organized into separate ranged-slider and unconstrained-drag nested groups.
    /// These values are intended for manual validation of Umbra's native double-precision controls.
    /// </summary>
    [UmbraAutoRegisterSettings]
    public record DoubleSamples
    {
        /// <summary>Gets or sets the ranged double slider samples.</summary>
        [UmbraSettingsParameter]
        [UmbraCategory("Sliders")]
        [UmbraSettingsPrefix("sliders")]
        [UmbraCollapseAsTree(true)]
        public DoubleSliderSamples Sliders { get; set; } = new();

        /// <summary>Gets or sets the unconstrained double drag samples.</summary>
        [UmbraSettingsParameter]
        [UmbraCategory("Drags")]
        [UmbraSettingsPrefix("drags")]
        [UmbraCollapseAsTree]
        public DoubleDragSamples Drags { get; set; } = new();
    }

    /// <summary>
    /// String samples organized into single-line and multi-line nested groups.
    /// </summary>
    [UmbraAutoRegisterSettings]
    public record StringSamples
    {
        /// <summary>Gets or sets the single-line text samples.</summary>
        [UmbraSettingsParameter]
        [UmbraCategory("Single Line")]
        [UmbraSettingsPrefix("singleLine")]
        [UmbraCollapseAsTree(true)]
        public SingleLineStringSamples SingleLine { get; set; } = new();

        /// <summary>Gets or sets the multi-line text samples.</summary>
        [UmbraSettingsParameter]
        [UmbraCategory("Multi Line")]
        [UmbraSettingsPrefix("multiline")]
        [UmbraCollapseAsTree]
        public MultilineStringSamples Multiline { get; set; } = new();
    }

    /// <summary>
    /// Enum samples demonstrating combo boxes and a dependent visibility predicate.
    /// </summary>
    [UmbraAutoRegisterSettings]
    public record EnumSamples
    {
        /// <summary>Gets or sets the sample quality level.</summary>
        [UmbraSettingsParameter]
        [UmbraDisplayName("Quality")]
        [UmbraDescription("Demonstrates enum rendering through the built-in combo-box control.")]
        public Parameter<SampleQualityLevel> Quality { get; set; } = new(SampleQualityLevel.High);

        /// <summary>Gets or sets the active sample theme.</summary>
        [UmbraSettingsParameter]
        [UmbraDisplayName("Theme")]
        [UmbraDescription("A second enum combo used to validate string-backed enum persistence.")]
        public Parameter<SampleTheme> Theme { get; set; } = new(SampleTheme.Classic);

        /// <summary>Gets or sets the sample update channel.</summary>
        [UmbraSettingsParameter]
        [UmbraDisplayName("Channel")]
        [UmbraDescription("Used to validate enum persistence with multiple options.")]
        public Parameter<SampleChannel> Channel { get; set; } = new(SampleChannel.Stable);

        /// <summary>Gets or sets whether the preview-theme enum is visible.</summary>
        [UmbraSettingsParameter]
        [UmbraDisplayName("Show Preview Theme")]
        [UmbraDescription("Reveals the dependent preview-theme enum below when enabled.")]
        [UmbraSpacingBefore]
        public Parameter<bool> ShowPreviewTheme { get; set; } = new(false);

        /// <summary>Gets or sets an optional nullable enum value with an explicit <c>&lt;None&gt;</c> choice.</summary>
        [UmbraSettingsParameter]
        [UmbraDisplayName("Optional Theme")]
        [UmbraDescription("Validates the built-in combo-box path for nullable enum parameters, including the <None> option.")]
        public Parameter<SampleTheme?> OptionalTheme { get; set; } = new(null);

        /// <summary>
        /// Gets or sets the preview theme shown only while <see cref="ShowPreviewTheme"/> is enabled.
        /// </summary>
        [UmbraSettingsParameter]
        [UmbraDisplayName("Preview Theme")]
        [UmbraDescription("A dependent enum combo-box used to validate HideIf against a sibling boolean.")]
        [UmbraHideIf<bool>(nameof(ShowPreviewTheme), false)]
        [UmbraIndent]
        public Parameter<SampleTheme> PreviewTheme { get; set; } = new(SampleTheme.Neon);
    }

    /// <summary>
    /// Samples covering full custom parameter drawers, nested-group drawers, and custom button colors.
    /// </summary>
    [UmbraAutoRegisterSettings]
    public record CustomDrawerSamples
    {
        /// <summary>Gets or sets the full custom parameter-drawer sample.</summary>
        [UmbraSettingsParameter]
        [UmbraDisplayName("Visual Meter")]
        [UmbraDescription("Rendered by a full custom parameter drawer instead of the default two-column layout.")]
        [UmbraCustomDrawer<NormalizedFloatPreviewDrawer>]
        public Parameter<float> VisualMeter { get; set; } = new(.42f);

        /// <summary>Gets or sets the sample click counter incremented by the custom-colored action button.</summary>
        [UmbraSettingsParameter]
        [UmbraDisplayName("Accent Button Clicks")]
        [UmbraDescription("A plain persisted value that makes the custom-colored button action observable.")]
        [UmbraRange(0, 999)]
        public Parameter<int> AccentButtonClicks { get; set; } = new(0);

        /// <summary>Gets or sets the custom-colored action button sample.</summary>
        [UmbraSettingsParameter]
        [UmbraDisplayName("Accent Action")]
        [UmbraDescription("Uses explicit custom RGBA button colors instead of a built-in button style.")]
        [UmbraCustomButtonColors(0.12f, 0.42f, 0.78f)]
        [UmbraControlWidth(-1f)]
        public Parameter<Action> AccentButton { get; init; }

        /// <summary>
        /// Gets or sets the first nested-group drawer sample.
        /// Its internal widgets intentionally reuse fixed local labels shared with the sibling sample.
        /// </summary>
        [UmbraSettingsParameter]
        [UmbraCategory("Nested Drawer")]
        [UmbraSettingsPrefix("primaryNestedDrawer")]
        [UmbraCollapseAsTree(true)]
        public NestedDrawerTest PrimaryNestedDrawer { get; set; } = new();

        /// <summary>
        /// Gets or sets the second nested-group drawer sample in the same category and parent scope.
        /// </summary>
        [UmbraSettingsParameter]
        [UmbraCategory("Nested Drawer")]
        [UmbraSettingsPrefix("secondaryNestedDrawer")]
        [UmbraCollapseAsTree(true)]
        public NestedDrawerTest SecondaryNestedDrawer { get; set; } = new()
        {
            Value1 = new(456),
            Value2 = new(false),
            Value3 = new("Second nested drawer sample"),
            Value4 = new(6.28f)
        };

        /// <summary>Initializes a new <see cref="CustomDrawerSamples"/> and wires the custom-colored button action.</summary>
        public CustomDrawerSamples()
        {
            AccentButton = new(() => AccentButtonClicks.Value = AccentButtonClicks.Value + 1);
        }
    }

    /// <summary>
    /// Samples focused on nested-type behavior, local category scoping, and presentation-metadata fallback.
    /// </summary>
    [UmbraAutoRegisterSettings]
    public record NestedTypeTests
    {
        /// <summary>Gets or sets the graphics branch used for local category-scoping tests.</summary>
        [UmbraSettingsParameter]
        [UmbraCategory("Graphics")]
        [UmbraSettingsPrefix("graphics")]
        [UmbraCollapseAsTree(true)]
        [UmbraParameterOrder(0)]
        public ScopedBranchSettings Graphics { get; set; } = new()
        {
            Intensity = new(.25f),
            Advanced = new()
            {
                Threshold = new(20),
                Bias = new(-3),
                Notes = new("Graphics branch")
            }
        };

        /// <summary>Gets or sets the audio branch used for local category-scoping tests.</summary>
        [UmbraSettingsParameter]
        [UmbraCategory("Audio")]
        [UmbraSettingsPrefix("audio")]
        [UmbraCollapseAsTree(true)]
        [UmbraParameterOrder(1)]
        public ScopedBranchSettings Audio { get; set; } = new()
        {
            Enabled = new(false),
            ShowAdvanced = new(false),
            Intensity = new(.80f),
            Advanced = new()
            {
                Threshold = new(75),
                Bias = new(4),
                Notes = new("Audio branch")
            }
        };

        /// <summary>
        /// Gets or sets the nested group that relies on its own type-level category and collapse metadata.
        /// </summary>
        [UmbraSettingsParameter]
        [UmbraSettingsPrefix("typeLevelFallback")]
        [UmbraParameterOrder(2)]
        public TypeLevelPresentationSettings TypeLevelFallback { get; set; } = new();

        /// <summary>
        /// Gets or sets the nested group whose property-level category overrides its type-level fallback metadata.
        /// </summary>
        [UmbraSettingsParameter]
        [UmbraCategory("Property Override")]
        [UmbraSettingsPrefix("propertyOverride")]
        [UmbraCollapseAsTree]
        [UmbraParameterOrder(3)]
        public TypeLevelPresentationSettings PropertyOverride { get; set; } = new()
        {
            SampleValue = new(77),
            Notes = new("Property-level category should win.")
        };

        /// <summary>
        /// Gets or sets the nested group that demonstrates type-level indentation and label-margin metadata.
        /// </summary>
        [UmbraSettingsParameter]
        [UmbraSettingsPrefix("indentedLayout")]
        [UmbraParameterOrder(4)]
        public IndentedLayoutSettings IndentedLayout { get; set; } = new();
    }

    /// <summary>
    /// Shared branch used by the nested-type tests to validate local category scoping and nested visibility.
    /// </summary>
    [UmbraAutoRegisterSettings]
    public record ScopedBranchSettings
    {
        /// <summary>Gets or sets whether this branch is enabled.</summary>
        [UmbraSettingsParameter]
        [UmbraDisplayName("Enabled")]
        [UmbraDescription("Whether this nested branch is active.")]
        [UmbraCategory("General")]
        public Parameter<bool> Enabled { get; set; } = new(true);

        /// <summary>Gets or sets whether the nested Advanced branch is visible.</summary>
        [UmbraSettingsParameter]
        [UmbraDisplayName("Show Advanced")]
        [UmbraDescription("Controls whether the nested Advanced branch below is rendered.")]
        [UmbraCategory("General")]
        [UmbraParameterOrder(0)]
        public Parameter<bool> ShowAdvanced { get; set; } = new(true);

        /// <summary>Gets or sets the branch intensity value.</summary>
        [UmbraSettingsParameter]
        [UmbraDisplayName("Intensity")]
        [UmbraDescription("A simple float used to validate nested ordering and local categories.")]
        [UmbraCategory("General")]
        [UmbraRange(0f, 1f)]
        [UmbraFormat("%.2f")]
        [UmbraParameterOrder(1)]
        public Parameter<float> Intensity { get; set; } = new(.50f);

        /// <summary>Gets or sets the nested Advanced branch.</summary>
        [UmbraSettingsParameter]
        [UmbraCategory("Advanced")]
        [UmbraSettingsPrefix("advanced")]
        [UmbraCollapseAsTree]
        [UmbraHideIf<bool>(nameof(ShowAdvanced), false)]
        [UmbraSpacingBefore]
        [UmbraSpacingAfter]
        [UmbraParameterOrder(2)]
        public ScopedAdvancedSettings Advanced { get; set; } = new();
    }

    /// <summary>
    /// Second-level nested branch used by the scoped-branch demo.
    /// </summary>
    [UmbraAutoRegisterSettings]
    public record ScopedAdvancedSettings
    {
        /// <summary>Gets or sets the advanced threshold value.</summary>
        [UmbraSettingsParameter]
        [UmbraDisplayName("Threshold")]
        [UmbraDescription("A nested integer rendered in the Tuning category.")]
        [UmbraCategory("Tuning")]
        [UmbraRange(0, 100)]
        public Parameter<int> Threshold { get; set; } = new(50);

        /// <summary>Gets or sets the advanced bias value.</summary>
        [UmbraSettingsParameter]
        [UmbraDisplayName("Bias")]
        [UmbraDescription("A second nested integer rendered alongside Threshold.")]
        [UmbraCategory("Tuning")]
        [UmbraRange(-10, 10)]
        public Parameter<int> Bias { get; set; } = new(0);

        /// <summary>Gets or sets the branch note text.</summary>
        [UmbraSettingsParameter]
        [UmbraDisplayName("Notes")]
        [UmbraDescription("A nested string rendered in a different local category.")]
        [UmbraCategory("Details")]
        [UmbraMaxLength(80)]
        public Parameter<string> Notes { get; set; } = new("Nested category names should remain local.");
    }

    /// <summary>
    /// Nested settings group that declares its own presentation metadata at the type level.
    /// </summary>
    [UmbraAutoRegisterSettings]
    [UmbraCategory("Type-Level Fallback")]
    [UmbraCollapseAsTree]
    public record TypeLevelPresentationSettings
    {
        /// <summary>Gets or sets the sample numeric value rendered in the fallback group.</summary>
        [UmbraSettingsParameter]
        [UmbraDisplayName("Sample Value")]
        [UmbraDescription("A sample numeric value used to validate type-level presentation metadata fallback.")]
        [UmbraCategory("Values")]
        [UmbraRange(0, 100)]
        public Parameter<int> SampleValue { get; set; } = new(42);

        /// <summary>Gets or sets the sample note rendered in the fallback group.</summary>
        [UmbraSettingsParameter]
        [UmbraDisplayName("Notes")]
        [UmbraDescription("A sample note used to validate type-level category fallback.")]
        [UmbraCategory("Details")]
        [UmbraMaxLength(80)]
        public Parameter<string> Notes { get; set; } = new("Type-level category fallback.");
    }

    /// <summary>
    /// Nested settings group that demonstrates class-level indentation and label-margin metadata.
    /// </summary>
    [UmbraAutoRegisterSettings]
    [UmbraCategory("Indented Layout")]
    [UmbraCollapseAsTree(true)]
    [UmbraIndent(18f)]
    [UmbraLabelMargin(16f)]
    public record IndentedLayoutSettings
    {
        /// <summary>Gets or sets the primary layout scale.</summary>
        [UmbraSettingsParameter]
        [UmbraDisplayName("Primary Scale")]
        [UmbraDescription("A ranged float rendered inside a class-level indented layout group.")]
        [UmbraRange(0.5f, 2.0f)]
        [UmbraFormat("%.2f")]
        public Parameter<float> PrimaryScale { get; set; } = new(1.00f);

        /// <summary>Gets or sets the secondary layout scale.</summary>
        [UmbraSettingsParameter]
        [UmbraDisplayName("Secondary Scale")]
        [UmbraDescription("An unconstrained float used to validate layout metadata on drag controls.")]
        [UmbraStep(0.05f)]
        [UmbraFormat("%.2f")]
        public Parameter<float> SecondaryScale { get; set; } = new(1.25f);

        /// <summary>Gets or sets free-form notes for the layout demo.</summary>
        [UmbraSettingsParameter]
        [UmbraDisplayName("Layout Notes")]
        [UmbraDescription("A multi-line string used to validate class-level label-margin behavior.")]
        [UmbraMultiline(3)]
        [UmbraMaxLength(160)]
        public Parameter<string> LayoutNotes { get; set; } = new("This group uses type-level indentation and extra label margin.");
    }

    /// <summary>
    /// Ranged integer slider samples.
    /// </summary>
    [UmbraAutoRegisterSettings]
    public record IntegerSliderSamples
    {
        [UmbraSettingsParameter, UmbraDisplayName("Master Volume"), UmbraDescription("A ranged integer slider with a percentage-like value."), UmbraRange(0, 100)]
        public Parameter<int> MasterVolume { get; set; } = new(80);

        [UmbraSettingsParameter, UmbraDisplayName("Retry Count"), UmbraDescription("A small ranged integer slider used for discrete step testing."), UmbraRange(0, 10)]
        public Parameter<int> RetryCount { get; set; } = new(3);

        [UmbraSettingsParameter, UmbraDisplayName("Padding"), UmbraDescription("A ranged integer slider with a custom integer display format."), UmbraRange(0, 64), UmbraFormat("%d px")]
        public Parameter<int> PaddingPixels { get; set; } = new(12);
    }

    /// <summary>
    /// Unconstrained integer drag samples.
    /// </summary>
    [UmbraAutoRegisterSettings]
    public record IntegerDragSamples
    {
        [UmbraSettingsParameter, UmbraDisplayName("Horizontal Offset"), UmbraDescription("An unconstrained integer drag sample."), UmbraStep(1)]
        public Parameter<int> HorizontalOffset { get; set; } = new(16);

        [UmbraSettingsParameter, UmbraDisplayName("Vertical Offset"), UmbraDescription("A second unconstrained integer drag sample with a negative default."), UmbraStep(1)]
        public Parameter<int> VerticalOffset { get; set; } = new(-8);

        [UmbraSettingsParameter, UmbraDisplayName("Priority Bias"), UmbraDescription("An unconstrained integer drag sample with a larger drag step."), UmbraStep(5)]
        public Parameter<int> PriorityBias { get; set; } = new(25);
    }

    /// <summary>
    /// Ranged float slider samples.
    /// </summary>
    [UmbraAutoRegisterSettings]
    public record FloatSliderSamples
    {
        [UmbraSettingsParameter, UmbraDisplayName("Opacity"), UmbraDescription("A normalized float slider."), UmbraRange(0f, 1f), UmbraFormat("%.2f")]
        public Parameter<float> Opacity { get; set; } = new(.65f);

        [UmbraSettingsParameter, UmbraDisplayName("Gamma"), UmbraDescription("A wider float slider range for manual validation."), UmbraRange(0.5f, 3.0f), UmbraFormat("%.2f")]
        public Parameter<float> Gamma { get; set; } = new(1.20f);

        [UmbraSettingsParameter, UmbraDisplayName("Exposure"), UmbraDescription("A float slider with one decimal place."), UmbraRange(-2f, 2f), UmbraFormat("%.1f")]
        public Parameter<float> Exposure { get; set; } = new(.5f);
    }

    /// <summary>
    /// Unconstrained float drag samples.
    /// </summary>
    [UmbraAutoRegisterSettings]
    public record FloatDragSamples
    {
        [UmbraSettingsParameter, UmbraDisplayName("Move Speed"), UmbraDescription("An unconstrained float drag sample."), UmbraStep(0.05f), UmbraFormat("%.2f")]
        public Parameter<float> MoveSpeed { get; set; } = new(1.50f);

        [UmbraSettingsParameter, UmbraDisplayName("Bloom Strength"), UmbraDescription("A smaller-step float drag sample."), UmbraStep(0.01f), UmbraFormat("%.2f")]
        public Parameter<float> BloomStrength { get; set; } = new(.35f);

        [UmbraSettingsParameter, UmbraDisplayName("Camera Lag"), UmbraDescription("A float drag sample with three decimal places."), UmbraStep(0.005f), UmbraFormat("%.3f")]
        public Parameter<float> CameraLag { get; set; } = new(.125f);
    }

    /// <summary>
    /// Ranged double slider samples.
    /// </summary>
    [UmbraAutoRegisterSettings]
    public record DoubleSliderSamples
    {
        [UmbraSettingsParameter, UmbraDisplayName("Precision Scale"), UmbraDescription("A ranged double slider used to validate native double-precision slider editing."), UmbraRange(0.0, 1.0), UmbraStep(0.001), UmbraFormat("%.3f")]
        public Parameter<double> PrecisionScale { get; set; } = new(0.125);

        [UmbraSettingsParameter, UmbraDisplayName("Zoom Factor"), UmbraDescription("A second ranged double slider with a broader range."), UmbraRange(0.5, 4.0), UmbraStep(0.001), UmbraFormat("%.3f")]
        public Parameter<double> ZoomFactor { get; set; } = new(1.750);
    }

    /// <summary>
    /// Unconstrained double drag samples.
    /// </summary>
    [UmbraAutoRegisterSettings]
    public record DoubleDragSamples
    {
        [UmbraSettingsParameter, UmbraDisplayName("World Offset"), UmbraDescription("An unconstrained double drag sample used to validate precision beyond float."), UmbraStep(0.125), UmbraFormat("%.3f")]
        public Parameter<double> WorldOffset { get; set; } = new(12.375);

        [UmbraSettingsParameter, UmbraDisplayName("Calibration Bias"), UmbraDescription("A second unconstrained double drag sample with a small step size."), UmbraStep(0.0005), UmbraFormat("%.4f")]
        public Parameter<double> CalibrationBias { get; set; } = new(0.0025);
    }

    /// <summary>
    /// Single-line string samples.
    /// </summary>
    [UmbraAutoRegisterSettings]
    public record SingleLineStringSamples
    {
        [UmbraSettingsParameter, UmbraDisplayName("Profile Name"), UmbraDescription("A short single-line string sample."), UmbraMaxLength(40)]
        public Parameter<string> ProfileName { get; set; } = new("Umbra Tester");

        [UmbraSettingsParameter, UmbraDisplayName("Export Directory"), UmbraDescription("A longer single-line string sample."), UmbraMaxLength(120)]
        public Parameter<string> ExportDirectory { get; set; } = new("data/Umbra/SamplePlugin/exports");

        [UmbraSettingsParameter, UmbraDisplayName("Search Filter"), UmbraDescription("A third single-line string sample used for quick manual edits."), UmbraMaxLength(60)]
        public Parameter<string> SearchFilter { get; set; } = new("player camera");
    }

    /// <summary>
    /// Multi-line string samples.
    /// </summary>
    [UmbraAutoRegisterSettings]
    public record MultilineStringSamples
    {
        [UmbraSettingsParameter, UmbraDisplayName("Notes"), UmbraDescription("A short multi-line text sample."), UmbraMultiline(3), UmbraMaxLength(160)]
        public Parameter<string> Notes { get; set; } = new("Use this field to validate multi-line text persistence.");

        [UmbraSettingsParameter, UmbraDisplayName("Changelog"), UmbraDescription("A larger multi-line text sample with more visible rows."), UmbraMultiline(5), UmbraMaxLength(320)]
        public Parameter<string> Changelog { get; set; } = new("- Added data-type sample groups\n- Added nested type tests\n- Added custom drawer coverage");
    }

    /// <summary>
    /// Sample nested settings group rendered by a custom nested-group drawer.
    /// Multiple instances of this type intentionally reuse the same local widget labels so the
    /// sample can validate nested-group ImGui ID scoping manually.
    /// </summary>
    [UmbraAutoRegisterSettings]
    [UmbraCategory("Drawer Test")]
    [UmbraCollapseAsTree]
    [UmbraNestedGroupDrawer<NestedDrawerTestDrawer>]
    public record NestedDrawerTest
    {
        /// <summary>Gets or sets the first sample integer value for the nested drawer test.</summary>
        [UmbraSettingsParameter]
        public Parameter<int> Value1 { get; set; } = new(123);

        /// <summary>Gets or sets the second sample boolean value for the nested drawer test.</summary>
        [UmbraSettingsParameter]
        public Parameter<bool> Value2 { get; set; } = new(true);

        /// <summary>Gets or sets the third sample string value for the nested drawer test.</summary>
        [UmbraSettingsParameter]
        public Parameter<string> Value3 { get; set; } = new("Hello, world!");

        /// <summary>Gets or sets the fourth sample float value for the nested drawer test.</summary>
        [UmbraSettingsParameter]
        public Parameter<float> Value4 { get; set; } = new(3.14f);
    }
}
