## Pi-Dev Utilities

**A collection of modular, high-quality utilities for Unity development.**\
All files are licensed under the MIT License unless otherwise specified.

### 📦 Installation

- Download or clone the repository
- Copy the entire *Assets/PiDev* folder into your Unity project's *Assets* directory.
- You now have full access to all utilities, components, and editor tools.
- Some utilities depend on DoTween by Demigiant. A version of it is included in Assets/DOTween. Make sure to manually update this when necessary, its provided only for the example project to work.
- **Unity 2021.3 LTS** or newer is recommended. *(Older versions may work but are not officially tested.)*

### 🗒️ Note about Examples

Pi-Dev Utilities is intended to include proper usage examples, but at the moment, a full sample package isn't available. Most utilities and components are straightforward to use, but I'm aware that a few — particularly the Adaptive UI tools — can feel complex or underdocumented.

In the future, I plan to add sample scenes to better demonstrate these tools in action, especially the Adaptive UI components.  

Thanks for your patience and understanding!


## 📁 Categories and Utilities
<details>
<summary>⚙️ Core - mostly C# extension methods and other utils to ease development.
</summary>

- **[CollectionUtils.cs](Assets/PiDev/Utilities/Core/CollectionUtils.cs)**  
  Extension methods for safer and more convenient list, array, and collection operations.
  ```cs
    array.GetOrDefault(index, defaultValue);
    list.GetOrDefault(index, defaultValue);
    dictionary.GetOrDefault(key, fallbackValue);
    dictionary.GetOrNull(key);
    weightedList.GetByWeight(); // ICollection<Utils.Weighted<T>>
  ```

- **[FindUtils.cs](Assets/PiDev/Utilities/Core/FindUtils.cs)**  
  Fast searching utilities for finding nearest objects, random objects, or tagged objects.
  ```cs
    transform.FindGrandChild("ChildName");
    Utils.GetClosestPoint(origin, listOfPoints);
    Utils.GetClosestNumber(originValue, floatValues);
    Utils.GetClosestObject(origin, objects); // GameObjects or Transforms
    Utils.GetClosestObjectWithTag(origin, "Enemy");
    Utils.GetAllComponents<UnityEngine.UI.Image>();
    Utils.GetClosestObjectWithComponent<MyComponent>(origin);
    Utils.GetClosestComponent<MyComponent>(origin);
    Utils.GetClosestComponent(origin, allowedList);
    Utils.GetClosestObjectImplementingInterface<IMyInterface>(origin, 10f, excludeList);
  ```

- **[ImporterUtils.cs](Assets/PiDev/Utilities/Core/ImporterUtils.cs)**  
  Scripts for reimporting assets and automatically applying settings like UI Sprite import mode.
  ```cs
    ImporterUtils.ImportAsUITexture("Assets/Path/To/Texture.png");
  ```

- **[MathUtils.cs](Assets/PiDev/Utilities/Core/MathUtils.cs)**  
  A collection of mathematical utilities and vector extensions for common Unity tasks.
  Includes remapping, damping, snapping, rounding, component-wise vector operations,
  circular lerp, angle snapping, and coordinate conversion helpers.
  
  Most of these are extension methods to basic primitives and Unity types. 
  ```cs
    value.RemapRanges(oldMin, oldMax, newMin, newMax);
    pos = pos.Damp(target, smoothing, deltaTime);
    angle = Utils.clerp(startAngle, endAngle, t);
    vec = vec.RoundMemberwise(); // Also: FloorMemberwise(), CeilMemberwise(), AbsMemberwise()
    float snapped = value.Snap(0.5f); // or angle = SnapAngleDeg(angle, 45f);
    v3 = v2.xy0(); v2 = v3.xz(); max = vec3.Max3();
  ```

- **[MiscUtils.cs](Assets/PiDev/Utilities/Core/MiscUtils.cs)**  
  Miscellaneous helpers including layer checking, hashing & others.
  ```cs
    var mc = gameObject.GetOrAddComponent<MyComponent>();
    gameObject.SetLayerRecursively(targetLayer);
    if (layer.IsInLayerMask(mask)) { ... }
    if (string.ContainsAny("check", "a", "b", "c")) { ... }
    myList.Each(item => Debug.Log(item));
  ```

- **[RandomUtils.cs](Assets/PiDev/Utilities/Core/RandomUtils.cs)**  
  Random selection helpers for arrays, lists, and weighted random picks.
  ```cs
    var item = Utils.Choose("A", "B", "C");
    var fromList = myList.GetRandomElement();
    var weighted = items.GetRandomElementByWeight(i => i.weight);
    float value = rangeVector2.RandomRange(); // rangeVector2 = new Vector2(min, max)
  ```
  
- **[ReflectionUtils.cs](Assets/PiDev/Utilities/Core/ReflectionUtils.cs)**  
  Access and manipulate private or public fields using reflection.
  ```cs
    ReflectionUtils.SetFieldValue(target, "fieldName", newValue);
    var value = ReflectionUtils.GetFieldValue(target, "fieldName");
    ReflectionUtils.CopyPublicMembers(sourceComponent, targetComponent);
  ```  
  
- **[SplineUtils.cs](Assets/PiDev/Utilities/Core/SplineUtils.cs)**  
  Generate and evaluate Catmull-Rom splines for smooth path interpolation.
  ```cs
    var points = PathSplineCatmullRom(controlPoints, loop, useLengths, resolution: 10)
  ``` 

- **[TimeUtils.cs](Assets/PiDev/Utilities/Core/TimeUtils.cs)**  
  Unix timestamp conversions for DateTime.
  ```cs
    long seconds = myDateTime.ToUnixTimeSeconds();
    long millis = myDateTime.ToUnixTimeMS();
  ```
  
- **[TransformUtils.cs](Assets/PiDev/Utilities/Core/TransformUtils.cs)**  
  Useful extension methods for manipulating Transforms.
  ```cs
    Rect screenRect = Utils.RectTransformToScreenSpace(myRectTransform);
  ```
  
- **[UnicodeUtils.cs](Assets/PiDev/Utilities/Core/UnicodeUtils.cs)**  
  Utilities for working with Unicode text, especially "fancy" or stylized mathematical characters.
  Provides normalization to ASCII, identification of stylized letters, and surrogate pair handling.
  Useful for cleaning up user input, chat systems, or data normalization in multilingual contexts.
  ```cs
    string clean = someString.NormalizeLeetText();
    bool isFancy = Utils.IsMathematicalLetter(codePoint);
    char ascii = Utils.ConvertToAsciiEquivalent(codePoint);
  ```
  
- **[Interfaces.cs](Assets/PiDev/Utilities/Core/Interfaces.cs)**  
  Core interfaces for standardizing common behaviors across utilities.

</details>

---

<details>
<summary>🎵 Audio - Sound banks & immersive, responsive audio behaviors without heavy scripting</summary>

- **[SoundBankSet.cs](Assets/PiDev/Utilities/Audio/SoundBankSet.cs)**  
  Configurable sound bank utility for playing random spatial and 2D audio clips 
  with randomized pitch and playback logic.
  Supports single-play and looping sounds with spatial blend, falloff settings, 
  and optional clip shuffling.
  Ideal for sound effects, ambient audio systems, or dynamic audio behavior in Unity.
  ```cs
    [SerializeField] SoundBankSet soundBank;
    soundBank.Play(position);
    soundBank.Play2D();
    soundBank.PlayLooping(position);
    soundBank.PlayLooping(followTransform);
  ```

- **[SoundBankSetHolder.cs](Assets/PiDev/Utilities/Audio/SoundBankSetHolder.cs)**  
  Component wrapper for easy use of a `SoundBankSet` with auto-play and filtering options.
  Supports optional low-pass filtering and radius visualization for editor debugging.
  Can delay playback on start and control sound lifecycle via Play/Stop.
  
  Attach to a GameObject, assign all clips and settings, and call Play() or enable PlayOnStart.
  Use 'useLowPassFilter' to add AudioLowPassFilter with custom frequency.

- **[VelocityBasedAudioSource.cs](Assets/PiDev/Utilities/Audio/VelocityBasedAudioSource.cs)**  
  Dynamically plays friction and impact sounds based on velocity using customizable curves and SoundBankSets.
  Supports Rigidbody, Transform, or custom velocity providers for friction calculation.
  
  Attach to an object with Rigidbody or implement `IVelocityAudioSourceFrictionProvider`.
  Assign `frictionSound` and `impactSound` along with velocity-based curves.
</details>

---

<details>
<summary>📋 Management - Object pooler, Singleton<T>, Reference wrapper</summary>
 
- **[ObjectPooler.cs](Assets/PiDev/Utilities/Management/ObjectPooler.cs)**  
  Yet another lightweight object pool for reusing objects efficiently.
  This one is not Unity-specific and allows full customization.
  ```cs
    var pool = new ObjectPooler<MyType>();
    pool.funcGenerate = () => new MyType();
    pool.Stock(10);
    var item = pool.Buy();
    pool.Recycle(item);
  ```
    
- **[ReferenceWrapper.cs](Assets/PiDev/Utilities/Management/ReferenceWrapper.cs)**  
  Reference wrapper to value type, `Ref<T>` allows structs to behave like references.
  ```cs
    var myRef = new Ref<int>(5);
    int value = myRef; // Implicitly converts to int
    myRef.Value = 10;
  ```

- **[Singleton.cs](Assets/PiDev/Utilities/Management/Singleton.cs)**  
  Yet another basic MonoBehaviour singleton implementation with duplicate protection.
  ```cs
    public class MyManager : Singleton<MyManager> { ... }
  ```
  Access the singleton with `MyManager.instance`.

- **[ObjectReferences.cs](Assets/PiDev/Utilities/Management/ObjectReferences.cs)**  
  Keep strong references to assets to prevent stripping during builds.

- **[Honeypot.cs](Assets/PiDev/Utilities/Management/Honeypot.cs)**  
  Rudimentary security trick to catch people who mess with cheating engines and tools.
  Don't treat this as a true security solution.
  ```cs
    var honeypot = new Honeypot<int>(initialValue, () => Debug.Log("Tampering detected!"));
    honeypot.SetValue(newValue);
    var currentValue = honeypot.GetValue();
    honeypot.CheckForTampering();
    honeypot.Dispose();
  ```

</details>

---

<details>
<summary>🧩 Helpers - USeful stuff for designers and programmers</summary>

- **[CommentComponent.cs](Assets/PiDev/Utilities/Helpers/CommentComponent.cs)**  
  A simple component for attaching notes or comments to GameObjects in the Unity Inspector.  
  Useful for leaving reminders or design-time annotations in scenes/prefabs.
  
- **[ActionButtons.cs](Assets/PiDev/Utilities/Helpers/ActionButtons.cs)**  
  Quickly create dynamic buttons in the inspector bound to Actions.
  ```cs
	[Header("Content tools")]
	public ActionButtons actions = new ActionButtons("",
	   new("Generate Board", () => GenerateChessboard()),
	   new("Capture pieces", CapturePreviews),
	   new("Export AssetBundle",  () => Debug.Log("TODO!"))
	);
  ```
  
- **[NaNFieldProperty.cs](Assets/PiDev/Utilities/Helpers/NaNFieldProperty.cs)**  
  Inspector attribute to easily set float fields to NaN (Not a Number).
  ```cs
    [NaNField] public float optionalValue;
  ```
  Press the *NaN* button next to the field in the Inspector to set it to `float.NaN`.
</details>

---

<details>
<summary>🧠 Logic - Common and uncommon behaviors, algorithms and components</summary>

- **[AStarPathfinder.cs](Assets/PiDev/Utilities/Logic/AStarPathfinder.cs)**  
  A\* pathfinding for 2D grid maps, customizable walkability.
  ```cs
    var grid = new int[width, height]; // Your grid representation
    var path = AStarPathfinder.FindPath(grid, start, goal, isWalkable);
    var simplified = AStarPathfinder.SimplifyPath(path);
  ```
- **[CountdownTimer.cs](Assets/PiDev/Utilities/Logic/CountdownTimer.cs)**  
  Simple countdown timer component with second-based events.
  ```cs
    timer.onSecondPassed.AddListener(sec => Debug.Log($"Seconds left: {sec}"));
    timer.onTimedOut.AddListener(() => Debug.Log("Timer finished"));
    timer.Start(seconds);
  ```
  You also must call `timer.Update()` in your Update() method.
  
- **[DelayedDestroy.cs](Assets/PiDev/Utilities/Logic/DelayedDestroy.cs)**  
  Destroy a GameObject after a specified delay automatically. \
  Attach to a GameObject and set the delay in the inspector or via script.
  
- **[MeshParticleEmitter.cs](Assets/PiDev/Utilities/Logic/MeshParticleEmitter.cs)**  
  Emits particles from mesh and skinned mesh vertex positions using a given ParticleSystem.
  Supports dynamic velocity calculation based on mesh vertex distance and optional mesh exclusion.
  Useful for effects like mesh-based bursts, trails, or vertex-driven particle systems.
  ```cs
    MeshParticleEmitter.EmitFromMeshes(particleSystem, transform);
    MeshParticleEmitter.EmitFromMeshVertices(particleSystem, transform, velocityMultiplier, ignoreList);
  ```
  
- **[PointDistributor.cs](Assets/PiDev/Utilities/Logic/PointDistributor.cs)**  
  Distributes a configurable number of points in 3D space based on selected shape: Line, Circle, or Path.
  Supports centering, custom overrides, and integration with external path providers like DoTweenPath.
  Also supports override sets for exact point configurations and Catmull-Rom interpolation for path shape.
  ```cs
    var points = pointDistributor.GetPoints(count);
  ```
  
- **[ValueInterpolator.cs](Assets/PiDev/Utilities/Logic/ValueInterpolator.cs)**  
  Generic value interpolator for smoothly transitioning between values using linear or lerped modes.
  Accepts custom interpolation functions for full type flexibility (float, Vector types, Quaternion, etc.).
  ```cs
    var interp = ValueInterpolator.Float(0, 10);
    interp.targetValue = 20;
    interp.Update(Time.deltaTime); // you must call this when recalculation is needed
    Debug.Log(interp.currentValue);
  ```
  Use the `Update()` method manually each frame, passing `deltaTime` to progress the interpolation.
  Supports different speeds for increasing vs. decreasing values via optional `negativeSpeed` logic.

</details>

---
<details>
<summary>🏃 Movement - Components for following targets, floating in place and other motion</summary>

- **[FollowTarget.cs](Assets/PiDev/Utilities/Movement/FollowTarget.cs)**  
  Simple follow target script with Transform / Rigidbody movement modes.

- **[OrientWithTarget.cs](Assets/PiDev/Utilities/Movement/OrientWithTarget.cs)**  
  Continuously orient towards another transform.

- **[FollowMultiTargets.cs](Assets/PiDev/Utilities/Movement/FollowMultiTargets.cs)**  
  Follows the average position and orientation of multiple target transforms.
  Supports optional axis snapping to align the resulting rotation with a reference transform.
  Useful for group tracking, midpoints, or collective indicators.

  Add transforms to `targets` and assign a reference to `objectRoot` for axis snapping.\
  Adjust `axisSnapStrength` to control how strongly to align with the reference up direction.

- **[FloatingObjectLocalSpaceMovement.cs](Assets/PiDev/Utilities/Movement/FloatingObjectLocalSpaceMovement.cs)**  
  Adds oscillating movement and rotation in local space for floating effects.
  Can optionally sync with FollowTarget and OrientWithTarget components for dynamic references.
  Resets transform on disable to ensure consistent behavior when re-enabled.

  Attach to a GameObject with `FollowTarget` or `OrientWithTarget` if needed.\
  Configure movement, frequency, and rotation axis in the inspector. 
  
- **[FloatingObjectMovement.cs](Assets/PiDev/Utilities/Movement/FloatingObjectMovement.cs)**  
  Smoothly floats an object around its target using randomized paths.
  
  Attach to a GameObject and configure radius, interval, and damping.\
  Optionally assign a FollowTarget to float relative to another transform.
  
</details>

---

<details>
<summary>🖥️ User Interface - Adaptive containers, Canvas init & Mobile joystick</summary>

The UI module have some powerful yet very niche components for dynamic layout control.

- **[RaycastTarget.cs](Assets/PiDev/Utilities/UI/RaycastTarget.cs)**  
  Invisible UI element to intercept or block raycasts without visuals.
  
- **[AdaptiveDPIScale.cs](Assets/PiDev/Utilities/UI/AdaptiveDPIScale.cs)**  
  Adjusts UI scale based on screen DPI for better readability.
  
- **[AdaptiveGridLayout.cs](Assets/PiDev/Utilities/UI/AdaptiveGridLayout.cs)**  
  Grid layout that adapts column and row count to screen aspect ratio.
  
- **[AdaptiveLayoutGroup.cs](Assets/PiDev/Utilities/UI/AdaptiveLayoutGroup.cs)**  
  Switch between vertical and horizontal layout groups dynamically.
  
- **[AdaptiveLayoutMode.cs](Assets/PiDev/Utilities/UI/AdaptiveLayoutMode.cs)**  
  Fully switch active UI containers based on screen aspect or device.
  
- **[AspectRatioPreferredSizeScaler.cs](Assets/PiDev/Utilities/UI/AspectRatioPreferredSizeScaler.cs)**  
  Dynamically scales a layout element based on preferred aspect ratio.
  
- **[CanvasInitialize.cs](Assets/PiDev/Utilities/UI/CanvasInitialize.cs)**  
  Automatic Canvas configuration for touch-only devices or specific setups.
  This component was created to help me overcome the nonsense where canvases stay at (0,0) during edit time. With this you can make a world space canvas attached to editing camera, but still get overlay UI during runtime.
  
- **[ImagePreferredSizeScaler.cs](Assets/PiDev/Utilities/UI/ImagePreferredSizeScaler.cs)**  
  Auto-scale Image preferred size while preserving aspect ratio.
  
- **[MobileUIJoystick.cs](Assets/PiDev/Utilities/UI/MobileUIJoystick.cs)**  
  Mobile-friendly on-screen joystick with flexible snapping behavior.

</details>

---

<details>
<summary>🛠️ Editor - Action Toolbar and Spreadsheets for editing data</summary>

- **[QuickActionToolbars.cs](Assets/PiDev/Utilities/Editor/QuickActionToolbar/QuickActionToolbars.cs)**  
   Allows adding custom action buttons to the left and right of Unity's Play, Pause, and Step buttons.
   Useful for quickly accessing common tools, shortcuts, or scene actions directly from the main editor toolbar. 
   
   Customize the toolbar by editing this script to define your own commands.

    
- **[CollectionTable.cs](Assets/PiDev/Utilities/Editor/TableView/CollectionTable.cs)**  
  Editor window base class for managing serialized collections via a table.
  
  Inherit from `CollectionTable<T>` and call `SetData(targetObject, "propertyPath")` to bind a serialized array.\
  Use `GetWindow<YourDerivedCollectionTableThing>()` to create and display the window.

- **[ScriptableObjectTable.cs](Assets/PiDev/Utilities/Editor/TableView/ScriptableObjectTable.cs)**  
  Editor window base class for managing ScriptableObject assets in a table view.
  
  Inherit from `ScriptableObjectTable<T>` then `GetWindow<YourDerivedTable>()` to open the window.
  
- **[TableView.cs](Assets/PiDev/Utilities/Editor/TableView/TableView.cs)**  
  A generic, reorderable and sortable Unity Editor table component with customizable columns.
  Supports custom drawing per column, sortable column modes, drag-and-drop row reordering, and selection tracking.
  
  Designed to be used by editor tools and inspectors for lists and serialized data displays.
  
  ```cs
   var table = new TableView<MyDataType>();
   table.AddColumn("Name", 100, (rect, item) => GUI.Label(rect, item.name));
   /* inside OnGUI */ table.Render(myItemsArray);
  ```
  Check `ScriptableObjectTable` and `CollectionTable` for working example.

</details>

---

## 📄 License

All content is under the **MIT License**, unless explicitly stated otherwise in specific files.  
Some splines/math code credits: Paul Bourke.  
Original toolbar concept partially inspired by Bob Berkebile.


## 👤 About the Developer

I'm **Petar Petrov**, known online as **PeterSvP**, an independent game developer and designer from Bulgaria. With a background in professional game development, including experience at Gameloft, I've transitioned to indie development, focusing on creating unique gaming experiences. 

I'm the creator of **ColorBlend FX: Desaturation**, a 2.5D puzzle-platformer metroidvania. 

Beyond game development, I engage with the world through various platforms:

- **Instagram**: [instagram.com/petersvp](https://www.instagram.com/petersvp/)
- **Twitch**: [twitch.tv/petersvp](https://www.twitch.tv/petersvp)
- **Steam**: [steamcommunity.com/id/petersvp](https://steamcommunity.com/id/petersvp/)
- **YouTube**: [youtube.com/petersvp](https://youtube.com/petersvp)

## 🚀 Support small indie Developers
If you use Pi-Dev Utilities in your projects and like them, please consider supporting me by purchasing my games. 
### [**ColorBlend FX: Desaturation** on Steam](https://store.steampowered.com/app/670510/ColorBlend_FX_Desaturation/)

I may also be available for paid freelance projects. 
[Check my profile on Upwork](https://www.upwork.com/freelancers/~01d5d33363951c052c)
