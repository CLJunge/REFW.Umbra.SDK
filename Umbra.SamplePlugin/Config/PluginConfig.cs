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
/// <remarks>
/// This sample config intentionally exercises most of Umbra's settings metadata surface so the
/// generated panel can serve as a manual validation bed for nested groups, category scoping,
/// custom drawers, button actions, ordering, spacing, and visibility predicates.
/// </remarks>
[UmbraAutoRegisterSettings]
[UmbraConfigRootNode("Sample Plugin v1.0")]
[UmbraSettingsPrefix("samplePlugin")]
public record PluginConfig
{
    /// <summary>Gets or sets whether the Sample Plugin is active.</summary>
    [UmbraSettingsParameter]
    [UmbraDisplayName("Enabled")]
    [UmbraDescription("Whether the plugin is enabled or not.")]
    public Parameter<bool> IsEnabled { get; set; } = new(true);

    /// <summary>Gets or sets the hotkey used to toggle the plugin on and off.</summary>
    [UmbraSettingsParameter]
    [UmbraDisplayName("Toggle Hotkey")]
    [UmbraDescription("The hotkey to toggle the plugin on and off.")]
    [UmbraTwoColumnCustomDrawer<TwoColumnHotkeyDrawer>]
    public Parameter<int> ToggleHotkey { get; set; } = new(574); // F3 (ImGuiKey.F3)

    /// <summary>Gets or sets the hotkey used to switch between first-person and third-person view.</summary>
    [UmbraSettingsParameter]
    [UmbraDisplayName("Switch View Hotkey")]
    [UmbraDescription("The hotkey to switch between first and third person view.")]
    [UmbraTwoColumnCustomDrawer<TwoColumnHotkeyDrawer>]
    public Parameter<int> SwitchViewHotkey { get; set; } = new(575); // F4 (ImGuiKey.F4)

    /// <summary>Gets or sets the field-of-view settings group.</summary>
    [UmbraSettingsParameter]
    [UmbraCategory("FOV")]
    [UmbraSettingsPrefix("fov")]
    [UmbraCollapseAsTree]
    public FovSettings Fov { get; set; } = new();

    /// <summary>Gets or sets the film grain settings group.</summary>
    [UmbraSettingsParameter]
    [UmbraCategory("Film Grain")]
    [UmbraSettingsPrefix("filmGrain")]
    [UmbraCollapseAsTree]
    public FilmGrainSettings FilmGrain { get; set; } = new();

    /// <summary>
    /// Gets or sets the deep nested-group demo used to validate that nested groups can contain
    /// further nested groups, that category names are local to their parent context, and that a
    /// categorized nested group renders as a single visible container rather than repeating the
    /// same category label inside itself.
    /// </summary>
    [UmbraSettingsParameter]
    [UmbraCategory("Nested Groups")]
    [UmbraSettingsPrefix("nestedGroups")]
    [UmbraCollapseAsTree]
    public NestedGroupsDemo NestedGroups { get; set; } = new();

    /// <summary>Gets or sets the sample nested settings group rendered by a custom group drawer.</summary>
    /// <remarks>
    /// The nested type itself carries <see cref="UmbraNestedGroupDrawerAttribute{TDrawer}"/>, so
    /// this property demonstrates the type-level fallback path for custom nested-group drawers.
    /// </remarks>
    [UmbraSettingsParameter]
    public NestedDrawerTest DrawerTest { get; set; } = new();

    /// <summary>
    /// Logs a diagnostic test message to the REFramework console.
    /// Demonstrates <see cref="ButtonDrawer"/> with a primary style and full-width layout,
    /// and <see cref="UmbraParameterOrderAttribute"/> to pin this button above all other root-level settings.
    /// </summary>
    [UmbraSettingsParameter]
    [UmbraDisplayName("Log Test Message")]
    [UmbraDescription("Logs a test message to the REFramework console to verify the plugin is active.")]
    [UmbraCustomDrawer<ButtonDrawer>]
    [UmbraButtonStyle(ButtonStyle.Primary)]
    [UmbraControlWidth(-1f)]
    [UmbraParameterOrder(0)]
    public Parameter<Action> LogTestMessage { get; init; } = new(static () => { });

    /// <summary>Resets all General settings to their default values.</summary>
    [UmbraSettingsParameter]
    [UmbraDisplayName("Reset General")]
    [UmbraDescription("Resets the enabled toggle and hotkey bindings to their default values.")]
    [UmbraCustomDrawer<ButtonDrawer>]
    [UmbraButtonStyle(ButtonStyle.Danger)]
    [UmbraControlWidth(-1f)]
    public Parameter<Action> ResetGeneral { get; init; }

    /// <summary>Initializes a new <see cref="PluginConfig"/> and wires up the sample button actions.</summary>
    /// <remarks>
    /// Delegate-backed button parameters are configured in the constructor so each loaded config
    /// instance owns the actions that operate on its own <see cref="Parameter{T}"/> objects.
    /// </remarks>
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
    [UmbraAutoRegisterSettings]
    public record FovSettings
    {
        private const float _minFov = 10f;
        private const float _maxFov = 180f;

        /// <summary>Gets or sets the FOV angle used in third-person view.</summary>
        [UmbraSettingsParameter]
        [UmbraDisplayName("3rd Person")]
        [UmbraDescription("The FOV to use in third person.")]
        [UmbraRange(_minFov, _maxFov)]
        [UmbraFormat("%.1f")]
        public Parameter<float> Tps { get; set; } = new(55f);

        /// <summary>Gets or sets the FOV angle used when aiming down sights in third-person view.</summary>
        /// <remarks>Declared before <see cref="Fps"/> but rendered after it via <c>[UmbraParameterOrder(2)]</c>.</remarks>
        [UmbraSettingsParameter]
        [UmbraDisplayName("3rd Person ADS")]
        [UmbraDescription("The FOV to use when aiming down sights in third person.")]
        [UmbraRange(_minFov, _maxFov)]
        [UmbraFormat("%.1f")]
        [UmbraParameterOrder(2)]
        public Parameter<float> TpsAds { get; set; } = new(45f);

        /// <summary>Gets or sets the FOV angle used in first-person view.</summary>
        /// <remarks>Declared after <see cref="TpsAds"/> but rendered before it via <c>[UmbraParameterOrder(1)]</c>.</remarks>
        [UmbraSettingsParameter]
        [UmbraDisplayName("1st Person")]
        [UmbraDescription("The FOV to use in first person.")]
        [UmbraRange(_minFov, _maxFov)]
        [UmbraFormat("%.1f")]
        [UmbraParameterOrder(1)]
        public Parameter<float> Fps { get; set; } = new(70f);

        /// <summary>Gets or sets the FOV angle used when aiming down sights in first-person view.</summary>
        [UmbraSettingsParameter]
        [UmbraDisplayName("1st Person ADS")]
        [UmbraDescription("The FOV to use when aiming down sights in first person.")]
        [UmbraRange(_minFov, _maxFov)]
        [UmbraFormat("%.1f")]
        public Parameter<float> FpsAds { get; set; } = new(35f);

        /// <summary>Resets all FOV settings to their default values.</summary>
        [UmbraSettingsParameter]
        [UmbraDisplayName("Reset FOV")]
        [UmbraDescription("Resets all field-of-view values to their defaults.")]
        [UmbraCustomDrawer<ButtonDrawer>]
        [UmbraButtonStyle(ButtonStyle.Danger)]
        [UmbraControlWidth(-1f)]
        public Parameter<Action> ResetFov { get; init; }

        /// <summary>Initializes a new <see cref="FovSettings"/> and wires up the reset action.</summary>
        /// <remarks>
        /// The reset action restores every view-mode-specific FOV parameter so the nested group can
        /// be returned to a known baseline with a single button press.
        /// </remarks>
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
    [UmbraAutoRegisterSettings]
    public record FilmGrainSettings
    {
        /// <summary>Gets or sets whether the film grain post-processing effect is disabled entirely.</summary>
        [UmbraSettingsParameter]
        [UmbraDisplayName("Disable Film Grain")]
        [UmbraDescription("Whether the film grain effect is disabled or not.")]
        public Parameter<bool> Disabled { get; set; } = new(true);

        /// <summary>
        /// Gets or sets the opacity of the film grain effect.
        /// Hidden in the UI while <see cref="Disabled"/> is <see langword="true"/>.
        /// </summary>
        [UmbraSettingsParameter]
        [UmbraDisplayName("Opacity")]
        [UmbraDescription("The opacity of the film grain effect. 0 = no effect, 1 = full effect.")]
        [UmbraRange(0f, 1f)]
        [UmbraFormat("%.2f")]
        [UmbraHideIf<bool>(nameof(Disabled), true)]
        public Parameter<float> Opacity { get; set; } = new(.15f);

        /// <summary>Resets all film grain settings to their default values.</summary>
        [UmbraSettingsParameter]
        [UmbraDisplayName("Reset Film Grain")]
        [UmbraDescription("Resets the film grain toggle and opacity to their defaults.")]
        [UmbraCustomDrawer<ButtonDrawer>]
        [UmbraButtonStyle(ButtonStyle.Danger)]
        [UmbraControlWidth(-1f)]
        public Parameter<Action> ResetFilmGrain { get; init; }

        /// <summary>Initializes a new <see cref="FilmGrainSettings"/> and wires up the reset action.</summary>
        /// <remarks>
        /// The action resets both the visibility-driving toggle and the dependent opacity value so
        /// the hide predicate can be revalidated from a clean default state.
        /// </remarks>
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
    [UmbraAutoRegisterSettings]
    public record NestedGroupsDemo
    {
        /// <summary>Gets or sets the graphics branch for the nested-group demo.</summary>
        [UmbraSettingsParameter]
        [UmbraCategory("Graphics")]
        [UmbraSettingsPrefix("graphics")]
        [UmbraCollapseAsTree]
        [UmbraParameterOrder(0)]
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
        [UmbraSettingsParameter]
        [UmbraCategory("Audio")]
        [UmbraSettingsPrefix("audio")]
        [UmbraCollapseAsTree]
        [UmbraParameterOrder(1)]
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
        [UmbraSettingsParameter]
        [UmbraSettingsPrefix("typeFallback")]
        [UmbraParameterOrder(2)]
        public TypeLevelCategorySettings TypeLevelFallback { get; set; } = new();

        /// <summary>
        /// Gets or sets the property-level category override branch.
        /// This validates that property-level presentation metadata wins over the nested type's
        /// fallback metadata.
        /// </summary>
        [UmbraSettingsParameter]
        [UmbraCategory("Property Override")]
        [UmbraSettingsPrefix("propertyOverride")]
        [UmbraCollapseAsTree]
        [UmbraParameterOrder(3)]
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
        [UmbraSettingsParameter]
        [UmbraCategory("Drawer Inside Tree")]
        [UmbraSettingsPrefix("drawerInsideTree")]
        [UmbraCollapseAsTree]
        [UmbraParameterOrder(4)]
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
        [UmbraSettingsParameter]
        [UmbraCategory("Drawer Inside Tree")]
        [UmbraSettingsPrefix("drawerInsideTreeDuplicate")]
        [UmbraCollapseAsTree]
        [UmbraParameterOrder(5)]
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
    [UmbraAutoRegisterSettings]
    public record BranchSettings
    {
        /// <summary>Gets or sets whether this branch is enabled.</summary>
        [UmbraSettingsParameter]
        [UmbraDisplayName("Enabled")]
        [UmbraDescription("Whether this demo branch is active.")]
        [UmbraCategory("General")]
        public Parameter<bool> Enabled { get; set; } = new(true);

        /// <summary>
        /// Gets or sets whether the nested Advanced branch is shown.
        /// This validates <see cref="HideIfAttribute{T}"/> on a nested-group property.
        /// </summary>
        [UmbraSettingsParameter]
        [UmbraDisplayName("Show Advanced")]
        [UmbraDescription("Controls whether the nested Advanced group is visible.")]
        [UmbraCategory("General")]
        [UmbraParameterOrder(0)]
        public Parameter<bool> ShowAdvanced { get; set; } = new(true);

        /// <summary>Gets or sets the intensity value shown in the branch's General category.</summary>
        [UmbraSettingsParameter]
        [UmbraDisplayName("Intensity")]
        [UmbraDescription("A simple value used to verify ordering and rendering inside the branch.")]
        [UmbraCategory("General")]
        [UmbraRange(0f, 1f)]
        [UmbraFormat("%.2f")]
        [UmbraParameterOrder(1)]
        public Parameter<float> Intensity { get; set; } = new(.50f);

        /// <summary>
        /// Gets or sets the nested advanced branch, reusing the same category name across sibling
        /// parent groups to validate local category scoping.
        /// </summary>
        [UmbraSettingsParameter]
        [UmbraCategory("Advanced")]
        [UmbraSettingsPrefix("advanced")]
        [UmbraCollapseAsTree]
        [UmbraHideIf<bool>(nameof(ShowAdvanced), false)]
        [UmbraSpacingBefore]
        [UmbraSpacingAfter]
        [UmbraParameterOrder(2)]
        public AdvancedBranchSettings Advanced { get; set; } = new();

        /// <summary>
        /// Gets or sets the type-level category fallback child branch inside this branch.
        /// This validates that child groups using type-level metadata can coexist with explicit
        /// property-level category branches in the same parent scope.
        /// </summary>
        [UmbraSettingsParameter]
        [UmbraSettingsPrefix("typeFallback")]
        [UmbraParameterOrder(3)]
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
        [UmbraSettingsParameter]
        [UmbraCategory("Drawer Test")]
        [UmbraSettingsPrefix("drawer")]
        [UmbraCollapseAsTree]
        [UmbraParameterOrder(4)]
        public NestedDrawerTest Drawer { get; set; } = new();

        /// <summary>
        /// Gets or sets the second nested custom-drawer sample inside this branch.
        /// This validates that sibling custom drawers in the same parent scope can safely reuse the
        /// same internal widget labels because each nested group subtree has its own ImGui ID scope.
        /// </summary>
        [UmbraSettingsParameter]
        [UmbraCategory("Drawer Test")]
        [UmbraSettingsPrefix("drawerDuplicate")]
        [UmbraCollapseAsTree]
        [UmbraParameterOrder(5)]
        public NestedDrawerTest DrawerDuplicate { get; set; } = new();
    }

    /// <summary>
    /// Innermost nested settings group in the demo, providing a second nesting level beneath the
    /// branch-specific Advanced category.
    /// </summary>
    [UmbraAutoRegisterSettings]
    public record AdvancedBranchSettings
    {
        /// <summary>
        /// Gets or sets the threshold value rendered inside the nested tuning category.
        /// Defaults differ between the demo's Graphics and Audio branches so they are easy to tell apart.
        /// </summary>
        [UmbraSettingsParameter]
        [UmbraDisplayName("Threshold")]
        [UmbraDescription("A nested numeric setting used to verify deep category scoping.")]
        [UmbraCategory("Tuning")]
        [UmbraRange(0, 100)]
        public Parameter<int> Threshold { get; set; } = new(50);

        /// <summary>
        /// Gets or sets the bias value rendered alongside <see cref="Threshold"/>.
        /// Defaults differ between the demo's Graphics and Audio branches so they are easy to tell apart.
        /// </summary>
        [UmbraSettingsParameter]
        [UmbraDisplayName("Bias")]
        [UmbraDescription("A second nested numeric setting used to verify deep category scoping.")]
        [UmbraCategory("Tuning")]
        [UmbraRange(-10, 10)]
        public Parameter<int> Bias { get; set; } = new(0);

        /// <summary>
        /// Gets or sets a note shown in the nested details category.
        /// Defaults differ between the demo's Graphics and Audio branches so they are easy to tell apart.
        /// </summary>
        [UmbraSettingsParameter]
        [UmbraDisplayName("Notes")]
        [UmbraDescription("Free-form text for verifying a second inner category under the same nested group.")]
        [UmbraCategory("Details")]
        [UmbraMaxLength(80)]
        public Parameter<string> Notes { get; set; } = new("Shared category names should stay local.");
    }

    /// <summary>
    /// Nested settings group that declares its own presentation metadata at the type level.
    /// Used to validate fallback behavior when the parent property supplies no category override.
    /// </summary>
    [UmbraAutoRegisterSettings]
    [UmbraCategory("Type-Level Fallback")]
    [UmbraCollapseAsTree]
    public record TypeLevelCategorySettings
    {
        /// <summary>Gets or sets the sample numeric value rendered in the fallback group.</summary>
        [UmbraSettingsParameter]
        [UmbraDisplayName("Value")]
        [UmbraDescription("A sample value used to verify type-level fallback presentation metadata.")]
        [UmbraCategory("Fallback Values")]
        [UmbraRange(0, 100)]
        public Parameter<int> Value { get; set; } = new(42);

        /// <summary>Gets or sets the sample note rendered in the fallback group.</summary>
        [UmbraSettingsParameter]
        [UmbraDisplayName("Notes")]
        [UmbraDescription("A note used to verify the fallback group renders as a scoped nested container.")]
        [UmbraCategory("Fallback Details")]
        [UmbraMaxLength(80)]
        public Parameter<string> Notes { get; set; } = new("Type-level category fallback.");
    }

    /// <summary>
    /// Sample nested settings group used to demonstrate a custom nested group drawer.
    /// The properties are modeled as <see cref="Parameter{T}"/> so they participate
    /// in the standard Umbra settings registration and persistence workflow. Multiple instances of
    /// this type are intentionally rendered in the sample config with identical internal widget
    /// labels so nested-group ImGui ID scoping can be verified manually.
    /// </summary>
    /// <remarks>
    /// The group is annotated with <see cref="UmbraNestedGroupDrawerAttribute{TDrawer}"/>, causing
    /// <see cref="NestedDrawerTestDrawer"/> to take over layout for every instance unless a parent
    /// property supplies its own overriding nested-group drawer attribute.
    /// </remarks>
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
