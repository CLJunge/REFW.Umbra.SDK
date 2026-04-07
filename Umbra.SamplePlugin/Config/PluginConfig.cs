using Umbra.Config;
using Umbra.Config.Attributes;
using Umbra.Input;
using System.Numerics;
using Umbra.UI.Config.Drawers;

namespace Umbra.SamplePlugin.Config;

/// <summary>
/// Root configuration record for the sample plugin.
/// Organizes the sample into nested groups for each major persisted parameter type and control
/// style, plus additional nested-type and custom-drawer demonstrations.
/// </summary>
/// <remarks>
/// This config is intentionally broad rather than minimal so the sample plugin can exercise most
/// of Umbra's config surface in one place: hotkeys, booleans, numeric sliders and drags,
/// strings, enums, action buttons, custom drawers, nested-group drawers, category scoping,
/// type-level metadata fallback, indentation, label margins, ordering, spacing, and visibility
/// predicates.
/// </remarks>
[UmbraAutoRegister]
[UmbraRootNode("Sample Plugin v2.0")]
[UmbraPrefix("samplePlugin")]
public record PluginConfig
{
    /// <summary>
    /// Logs a diagnostic message to the REFramework console.
    /// The sample plugin entry point replaces the default no-op delegate with a live logger action
    /// after loading the persisted config instance.
    /// </summary>
    [UmbraParameter]
    [UmbraDisplayName("Log Test Message")]
    [UmbraDescription("Logs a test message to the REFramework console to verify the sample plugin is active.")]
    [UmbraButtonStyle(ButtonStyle.Primary)]
    [UmbraControlWidth(-1f)]
    [UmbraParameterOrder(0)]
    public Parameter<Action> LogTestMessage { get; init; } = new(static () => { });

    /// <summary>
    /// Resets the entire sample configuration tree to its default values.
    /// The sample plugin entry point replaces the default no-op delegate with the live store-level
    /// reset action after loading the persisted config instance.
    /// </summary>
    [UmbraParameter]
    [UmbraDisplayName("Reset All Samples")]
    [UmbraDescription("Resets every sample group to its default values.")]
    [UmbraButtonStyle(ButtonStyle.Danger)]
    [UmbraControlWidth(-1f)]
    [UmbraParameterOrder(1)]
    public Parameter<Action> ResetAllSamples { get; init; } = new(static () => { });

    /// <summary>
    /// Gets or sets the general sample config group, including the enable toggle and hotkey drawers.
    /// </summary>
    [UmbraParameter]
    [UmbraCategory("General")]
    [UmbraPrefix("general")]
    [UmbraCollapseAsTree(true)]
    public GeneralConfig General { get; set; } = new();

    /// <summary>
    /// Gets or sets the boolean checkbox samples.
    /// </summary>
    [UmbraParameter]
    [UmbraCategory("Booleans")]
    [UmbraPrefix("booleans")]
    [UmbraCollapseAsTree]
    public BooleanSamples Booleans { get; set; } = new();

    /// <summary>
    /// Gets or sets the integer slider and drag samples.
    /// </summary>
    [UmbraParameter]
    [UmbraCategory("Integers")]
    [UmbraPrefix("integers")]
    [UmbraCollapseAsTree]
    public IntegerSamples Integers { get; set; } = new();

    /// <summary>
    /// Gets or sets the float slider and drag samples.
    /// </summary>
    [UmbraParameter]
    [UmbraCategory("Floats")]
    [UmbraPrefix("floats")]
    [UmbraCollapseAsTree]
    public FloatSamples Floats { get; set; } = new();

    /// <summary>
    /// Gets or sets the double slider and drag samples.
    /// </summary>
    [UmbraParameter]
    [UmbraCategory("Doubles")]
    [UmbraPrefix("doubles")]
    [UmbraCollapseAsTree]
    public DoubleSamples Doubles { get; set; } = new();

    /// <summary>
    /// Gets or sets the string single-line and multi-line text samples.
    /// </summary>
    [UmbraParameter]
    [UmbraCategory("Strings")]
    [UmbraPrefix("strings")]
    [UmbraCollapseAsTree]
    public StringSamples Strings { get; set; } = new();

    /// <summary>
    /// Gets or sets the enum combo-box samples.
    /// </summary>
    [UmbraParameter]
    [UmbraCategory("Enums")]
    [UmbraPrefix("enums")]
    [UmbraCollapseAsTree]
    public EnumSamples Enums { get; set; } = new();

    /// <summary>
    /// Gets or sets the samples covering custom parameter drawers, custom button colors, and
    /// nested-group drawers.
    /// </summary>
    [UmbraParameter]
    [UmbraCategory("Custom Drawers")]
    [UmbraPrefix("customDrawers")]
    [UmbraCollapseAsTree]
    public CustomDrawerSamples CustomDrawers { get; set; } = new();

    /// <summary>
    /// Gets or sets the samples focused on nested-group behavior and presentation metadata.
    /// </summary>
    [UmbraParameter]
    [UmbraCategory("Nested Type Tests")]
    [UmbraPrefix("nestedTypeTests")]
    [UmbraCollapseAsTree]
    public NestedTypeTests NestedTypes { get; set; } = new();

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
    /// General sample config covering the basic boolean and hotkey controls used by many plugins.
    /// </summary>
    [UmbraAutoRegister]
    public record GeneralConfig
    {
        /// <summary>Gets or sets whether the sample plugin is enabled.</summary>
        [UmbraParameter]
        [UmbraDisplayName("Enabled")]
        [UmbraDescription("Whether the sample plugin is active.")]
        public Parameter<bool> IsEnabled { get; set; } = new(true);

        /// <summary>Gets or sets the hotkey that toggles the sample plugin.</summary>
        [UmbraParameter]
        [UmbraDisplayName("Toggle Hotkey")]
        [UmbraDescription("The hotkey used to toggle the sample plugin on and off.")]
        [UmbraTwoColumnDrawer<TwoColumnHotkeyDrawer>]
        public Parameter<HotkeyBinding> ToggleHotkey { get; set; } = new(new HotkeyBinding(574, false, false, false));

        /// <summary>Gets or sets the hotkey that switches between demo views.</summary>
        [UmbraParameter]
        [UmbraDisplayName("Switch View Hotkey")]
        [UmbraDescription("The hotkey used to switch between first-person and third-person demo views.")]
        [UmbraTwoColumnDrawer<TwoColumnHotkeyDrawer>]
        public Parameter<HotkeyBinding> SwitchViewHotkey { get; set; } = new(new HotkeyBinding(575, false, false, false));

        /// <summary>Gets or sets whether the sample emits extra diagnostic logging.</summary>
        [UmbraParameter]
        [UmbraDisplayName("Verbose Logs")]
        [UmbraDescription("When enabled, the sample plugin emits extra diagnostic log lines during manual testing.")]
        public Parameter<bool> ShowVerboseLogs { get; set; } = new(false);

        /// <summary>Resets the general sample config values to their defaults.</summary>
        [UmbraParameter]
        [UmbraDisplayName("Reset General")]
        [UmbraDescription("Resets the sample plugin enable toggle, hotkeys, and verbose logging flag.")]
        [UmbraButtonStyle(ButtonStyle.Danger)]
        [UmbraControlWidth(-1f)]
        public Parameter<Action> ResetGeneral { get; init; }

        /// <summary>Initializes a new <see cref="GeneralConfig"/> and wires the reset action.</summary>
        public GeneralConfig()
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
    [UmbraAutoRegister]
    public record BooleanSamples
    {
        /// <summary>Gets or sets whether the sample overlay is shown.</summary>
        [UmbraParameter]
        [UmbraDisplayName("Enable Overlay")]
        [UmbraDescription("Shows or hides the sample overlay elements.")]
        public Parameter<bool> EnableOverlay { get; set; } = new(true);

        /// <summary>Gets or sets whether the film grain demo flag is enabled.</summary>
        [UmbraParameter]
        [UmbraDisplayName("Enable Film Grain")]
        [UmbraDescription("A second checkbox used to validate basic boolean persistence.")]
        public Parameter<bool> EnableFilmGrain { get; set; } = new(false);

        /// <summary>Gets or sets whether the experimental pipeline path is enabled.</summary>
        [UmbraParameter]
        [UmbraDisplayName("Experimental Pipeline")]
        [UmbraDescription("Enables an experimental code path and reveals the dependent restart flag below.")]
        [UmbraSpacingBefore]
        public Parameter<bool> UseExperimentalPipeline { get; set; } = new(false);

        /// <summary>
        /// Gets or sets whether a restart is required after changing the experimental pipeline.
        /// Hidden unless <see cref="UseExperimentalPipeline"/> is enabled.
        /// </summary>
        [UmbraParameter]
        [UmbraDisplayName("Require Restart")]
        [UmbraDescription("Visible only while the experimental pipeline is enabled.")]
        [UmbraHideIf<bool>(nameof(UseExperimentalPipeline), false)]
        [UmbraIndent]
        public Parameter<bool> RequireRestart { get; set; } = new(true);

        /// <summary>Resets the boolean samples to their defaults.</summary>
        [UmbraParameter]
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
    [UmbraAutoRegister]
    [UmbraLabelMargin(10f)]
    public record IntegerSamples
    {
        /// <summary>Gets or sets the ranged integer slider samples.</summary>
        [UmbraParameter]
        [UmbraCategory("Sliders")]
        [UmbraPrefix("sliders")]
        [UmbraCollapseAsTree(true)]
        public IntegerSliderSamples Sliders { get; set; } = new();

        /// <summary>Gets or sets the unconstrained integer drag samples.</summary>
        [UmbraParameter]
        [UmbraCategory("Drags")]
        [UmbraPrefix("drags")]
        [UmbraCollapseAsTree]
        public IntegerDragSamples Drags { get; set; } = new();
    }

    /// <summary>
    /// Float samples organized into separate ranged-slider and unconstrained-drag nested groups.
    /// </summary>
    [UmbraAutoRegister]
    public record FloatSamples
    {
        /// <summary>Gets or sets the ranged float slider samples.</summary>
        [UmbraParameter]
        [UmbraCategory("Sliders")]
        [UmbraPrefix("sliders")]
        [UmbraCollapseAsTree(true)]
        public FloatSliderSamples Sliders { get; set; } = new();

        /// <summary>Gets or sets the unconstrained float drag samples.</summary>
        [UmbraParameter]
        [UmbraCategory("Drags")]
        [UmbraPrefix("drags")]
        [UmbraCollapseAsTree]
        public FloatDragSamples Drags { get; set; } = new();
    }

    /// <summary>
    /// Double samples organized into separate ranged-slider and unconstrained-drag nested groups.
    /// These values are intended for manual validation of Umbra's native double-precision controls.
    /// </summary>
    [UmbraAutoRegister]
    public record DoubleSamples
    {
        /// <summary>Gets or sets the ranged double slider samples.</summary>
        [UmbraParameter]
        [UmbraCategory("Sliders")]
        [UmbraPrefix("sliders")]
        [UmbraCollapseAsTree(true)]
        public DoubleSliderSamples Sliders { get; set; } = new();

        /// <summary>Gets or sets the unconstrained double drag samples.</summary>
        [UmbraParameter]
        [UmbraCategory("Drags")]
        [UmbraPrefix("drags")]
        [UmbraCollapseAsTree]
        public DoubleDragSamples Drags { get; set; } = new();
    }

    /// <summary>
    /// String samples organized into single-line and multi-line nested groups.
    /// </summary>
    [UmbraAutoRegister]
    public record StringSamples
    {
        /// <summary>Gets or sets the single-line text samples.</summary>
        [UmbraParameter]
        [UmbraCategory("Single Line")]
        [UmbraPrefix("singleLine")]
        [UmbraCollapseAsTree(true)]
        public SingleLineStringSamples SingleLine { get; set; } = new();

        /// <summary>Gets or sets the multi-line text samples.</summary>
        [UmbraParameter]
        [UmbraCategory("Multi Line")]
        [UmbraPrefix("multiline")]
        [UmbraCollapseAsTree]
        public MultilineStringSamples Multiline { get; set; } = new();
    }

    /// <summary>
    /// Enum samples demonstrating combo boxes and a dependent visibility predicate.
    /// </summary>
    [UmbraAutoRegister]
    public record EnumSamples
    {
        /// <summary>Gets or sets the sample quality level.</summary>
        [UmbraParameter]
        [UmbraDisplayName("Quality")]
        [UmbraDescription("Demonstrates enum rendering through the built-in combo-box control.")]
        public Parameter<SampleQualityLevel> Quality { get; set; } = new(SampleQualityLevel.High);

        /// <summary>Gets or sets the active sample theme.</summary>
        [UmbraParameter]
        [UmbraDisplayName("Theme")]
        [UmbraDescription("A second enum combo used to validate string-backed enum persistence.")]
        public Parameter<SampleTheme> Theme { get; set; } = new(SampleTheme.Classic);

        /// <summary>Gets or sets the sample update channel.</summary>
        [UmbraParameter]
        [UmbraDisplayName("Channel")]
        [UmbraDescription("Used to validate enum persistence with multiple options.")]
        public Parameter<SampleChannel> Channel { get; set; } = new(SampleChannel.Stable);

        /// <summary>Gets or sets whether the preview-theme enum is visible.</summary>
        [UmbraParameter]
        [UmbraDisplayName("Show Preview Theme")]
        [UmbraDescription("Reveals the dependent preview-theme enum below when enabled.")]
        [UmbraSpacingBefore]
        public Parameter<bool> ShowPreviewTheme { get; set; } = new(false);

        /// <summary>Gets or sets an optional nullable enum value with an explicit <c>&lt;None&gt;</c> choice.</summary>
        [UmbraParameter]
        [UmbraDisplayName("Optional Theme")]
        [UmbraDescription("Validates the built-in combo-box path for nullable enum parameters, including the <None> option.")]
        public Parameter<SampleTheme?> OptionalTheme { get; set; } = new(null);

        /// <summary>
        /// Gets or sets the preview theme shown only while <see cref="ShowPreviewTheme"/> is enabled.
        /// </summary>
        [UmbraParameter]
        [UmbraDisplayName("Preview Theme")]
        [UmbraDescription("A dependent enum combo-box used to validate HideIf against a sibling boolean.")]
        [UmbraHideIf<bool>(nameof(ShowPreviewTheme), false)]
        [UmbraIndent]
        public Parameter<SampleTheme> PreviewTheme { get; set; } = new(SampleTheme.Neon);
    }

    /// <summary>
    /// Samples covering full custom parameter drawers, nested-group drawers, and custom button colors.
    /// </summary>
    [UmbraAutoRegister]
    public record CustomDrawerSamples
    {
        /// <summary>Gets or sets the full custom parameter-drawer sample.</summary>
        [UmbraParameter]
        [UmbraDisplayName("Visual Meter")]
        [UmbraDescription("Rendered by a full custom parameter drawer instead of the default two-column layout.")]
        [UmbraDrawer<NormalizedFloatPreviewDrawer>]
        public Parameter<float> VisualMeter { get; set; } = new(.42f);

        /// <summary>Gets or sets the sample click counter incremented by the custom-colored action button.</summary>
        [UmbraParameter]
        [UmbraDisplayName("Accent Button Clicks")]
        [UmbraDescription("A plain persisted value that makes the custom-colored button action observable.")]
        [UmbraRange(0, 999)]
        public Parameter<int> AccentButtonClicks { get; set; } = new(0);

        /// <summary>Gets or sets the custom-colored action button sample.</summary>
        [UmbraParameter]
        [UmbraDisplayName("Accent Action")]
        [UmbraDescription("Uses explicit custom RGBA button colors instead of a built-in button style.")]
        [UmbraCustomButtonColors(0.12f, 0.42f, 0.78f)]
        [UmbraControlWidth(-1f)]
        public Parameter<Action> AccentButton { get; init; }

        /// <summary>
        /// Gets or sets the first nested-group drawer sample.
        /// Its internal widgets intentionally reuse fixed local labels shared with the sibling sample.
        /// </summary>
        [UmbraParameter]
        [UmbraCategory("Nested Drawer")]
        [UmbraPrefix("primaryNestedDrawer")]
        [UmbraCollapseAsTree(true)]
        public NestedDrawerTest PrimaryNestedDrawer { get; set; } = new();

        /// <summary>
        /// Gets or sets the second nested-group drawer sample in the same category and parent scope.
        /// </summary>
        [UmbraParameter]
        [UmbraCategory("Nested Drawer")]
        [UmbraPrefix("secondaryNestedDrawer")]
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
    [UmbraAutoRegister]
    public record NestedTypeTests
    {
        /// <summary>Gets or sets the graphics branch used for local category-scoping tests.</summary>
        [UmbraParameter]
        [UmbraCategory("Graphics")]
        [UmbraPrefix("graphics")]
        [UmbraCollapseAsTree(true)]
        [UmbraParameterOrder(0)]
        public ScopedBranchConfig Graphics { get; set; } = new()
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
        [UmbraParameter]
        [UmbraCategory("Audio")]
        [UmbraPrefix("audio")]
        [UmbraCollapseAsTree(true)]
        [UmbraParameterOrder(1)]
        public ScopedBranchConfig Audio { get; set; } = new()
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
        [UmbraParameter]
        [UmbraPrefix("typeLevelFallback")]
        [UmbraParameterOrder(2)]
        public TypeLevelPresentationConfig TypeLevelFallback { get; set; } = new();

        /// <summary>
        /// Gets or sets the nested group whose property-level category overrides its type-level fallback metadata.
        /// </summary>
        [UmbraParameter]
        [UmbraCategory("Property Override")]
        [UmbraPrefix("propertyOverride")]
        [UmbraCollapseAsTree]
        [UmbraParameterOrder(3)]
        public TypeLevelPresentationConfig PropertyOverride { get; set; } = new()
        {
            SampleValue = new(77),
            Notes = new("Property-level category should win.")
        };

        /// <summary>
        /// Gets or sets the nested group that demonstrates type-level indentation and label-margin metadata.
        /// </summary>
        [UmbraParameter]
        [UmbraPrefix("indentedLayout")]
        [UmbraParameterOrder(4)]
        public IndentedLayoutConfig IndentedLayout { get; set; } = new();
    }

    /// <summary>
    /// Shared branch used by the nested-type tests to validate local category scoping and nested visibility.
    /// </summary>
    [UmbraAutoRegister]
    public record ScopedBranchConfig
    {
        /// <summary>Gets or sets whether this branch is enabled.</summary>
        [UmbraParameter]
        [UmbraDisplayName("Enabled")]
        [UmbraDescription("Whether this nested branch is active.")]
        [UmbraCategory("General")]
        public Parameter<bool> Enabled { get; set; } = new(true);

        /// <summary>Gets or sets whether the nested Advanced branch is visible.</summary>
        [UmbraParameter]
        [UmbraDisplayName("Show Advanced")]
        [UmbraDescription("Controls whether the nested Advanced branch below is rendered.")]
        [UmbraCategory("General")]
        [UmbraParameterOrder(0)]
        public Parameter<bool> ShowAdvanced { get; set; } = new(true);

        /// <summary>Gets or sets the branch intensity value.</summary>
        [UmbraParameter]
        [UmbraDisplayName("Intensity")]
        [UmbraDescription("A simple float used to validate nested ordering and local categories.")]
        [UmbraCategory("General")]
        [UmbraRange(0f, 1f)]
        [UmbraFormat("%.2f")]
        [UmbraParameterOrder(1)]
        public Parameter<float> Intensity { get; set; } = new(.50f);

        /// <summary>Gets or sets the nested Advanced branch.</summary>
        [UmbraParameter]
        [UmbraCategory("Advanced")]
        [UmbraPrefix("advanced")]
        [UmbraCollapseAsTree]
        [UmbraHideIf<bool>(nameof(ShowAdvanced), false)]
        [UmbraSpacingBefore]
        [UmbraSpacingAfter]
        [UmbraParameterOrder(2)]
        public ScopedAdvancedConfig Advanced { get; set; } = new();
    }

    /// <summary>
    /// Second-level nested branch used by the scoped-branch demo.
    /// </summary>
    [UmbraAutoRegister]
    public record ScopedAdvancedConfig
    {
        /// <summary>Gets or sets the advanced threshold value.</summary>
        [UmbraParameter]
        [UmbraDisplayName("Threshold")]
        [UmbraDescription("A nested integer rendered in the Tuning category.")]
        [UmbraCategory("Tuning")]
        [UmbraRange(0, 100)]
        public Parameter<int> Threshold { get; set; } = new(50);

        /// <summary>Gets or sets the advanced bias value.</summary>
        [UmbraParameter]
        [UmbraDisplayName("Bias")]
        [UmbraDescription("A second nested integer rendered alongside Threshold.")]
        [UmbraCategory("Tuning")]
        [UmbraRange(-10, 10)]
        public Parameter<int> Bias { get; set; } = new(0);

        /// <summary>Gets or sets the branch note text.</summary>
        [UmbraParameter]
        [UmbraDisplayName("Notes")]
        [UmbraDescription("A nested string rendered in a different local category.")]
        [UmbraCategory("Details")]
        [UmbraMaxLength(80)]
        public Parameter<string> Notes { get; set; } = new("Nested category names should remain local.");
    }

    /// <summary>
    /// Nested config group that declares its own presentation metadata at the type level.
    /// </summary>
    [UmbraAutoRegister]
    [UmbraCategory("Type-Level Fallback")]
    [UmbraCollapseAsTree]
    public record TypeLevelPresentationConfig
    {
        /// <summary>Gets or sets the sample numeric value rendered in the fallback group.</summary>
        [UmbraParameter]
        [UmbraDisplayName("Sample Value")]
        [UmbraDescription("A sample numeric value used to validate type-level presentation metadata fallback.")]
        [UmbraCategory("Values")]
        [UmbraRange(0, 100)]
        public Parameter<int> SampleValue { get; set; } = new(42);

        /// <summary>Gets or sets the sample note rendered in the fallback group.</summary>
        [UmbraParameter]
        [UmbraDisplayName("Notes")]
        [UmbraDescription("A sample note used to validate type-level category fallback.")]
        [UmbraCategory("Details")]
        [UmbraMaxLength(80)]
        public Parameter<string> Notes { get; set; } = new("Type-level category fallback.");
    }

    /// <summary>
    /// Nested config group that demonstrates class-level indentation and label-margin metadata.
    /// </summary>
    [UmbraAutoRegister]
    [UmbraCategory("Indented Layout")]
    [UmbraCollapseAsTree(true)]
    [UmbraIndent(18f)]
    [UmbraLabelMargin(16f)]
    public record IndentedLayoutConfig
    {
        /// <summary>Gets or sets the primary layout scale.</summary>
        [UmbraParameter]
        [UmbraDisplayName("Primary Scale")]
        [UmbraDescription("A ranged float rendered inside a class-level indented layout group.")]
        [UmbraRange(0.5f, 2.0f)]
        [UmbraFormat("%.2f")]
        public Parameter<float> PrimaryScale { get; set; } = new(1.00f);

        /// <summary>Gets or sets the secondary layout scale.</summary>
        [UmbraParameter]
        [UmbraDisplayName("Secondary Scale")]
        [UmbraDescription("An unconstrained float used to validate layout metadata on drag controls.")]
        [UmbraStep(0.05f)]
        [UmbraFormat("%.2f")]
        public Parameter<float> SecondaryScale { get; set; } = new(1.25f);

        /// <summary>Gets or sets free-form notes for the layout demo.</summary>
        [UmbraParameter]
        [UmbraDisplayName("Layout Notes")]
        [UmbraDescription("A multi-line string used to validate class-level label-margin behavior.")]
        [UmbraMultiline(3)]
        [UmbraMaxLength(160)]
        public Parameter<string> LayoutNotes { get; set; } = new("This group uses type-level indentation and extra label margin.");
    }

    /// <summary>
    /// Ranged integer slider samples.
    /// </summary>
    [UmbraAutoRegister]
    public record IntegerSliderSamples
    {
        /// <summary>Gets or sets the master volume slider sample.</summary>
        [UmbraParameter, UmbraDisplayName("Master Volume"), UmbraDescription("A ranged integer slider with a percentage-like value."), UmbraRange(0, 100)]
        public Parameter<int> MasterVolume { get; set; } = new(80);

        /// <summary>Gets or sets the retry count slider sample.</summary>
        [UmbraParameter, UmbraDisplayName("Retry Count"), UmbraDescription("A small ranged integer slider used for discrete step testing."), UmbraRange(0, 10)]
        public Parameter<int> RetryCount { get; set; } = new(3);

        /// <summary>Gets or sets the padding slider sample with a custom display format.</summary>
        [UmbraParameter, UmbraDisplayName("Padding"), UmbraDescription("A ranged integer slider with a custom integer display format."), UmbraRange(0, 64), UmbraFormat("%d px")]
        public Parameter<int> PaddingPixels { get; set; } = new(12);
    }

    /// <summary>
    /// Unconstrained integer drag samples.
    /// </summary>
    [UmbraAutoRegister]
    public record IntegerDragSamples
    {
        /// <summary>Gets or sets the horizontal offset drag sample.</summary>
        [UmbraParameter, UmbraDisplayName("Horizontal Offset"), UmbraDescription("An unconstrained integer drag sample."), UmbraStep(1)]
        public Parameter<int> HorizontalOffset { get; set; } = new(16);

        /// <summary>Gets or sets the vertical offset drag sample.</summary>
        [UmbraParameter, UmbraDisplayName("Vertical Offset"), UmbraDescription("A second unconstrained integer drag sample with a negative default."), UmbraStep(1)]
        public Parameter<int> VerticalOffset { get; set; } = new(-8);

        /// <summary>Gets or sets the priority bias drag sample.</summary>
        [UmbraParameter, UmbraDisplayName("Priority Bias"), UmbraDescription("An unconstrained integer drag sample with a larger drag step."), UmbraStep(5)]
        public Parameter<int> PriorityBias { get; set; } = new(25);
    }

    /// <summary>
    /// Ranged float slider samples.
    /// </summary>
    [UmbraAutoRegister]
    public record FloatSliderSamples
    {
        /// <summary>Gets or sets the opacity slider sample.</summary>
        [UmbraParameter, UmbraDisplayName("Opacity"), UmbraDescription("A normalized float slider."), UmbraRange(0f, 1f), UmbraFormat("%.2f")]
        public Parameter<float> Opacity { get; set; } = new(.65f);

        /// <summary>Gets or sets the gamma slider sample.</summary>
        [UmbraParameter, UmbraDisplayName("Gamma"), UmbraDescription("A wider float slider range for manual validation."), UmbraRange(0.5f, 3.0f), UmbraFormat("%.2f")]
        public Parameter<float> Gamma { get; set; } = new(1.20f);

        /// <summary>Gets or sets the exposure slider sample.</summary>
        [UmbraParameter, UmbraDisplayName("Exposure"), UmbraDescription("A float slider with one decimal place."), UmbraRange(-2f, 2f), UmbraFormat("%.1f")]
        public Parameter<float> Exposure { get; set; } = new(.5f);
    }

    /// <summary>
    /// Unconstrained float drag samples.
    /// </summary>
    [UmbraAutoRegister]
    public record FloatDragSamples
    {
        /// <summary>Gets or sets the move speed drag sample.</summary>
        [UmbraParameter, UmbraDisplayName("Move Speed"), UmbraDescription("An unconstrained float drag sample."), UmbraStep(0.05f), UmbraFormat("%.2f")]
        public Parameter<float> MoveSpeed { get; set; } = new(1.50f);

        /// <summary>Gets or sets the bloom strength drag sample.</summary>
        [UmbraParameter, UmbraDisplayName("Bloom Strength"), UmbraDescription("A smaller-step float drag sample."), UmbraStep(0.01f), UmbraFormat("%.2f")]
        public Parameter<float> BloomStrength { get; set; } = new(.35f);

        /// <summary>Gets or sets the camera lag drag sample.</summary>
        [UmbraParameter, UmbraDisplayName("Camera Lag"), UmbraDescription("A float drag sample with three decimal places."), UmbraStep(0.005f), UmbraFormat("%.3f")]
        public Parameter<float> CameraLag { get; set; } = new(.125f);
    }

    /// <summary>
    /// Ranged double slider samples.
    /// </summary>
    [UmbraAutoRegister]
    public record DoubleSliderSamples
    {
        /// <summary>Gets or sets the precision scale slider sample.</summary>
        [UmbraParameter, UmbraDisplayName("Precision Scale"), UmbraDescription("A ranged double slider used to validate native double-precision slider editing."), UmbraRange(0.0, 1.0), UmbraStep(0.001), UmbraFormat("%.3f")]
        public Parameter<double> PrecisionScale { get; set; } = new(0.125);

        /// <summary>Gets or sets the zoom factor slider sample.</summary>
        [UmbraParameter, UmbraDisplayName("Zoom Factor"), UmbraDescription("A second ranged double slider with a broader range."), UmbraRange(0.5, 4.0), UmbraStep(0.001), UmbraFormat("%.3f")]
        public Parameter<double> ZoomFactor { get; set; } = new(1.750);
    }

    /// <summary>
    /// Unconstrained double drag samples.
    /// </summary>
    [UmbraAutoRegister]
    public record DoubleDragSamples
    {
        /// <summary>Gets or sets the world offset drag sample.</summary>
        [UmbraParameter, UmbraDisplayName("World Offset"), UmbraDescription("An unconstrained double drag sample used to validate precision beyond float."), UmbraStep(0.125), UmbraFormat("%.3f")]
        public Parameter<double> WorldOffset { get; set; } = new(12.375);

        /// <summary>Gets or sets the calibration bias drag sample.</summary>
        [UmbraParameter, UmbraDisplayName("Calibration Bias"), UmbraDescription("A second unconstrained double drag sample with a small step size."), UmbraStep(0.0005), UmbraFormat("%.4f")]
        public Parameter<double> CalibrationBias { get; set; } = new(0.0025);
    }

    /// <summary>
    /// Single-line string samples.
    /// </summary>
    [UmbraAutoRegister]
    public record SingleLineStringSamples
    {
        /// <summary>Gets or sets the profile name text sample.</summary>
        [UmbraParameter, UmbraDisplayName("Profile Name"), UmbraDescription("A short single-line string sample with required and minimum-length validation."), UmbraRequired, UmbraMinLength(3), UmbraMaxLength(40)]
        public Parameter<string> ProfileName { get; set; } = new("Umbra Tester");

        /// <summary>Gets or sets the export directory text sample.</summary>
        [UmbraParameter, UmbraDisplayName("Export Directory"), UmbraDescription("A longer single-line string sample with required and regex validation."), UmbraRequired, UmbraRegex("^[A-Za-z0-9_./-]+$", Message = "Use letters, digits, underscores, dots, slashes, or dashes only."), UmbraMaxLength(120)]
        public Parameter<string> ExportDirectory { get; set; } = new("data/Umbra/SamplePlugin/exports");

        /// <summary>Gets or sets the search filter text sample with custom validation.</summary>
        [UmbraParameter, UmbraDisplayName("Search Filter"), UmbraDescription("A third single-line string sample that demonstrates required, minimum-length, regex, and custom-validator feedback."), UmbraRequired, UmbraMinLength(3), UmbraRegex("^[a-z0-9 _-]+$", Message = "Use lowercase letters, digits, spaces, underscores, or dashes only."), UmbraValidateWith<SearchFilterValidator>, UmbraMaxLength(60)]
        public Parameter<string> SearchFilter { get; set; } = new("player camera");
    }

    /// <summary>
    /// Multi-line string samples.
    /// </summary>
    [UmbraAutoRegister]
    public record MultilineStringSamples
    {
        /// <summary>Gets or sets the multi-line notes text sample.</summary>
        [UmbraParameter, UmbraDisplayName("Notes"), UmbraDescription("A short multi-line text sample that allows whitespace-only content while still rejecting null or empty text."), UmbraRequired(AllowWhitespace = true), UmbraMultiline(3), UmbraMaxLength(160)]
        public Parameter<string> Notes { get; set; } = new("Use this field to validate multi-line text persistence.");

        /// <summary>Gets or sets the multi-line changelog text sample.</summary>
        [UmbraParameter, UmbraDisplayName("Changelog"), UmbraDescription("A larger multi-line text sample with more visible rows."), UmbraMultiline(5), UmbraMaxLength(320)]
        public Parameter<string> Changelog { get; set; } = new("- Added data-type sample groups\n- Added nested type tests\n- Added custom drawer coverage");
    }

    /// <summary>
    /// Sample nested config group rendered by a custom nested-group drawer.
    /// Multiple instances of this type intentionally reuse the same local widget labels so the
    /// sample can validate nested-group ImGui ID scoping manually.
    /// </summary>
    [UmbraAutoRegister]
    [UmbraCategory("Drawer Test")]
    [UmbraCollapseAsTree]
    [UmbraNestedDrawer<NestedDrawerTestDrawer>]
    public record NestedDrawerTest
    {
        /// <summary>Gets or sets the first sample integer value for the nested drawer test.</summary>
        [UmbraParameter]
        public Parameter<int> Value1 { get; set; } = new(123);

        /// <summary>Gets or sets the second sample boolean value for the nested drawer test.</summary>
        [UmbraParameter]
        public Parameter<bool> Value2 { get; set; } = new(true);

        /// <summary>Gets or sets the third sample string value for the nested drawer test.</summary>
        [UmbraParameter]
        public Parameter<string> Value3 { get; set; } = new("Hello, world!");

        /// <summary>Gets or sets the fourth sample float value for the nested drawer test.</summary>
        [UmbraParameter]
        public Parameter<float> Value4 { get; set; } = new(3.14f);
    }
}
