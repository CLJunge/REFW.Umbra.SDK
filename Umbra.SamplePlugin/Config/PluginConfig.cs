using Umbra.Config;
using Umbra.Config.Attributes;
using Umbra.UI.Config.Drawers;

namespace Umbra.SamplePlugin.Config;

/// <summary>
/// Root configuration record for the sample plugin.
/// Contains the enable toggle, hotkey bindings, nested settings groups for field-of-view and
/// film grain adjustments, a deep nested-group demo that reuses category names in separate parent
/// scopes, a custom nested-group drawer demo, and action-backed button parameters.
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
    [Category("FOV")]
    [SettingsPrefix("fov")]
    [CollapseAsTree]
    public FovSettings Fov { get; set; } = new();

    /// <summary>Gets or sets the film grain settings group.</summary>
    [SettingsParameter]
    [Category("Film Grain")]
    [SettingsPrefix("filmGrain")]
    [CollapseAsTree]
    public FilmGrainSettings FilmGrain { get; set; } = new();

    /// <summary>
    /// Gets or sets the deep nested-group demo used to validate that nested groups can contain
    /// further nested groups, that category names are local to their parent context, and that a
    /// categorized nested group renders as a single visible container rather than repeating the
    /// same category label inside itself.
    /// </summary>
    [SettingsParameter]
    [Category("Nested Groups")]
    [SettingsPrefix("nestedGroups")]
    [CollapseAsTree]
    public NestedGroupsDemo NestedGroups { get; set; } = new();

    /// <summary>Gets or sets the sample nested settings group rendered by a custom group drawer.</summary>
    [SettingsParameter]
    public NestedDrawerTest DrawerTest { get; set; } = new();

    /// <summary>
    /// Logs a diagnostic test message to the REFramework console.
    /// Demonstrates <see cref="ButtonDrawer"/> with a primary style and full-width layout,
    /// and <see cref="ParameterOrderAttribute"/> to pin this button above all other root-level settings.
    /// </summary>
    [SettingsParameter]
    [DisplayName("Log Test Message")]
    [Description("Logs a test message to the REFramework console to verify the plugin is active.")]
    [CustomDrawer<ButtonDrawer>]
    [ButtonStyle(ButtonStyle.Primary)]
    [ControlWidth(-1f)]
    [ParameterOrder(0)]
    public Parameter<Action> LogTestMessage { get; init; } = new(static () => { });

    /// <summary>Resets all General settings to their default values.</summary>
    [SettingsParameter]
    [DisplayName("Reset General")]
    [Description("Resets the enabled toggle and hotkey bindings to their default values.")]
    [CustomDrawer<ButtonDrawer>]
    [ButtonStyle(ButtonStyle.Danger)]
    [ControlWidth(-1f)]
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
        /// <remarks>Declared before <see cref="Fps"/> but rendered after it via <c>[ParameterOrder(2)]</c>.</remarks>
        [SettingsParameter]
        [DisplayName("3rd Person ADS")]
        [Description("The FOV to use when aiming down sights in third person.")]
        [Range(_minFov, _maxFov)]
        [Format("%.1f")]
        [ParameterOrder(2)]
        public Parameter<float> TpsAds { get; set; } = new(45f);

        /// <summary>Gets or sets the FOV angle used in first-person view.</summary>
        /// <remarks>Declared after <see cref="TpsAds"/> but rendered before it via <c>[ParameterOrder(1)]</c>.</remarks>
        [SettingsParameter]
        [DisplayName("1st Person")]
        [Description("The FOV to use in first person.")]
        [Range(_minFov, _maxFov)]
        [Format("%.1f")]
        [ParameterOrder(1)]
        public Parameter<float> Fps { get; set; } = new(70f);

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
        [ControlWidth(-1f)]
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
        [ControlWidth(-1f)]
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

    /// <summary>
    /// Deep nested settings demo that provides two sibling branches with identical inner category
    /// names so the sample can verify that categories are scoped to their owning parent group and
    /// that each categorized nested group renders a single container for its direct controls and
    /// child categories.
    /// The branches and nested custom drawers intentionally use different default values so the
    /// rendered hierarchy and repeated custom-drawer instances are easy to distinguish visually
    /// during manual validation.
    /// </summary>
    [AutoRegisterSettings]
    public record NestedGroupsDemo
    {
        /// <summary>Gets or sets the graphics branch for the nested-group demo.</summary>
        [SettingsParameter]
        [Category("Graphics")]
        [SettingsPrefix("graphics")]
        [CollapseAsTree]
        [ParameterOrder(0)]
        public BranchSettings Graphics { get; set; } = new()
        {
            Intensity = new(.25f),
            Advanced = new()
            {
                Threshold = new(20),
                Bias = new(-3),
                Notes = new("Graphics branch")
            },
            Drawer = new()
            {
                Value1 = new(101),
                Value2 = new(true),
                Value3 = new("Graphics drawer A"),
                Value4 = new(1.01f)
            },
            DrawerDuplicate = new()
            {
                Value1 = new(102),
                Value2 = new(false),
                Value3 = new("Graphics drawer B"),
                Value4 = new(1.02f)
            }
        };

        /// <summary>Gets or sets the audio branch for the nested-group demo.</summary>
        [SettingsParameter]
        [Category("Audio")]
        [SettingsPrefix("audio")]
        [CollapseAsTree]
        [ParameterOrder(1)]
        public BranchSettings Audio { get; set; } = new()
        {
            Enabled = new(false),
            ShowAdvanced = new(false),
            Intensity = new(.80f),
            Advanced = new()
            {
                Threshold = new(75),
                Bias = new(4),
                Notes = new("Audio branch")
            },
            Drawer = new()
            {
                Value1 = new(201),
                Value2 = new(true),
                Value3 = new("Audio drawer A"),
                Value4 = new(2.01f)
            },
            DrawerDuplicate = new()
            {
                Value1 = new(202),
                Value2 = new(false),
                Value3 = new("Audio drawer B"),
                Value4 = new(2.02f)
            }
        };

        /// <summary>
        /// Gets or sets the type-level category fallback branch.
        /// This validates that a nested group's type-level presentation metadata is used when the
        /// parent property does not override it.
        /// </summary>
        [SettingsParameter]
        [SettingsPrefix("typeFallback")]
        [ParameterOrder(2)]
        public TypeLevelCategorySettings TypeLevelFallback { get; set; } = new();

        /// <summary>
        /// Gets or sets the property-level category override branch.
        /// This validates that property-level presentation metadata wins over the nested type's
        /// fallback metadata.
        /// </summary>
        [SettingsParameter]
        [Category("Property Override")]
        [SettingsPrefix("propertyOverride")]
        [CollapseAsTree]
        [ParameterOrder(3)]
        public TypeLevelCategorySettings PropertyOverride { get; set; } = new()
        {
            Value = new(77),
            Notes = new("Property category should win.")
        };

        /// <summary>
        /// Gets or sets the nested custom-drawer branch inside the deep tree.
        /// This validates that a nested group using <see cref="NestedGroupDrawerAttribute{TDrawer}"/>
        /// can participate in the same scoped layout as regular nested groups.
        /// </summary>
        [SettingsParameter]
        [Category("Drawer Inside Tree")]
        [SettingsPrefix("drawerInsideTree")]
        [CollapseAsTree]
        [ParameterOrder(4)]
        public NestedDrawerTest DrawerInsideTree { get; set; } = new()
        {
            Value1 = new(301),
            Value2 = new(true),
            Value3 = new("Root drawer A"),
            Value4 = new(3.01f)
        };

        /// <summary>
        /// Gets or sets a second nested custom-drawer branch in the same parent scope and category.
        /// This validates that sibling custom drawers reusing the same internal widget labels do
        /// not collide when each nested group has its own ImGui ID scope.
        /// </summary>
        [SettingsParameter]
        [Category("Drawer Inside Tree")]
        [SettingsPrefix("drawerInsideTreeDuplicate")]
        [CollapseAsTree]
        [ParameterOrder(5)]
        public NestedDrawerTest DrawerInsideTreeDuplicate { get; set; } = new()
        {
            Value1 = new(302),
            Value2 = new(false),
            Value3 = new("Root drawer B"),
            Value4 = new(3.02f)
        };
    }

    /// <summary>
    /// Shared nested branch used by multiple parent groups in the demo to prove that categories
    /// with the same label remain independent when they live under different parent scopes.
    /// </summary>
    [AutoRegisterSettings]
    public record BranchSettings
    {
        /// <summary>Gets or sets whether this branch is enabled.</summary>
        [SettingsParameter]
        [DisplayName("Enabled")]
        [Description("Whether this demo branch is active.")]
        [Category("General")]
        public Parameter<bool> Enabled { get; set; } = new(true);

        /// <summary>
        /// Gets or sets whether the nested Advanced branch is shown.
        /// This validates <see cref="HideIfAttribute{T}"/> on a nested-group property.
        /// </summary>
        [SettingsParameter]
        [DisplayName("Show Advanced")]
        [Description("Controls whether the nested Advanced group is visible.")]
        [Category("General")]
        [ParameterOrder(0)]
        public Parameter<bool> ShowAdvanced { get; set; } = new(true);

        /// <summary>Gets or sets the intensity value shown in the branch's General category.</summary>
        [SettingsParameter]
        [DisplayName("Intensity")]
        [Description("A simple value used to verify ordering and rendering inside the branch.")]
        [Category("General")]
        [Range(0f, 1f)]
        [Format("%.2f")]
        [ParameterOrder(1)]
        public Parameter<float> Intensity { get; set; } = new(.50f);

        /// <summary>
        /// Gets or sets the nested advanced branch, reusing the same category name across sibling
        /// parent groups to validate local category scoping.
        /// </summary>
        [SettingsParameter]
        [Category("Advanced")]
        [SettingsPrefix("advanced")]
        [CollapseAsTree]
        [HideIf<bool>(nameof(ShowAdvanced), false)]
        [SpacingBefore]
        [SpacingAfter]
        [ParameterOrder(2)]
        public AdvancedBranchSettings Advanced { get; set; } = new();

        /// <summary>
        /// Gets or sets the type-level category fallback child branch inside this branch.
        /// This validates that child groups using type-level metadata can coexist with explicit
        /// property-level category branches in the same parent scope.
        /// </summary>
        [SettingsParameter]
        [SettingsPrefix("typeFallback")]
        [ParameterOrder(3)]
        public TypeLevelCategorySettings TypeLevelFallback { get; set; } = new()
        {
            Value = new(12),
            Notes = new("Branch-local fallback")
        };

        /// <summary>
        /// Gets or sets the first nested custom-drawer sample inside this branch.
        /// This validates that custom nested-group drawers compose with category scoping and
        /// property-level ordering metadata.
        /// </summary>
        [SettingsParameter]
        [Category("Drawer Test")]
        [SettingsPrefix("drawer")]
        [CollapseAsTree]
        [ParameterOrder(4)]
        public NestedDrawerTest Drawer { get; set; } = new();

        /// <summary>
        /// Gets or sets the second nested custom-drawer sample inside this branch.
        /// This validates that sibling custom drawers in the same parent scope can safely reuse the
        /// same internal widget labels because each nested group subtree has its own ImGui ID scope.
        /// </summary>
        [SettingsParameter]
        [Category("Drawer Test")]
        [SettingsPrefix("drawerDuplicate")]
        [CollapseAsTree]
        [ParameterOrder(5)]
        public NestedDrawerTest DrawerDuplicate { get; set; } = new();
    }

    /// <summary>
    /// Innermost nested settings group in the demo, providing a second nesting level beneath the
    /// branch-specific Advanced category.
    /// </summary>
    [AutoRegisterSettings]
    public record AdvancedBranchSettings
    {
        /// <summary>
        /// Gets or sets the threshold value rendered inside the nested tuning category.
        /// Defaults differ between the demo's Graphics and Audio branches so they are easy to tell apart.
        /// </summary>
        [SettingsParameter]
        [DisplayName("Threshold")]
        [Description("A nested numeric setting used to verify deep category scoping.")]
        [Category("Tuning")]
        [Range(0, 100)]
        public Parameter<int> Threshold { get; set; } = new(50);

        /// <summary>
        /// Gets or sets the bias value rendered alongside <see cref="Threshold"/>.
        /// Defaults differ between the demo's Graphics and Audio branches so they are easy to tell apart.
        /// </summary>
        [SettingsParameter]
        [DisplayName("Bias")]
        [Description("A second nested numeric setting used to verify deep category scoping.")]
        [Category("Tuning")]
        [Range(-10, 10)]
        public Parameter<int> Bias { get; set; } = new(0);

        /// <summary>
        /// Gets or sets a note shown in the nested details category.
        /// Defaults differ between the demo's Graphics and Audio branches so they are easy to tell apart.
        /// </summary>
        [SettingsParameter]
        [DisplayName("Notes")]
        [Description("Free-form text for verifying a second inner category under the same nested group.")]
        [Category("Details")]
        [MaxLength(80)]
        public Parameter<string> Notes { get; set; } = new("Shared category names should stay local.");
    }

    /// <summary>
    /// Nested settings group that declares its own presentation metadata at the type level.
    /// Used to validate fallback behavior when the parent property supplies no category override.
    /// </summary>
    [AutoRegisterSettings]
    [Category("Type-Level Fallback")]
    [CollapseAsTree]
    public record TypeLevelCategorySettings
    {
        /// <summary>Gets or sets the sample numeric value rendered in the fallback group.</summary>
        [SettingsParameter]
        [DisplayName("Value")]
        [Description("A sample value used to verify type-level fallback presentation metadata.")]
        [Category("Fallback Values")]
        [Range(0, 100)]
        public Parameter<int> Value { get; set; } = new(42);

        /// <summary>Gets or sets the sample note rendered in the fallback group.</summary>
        [SettingsParameter]
        [DisplayName("Notes")]
        [Description("A note used to verify the fallback group renders as a scoped nested container.")]
        [Category("Fallback Details")]
        [MaxLength(80)]
        public Parameter<string> Notes { get; set; } = new("Type-level category fallback.");
    }

    /// <summary>
    /// Sample nested settings group used to demonstrate a custom nested group drawer.
    /// The properties are modeled as <see cref="Parameter{T}"/> so they participate
    /// in the standard Umbra settings registration and persistence workflow. Multiple instances of
    /// this type are intentionally rendered in the sample config with identical internal widget
    /// labels so nested-group ImGui ID scoping can be verified manually.
    /// </summary>
    [AutoRegisterSettings]
    [Category("Drawer Test")]
    [CollapseAsTree]
    [NestedGroupDrawer<NestedDrawerTestDrawer>]
    public record NestedDrawerTest
    {
        /// <summary>Gets or sets the first sample integer value for the nested drawer test.</summary>
        [SettingsParameter]
        public Parameter<int> Value1 { get; set; } = new(123);

        /// <summary>Gets or sets the second sample boolean value for the nested drawer test.</summary>
        [SettingsParameter]
        public Parameter<bool> Value2 { get; set; } = new(true);

        /// <summary>Gets or sets the third sample string value for the nested drawer test.</summary>
        [SettingsParameter]
        public Parameter<string> Value3 { get; set; } = new("Hello, world!");

        /// <summary>Gets or sets the fourth sample float value for the nested drawer test.</summary>
        [SettingsParameter]
        public Parameter<float> Value4 { get; set; } = new(3.14f);
    }
}
