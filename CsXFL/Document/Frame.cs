namespace CsXFL;

using System.Collections.ObjectModel;
using System.Xml.Linq;

public class Frame : ILibraryEventReceiver, IDisposable
{
    internal const string FRAME_NODE_IDENTIFIER = "DOMFrame",
    FRAMES_NODEGROUP_IDENTIFIER = "frames",
    ACTIONSCRIPT_NODE_IDENTIFIER = "Actionscript",
    SCRIPT_NODE_IDENTIFIER = "script",
    FRAME_COLOR_NODE_IDENTIFIER = "frameColor";
    private static readonly HashSet<string> AcceptableLabelTypes = new HashSet<string> { "none", "name", "comment", "anchor" };
    public enum KeyModes : int
    {
        Normal = 9728,
        ClassicTween = 22017,
        ShapeTween = 17922,
        MotionTween = 8195,
        ShapeLayers = 8192
    }
    public static class DefaultValues
    {
        public const int StartFrame = 0;
        public const int Duration = 1;
        public const int KeyMode = (int)KeyModes.Normal;
        public const int InPoint44 = 0;
        public const int MotionTweenRotateTimes = 0;
        public const string LabelType = "none";
        public const string Name = "";
        public const string SoundName = "";
        public const string SoundSync = "event";
        public const string TweenType = "none";
        public const bool MotionTweenSnap = false;
        public const string EaseMethodName = "classic";
        public const string MotionTweenRotate = "auto";
        public const bool HasCustomEase = false;
        public const bool Bookmark = false;
        public const bool UseSingleEaseCurve = true;
        public const string BlendMode = "normal";
        public const bool Visible = true;
    }
    private XElement? root;
    private readonly XNamespace ns;
    private readonly List<Element> elements;
    private readonly List<IEase> eases;
    private readonly List<Filter> frameFilters;
    private int startFrame, duration, keyMode, inPoint44, motionTweenRotateTimes;
    private string labelType, name, soundName, soundSync, tweenType, easeMethodName, motionTweenRotate;
    private bool registeredForSoundItem, motionTweenSnap, hasCustomEase, bookmark, useSingleEaseCurve;
    private MorphShape? morphShape;
    private Library? library;
    private Color? frameColor;
    private string? actionScript;
    private string blendMode;
    private bool visible;
    internal XElement? Root { get { return root; } set { root = value; } }
    public XNamespace Ns { get { return ns.ToString(); } }
    public int StartFrame { get { return startFrame; } set { startFrame = value; root?.SetAttributeValue("index", value); } }
    public int Duration { get { return duration; } set { duration = value; root?.SetOrRemoveAttribute("duration", value, DefaultValues.Duration); } }
    public int KeyMode { get { return keyMode; } set { keyMode = value; root?.SetOrRemoveAttribute("keyMode", value, DefaultValues.KeyMode); } }
    public int InPoint44 { get { return inPoint44; } set { inPoint44 = value; root?.SetOrRemoveAttribute("inPoint44", value, DefaultValues.InPoint44); } }
    public int MotionTweenRotateTimes { get { return motionTweenRotateTimes; } set { motionTweenRotateTimes = value; root?.SetOrRemoveAttribute("motionTweenRotateTimes", value, DefaultValues.MotionTweenRotateTimes); } }
    public string LabelType
    {
        get { return labelType; }
        set
        {
            if (!AcceptableLabelTypes.Contains(value)) throw new ArgumentException();
            labelType = value;
            root?.SetOrRemoveAttribute("labelType", value, DefaultValues.LabelType);
            Bookmark = value == "anchor";
        }
    }
    public string Name { get { return name; } set { name = value; root?.SetOrRemoveAttribute("name", value, DefaultValues.Name); } }
    public string SoundName
    {
        get { return soundName; }
        set
        {
            SoundItem? oldSoundItem = CorrespondingSoundItem;
            soundName = value;
            SoundItem? newSoundItem = CorrespondingSoundItem;
            root?.SetOrRemoveAttribute("soundName", value, DefaultValues.SoundName);
            registeredForSoundItem = value != DefaultValues.SoundName;
            if (oldSoundItem is not null)
            {
                LibraryEventMessenger.Instance.UnregisterReceiver(oldSoundItem, this);
                oldSoundItem.UseCount--;
            }
            if (registeredForSoundItem && newSoundItem is not null)
            {
                LibraryEventMessenger.Instance.RegisterReceiver(newSoundItem!, this);
                newSoundItem.UseCount++;
            }
        }
    }
    public SoundItem? CorrespondingSoundItem { get { return library is not null && library.Items.TryGetValue(SoundName, out Item? item) ? item as SoundItem : null; } }
    public string SoundSync
    {
        get { return soundSync; }
        set
        {
            if (!AcceptableSoundSyncs.Contains(value)) throw new ArgumentException();
            soundSync = value;
            root?.SetOrRemoveAttribute("soundSync", value, DefaultValues.SoundSync);
        }
    }
    public string TweenType { get { return tweenType; } set { tweenType = value; root?.SetOrRemoveAttribute("tweenType", value, DefaultValues.TweenType); } }
    public bool MotionTweenSnap { get { return motionTweenSnap; } set { motionTweenSnap = value; root?.SetOrRemoveAttribute("motionTweenSnap", value, DefaultValues.MotionTweenSnap); } }
    public bool HasCustomEase { get { return hasCustomEase; } set { hasCustomEase = value; root?.SetOrRemoveAttribute("hasCustomEase", value, DefaultValues.HasCustomEase); } }
    public bool Bookmark { get { return bookmark; } set { bookmark = value; root?.SetOrRemoveAttribute("bookmark", value, DefaultValues.Bookmark); } }
    public bool UseSingleEaseCurve { get { return useSingleEaseCurve; } set { useSingleEaseCurve = value; root?.SetOrRemoveAttribute("useSingleEaseCurve", value, DefaultValues.UseSingleEaseCurve); } }
    public string EaseMethodName { get { return easeMethodName; } internal set { easeMethodName = value; root?.SetOrRemoveAttribute("easeMethodName", value, DefaultValues.EaseMethodName); } }
    public string MotionTweenRotate { get { return motionTweenRotate; } set { motionTweenRotate = value; root?.SetOrRemoveAttribute("motionTweenRotate", value, DefaultValues.MotionTweenRotate); } }
    private static readonly HashSet<string> AcceptableSoundSyncs = new HashSet<string> { "event", "start", "stop", "stream" };
    public ReadOnlyCollection<Element> Elements { get { return elements.AsReadOnly(); } }
    public MorphShape? MorphShape { get { return morphShape; } }
    public Color? FrameColor { get { return frameColor; } }
    public string? ActionScript { get { return actionScript; } set { SetActionscript(value); } }
    public ReadOnlyCollection<Filter> Filters { get { return frameFilters.AsReadOnly(); } }
    public string BlendMode { get { return blendMode; } set { blendMode = value; root?.SetOrRemoveAttribute("blendMode", value, DefaultValues.BlendMode); } }
    public bool Visible { get { return visible; } set { visible = value; root?.SetOrRemoveAttribute("visible", value, DefaultValues.Visible); } }
    private void SetActionscript(string? value)
    {
        if (actionScript is null && value is not null)
        {
            // create <Actionscript> node with <script> tag 
            XElement actionScriptNode = new XElement(ns + ACTIONSCRIPT_NODE_IDENTIFIER);
            XElement scriptNode = new XElement(ns + SCRIPT_NODE_IDENTIFIER);
            XCData data = new XCData(value);
            scriptNode.Add(data);
            actionScriptNode.Add(scriptNode);
            root?.Add(actionScriptNode);
        }
        else if (actionScript is not null && value is null)
        {
            // remove <Actionscript> node
            root?.Element(ns + ACTIONSCRIPT_NODE_IDENTIFIER)?.Remove();
        }
        else if (actionScript is not null && value is not null)
        {
            // update <Actionscript> node
            XElement actionScriptNode = root?.Element(ns + ACTIONSCRIPT_NODE_IDENTIFIER)!;
            XElement scriptNode = actionScriptNode.Element(ns + SCRIPT_NODE_IDENTIFIER)!;
            XCData data = new XCData(value);
            scriptNode.ReplaceAll(data);
        }
        actionScript = value;
    }
    private void LoadElements(in XElement frameNode)
    {
        List<XElement>? elementNodes = frameNode.Element(ns + Element.ELEMENTS_NODEGROUP_IDENTIFIER)?.Elements().ToList();
        if (elementNodes is null) return;
        foreach (XElement elementNode in elementNodes)
        {
            string elementName = elementNode.Name.LocalName.ToString();
            switch (elementName)
            {
                case BitmapInstance.BITMAPINSTANCE_NODE_IDENTIFIER:
                    elements.Add(new BitmapInstance(elementNode, library));
                    var CorrespondingItem = (elements.Last() as BitmapInstance)!.CorrespondingItem;
                    if (CorrespondingItem is not null)
                        LibraryEventMessenger.Instance.RegisterReceiver(CorrespondingItem, this);
                    break;
                case SymbolInstance.SYMBOLINSTANCE_NODE_IDENTIFIER:
                    elements.Add(new SymbolInstance(elementNode, library));
                    CorrespondingItem = (elements.Last() as SymbolInstance)!.CorrespondingItem;
                    if (CorrespondingItem is not null)
                        LibraryEventMessenger.Instance.RegisterReceiver(CorrespondingItem, this);
                    break;
                case Text.STATIC_TEXT_NODE_IDENTIFIER:
                    elements.Add(new StaticText(elementNode));
                    break;
                case Text.DYNAMIC_TEXT_NODE_IDENTIFIER:
                    elements.Add(new DynamicText(elementNode));
                    break;
                case Text.INPUT_TEXT_NODE_IDENTIFIER:
                    elements.Add(new InputText(elementNode));
                    break;
                case Shape.SHAPE_NODE_IDENTIFIER:
                    elements.Add(new Shape(elementNode));
                    break;
                case Group.GROUP_NODE_IDENTIFIER:
                    elements.Add(new Group(elementNode, library));
                    break;
                case PrimitiveOval.PRIMITIVE_OVAL_NODE_IDENTIFIER:
                    elements.Add(new PrimitiveOval(elementNode));
                    break;
                case PrimitiveRectangle.PRIMITIVE_RECTANGLE_NODE_IDENTIFIER:
                    elements.Add(new PrimitiveRectangle(elementNode));
                    break;
            }
        }
    }
    private void LoadEases(in XElement frameNode)
    {
        List<XElement>? easeNodes = frameNode.Element(ns + IEase.EASES_NODEGROUP_IDENTIFIER)?.Elements().ToList();
        if (easeNodes is null) return;
        foreach (XElement easeNode in easeNodes)
        {
            string easeName = easeNode.Name.LocalName.ToString();
            switch (easeName)
            {
                case Ease.EASE_NODE_IDENTIFIER:
                    eases.Add(new Ease(easeNode));
                    break;
                case CustomEase.CUSTOM_EASE_NODE_IDENTIFIER:
                    eases.Add(new CustomEase(easeNode));
                    break;
            }
        }
    }
    private void LoadFilters(in XElement frameNode)
    {
        List<XElement>? filterNodes = frameNode.Element(ns + Filter.FRAME_FILTER_NODEGROUP_IDENTIFIER)?.Elements().ToList();
        if (filterNodes is null) return;
        foreach (XElement filterNode in filterNodes)
        {
            switch (filterNode.Name.LocalName)
            {
                case DropShadowFilter.DROPSHADOWFILTER_NODE_IDENTIFIER:
                    frameFilters.Add(new DropShadowFilter(filterNode));
                    break;
                case BlurFilter.BLURFILTER_NODE_IDENTIFIER:
                    frameFilters.Add(new BlurFilter(filterNode));
                    break;
                case GlowFilter.GLOWFILTER_NODE_IDENTIFIER:
                    frameFilters.Add(new GlowFilter(filterNode));
                    break;
                case BevelFilter.BEVELFILTER_NODE_IDENTIFIER:
                    frameFilters.Add(new BevelFilter(filterNode));
                    break;
                case GradientBevelFilter.GRADIENTBEVELFILTER_NODE_IDENTIFIER:
                    frameFilters.Add(new GradientBevelFilter(filterNode));
                    break;
                case GradientGlowFilter.GRADIENTGLOWFILTER_NODE_IDENTIFIER:
                    frameFilters.Add(new GradientGlowFilter(filterNode));
                    break;
                case AdjustColorFilter.ADJUSTCOLORFILTER_NODE_IDENTIFIER:
                    frameFilters.Add(new AdjustColorFilter(filterNode));
                    break;
                default:
                    throw new ArgumentException("Invalid filter type: " + filterNode.Name.LocalName);
            }
        }
    }
    internal Frame(in XElement frameNode, Library? library, bool isBlank = false)
    {
        root = frameNode;
        ns = root.Name.Namespace;
        startFrame = (int?)frameNode.Attribute("index") ?? DefaultValues.StartFrame;
        duration = (int?)frameNode.Attribute("duration") ?? DefaultValues.Duration;
        keyMode = (int?)frameNode.Attribute("keyMode") ?? DefaultValues.KeyMode;
        inPoint44 = (int?)frameNode.Attribute("inPoint44") ?? DefaultValues.InPoint44;
        motionTweenRotateTimes = (int?)frameNode.Attribute("motionTweenRotateTimes") ?? DefaultValues.MotionTweenRotateTimes;
        labelType = (string?)frameNode.Attribute("labelType") ?? DefaultValues.LabelType;
        name = (string?)frameNode.Attribute("name") ?? DefaultValues.Name;
        soundName = (string?)frameNode.Attribute("soundName") ?? DefaultValues.SoundName;
        soundSync = (string?)frameNode.Attribute("soundSync") ?? DefaultValues.SoundSync;
        tweenType = (string?)frameNode.Attribute("tweenType") ?? DefaultValues.TweenType;
        motionTweenSnap = (bool?)frameNode.Attribute("motionTweenSnap") ?? DefaultValues.MotionTweenSnap;
        hasCustomEase = (bool?)frameNode.Attribute("hasCustomEase") ?? DefaultValues.HasCustomEase;
        bookmark = (bool?)frameNode.Attribute("bookmark") ?? DefaultValues.Bookmark;
        useSingleEaseCurve = (bool?)frameNode.Attribute("useSingleEaseCurve") ?? DefaultValues.UseSingleEaseCurve;
        easeMethodName = (string?)frameNode.Attribute("easeMethodName") ?? DefaultValues.EaseMethodName;
        motionTweenRotate = (string?)frameNode.Attribute("motionTweenRotate") ?? DefaultValues.MotionTweenRotate;
        morphShape = (frameNode.Element(ns + MorphShape.MORPHSHAPE_NODE_IDENTIEFIER) is null) ? null : new MorphShape(frameNode.Element(ns + MorphShape.MORPHSHAPE_NODE_IDENTIEFIER)!);
        frameColor = (frameNode.Element(ns + Frame.FRAME_COLOR_NODE_IDENTIFIER)?.Element(ns + Color.COLOR_NODE_IDENTIFIER) is null) ? null : new Color(frameNode.Element(ns + Frame.FRAME_COLOR_NODE_IDENTIFIER)!.Element(ns + Color.COLOR_NODE_IDENTIFIER)!);
        this.library = library;
        elements = new List<Element>();
        eases = new List<IEase>();
        frameFilters = new List<Filter>();
        if (!isBlank)
        {
            LoadElements(root);
            LoadEases(root);
            LoadFilters(root);
        }
        registeredForSoundItem = SoundName != DefaultValues.SoundName;
        if (registeredForSoundItem && library is not null)
        {
            LibraryEventMessenger.Instance.RegisterReceiver(CorrespondingSoundItem!, this);
            CorrespondingSoundItem!.UseCount++;
        }
        actionScript = frameNode.Element(ns + ACTIONSCRIPT_NODE_IDENTIFIER)?.Value;
        blendMode = (string?)frameNode.Attribute("blendMode") ?? DefaultValues.BlendMode;
        visible = (bool?)frameNode.Attribute("visible") ?? DefaultValues.Visible;
    }

    internal Frame(Frame other, bool isBlank = false)
    {
        root = other.root is null ? null : new XElement(other.root);
        ns = other.ns;
        startFrame = other.startFrame;
        duration = other.duration;
        keyMode = other.keyMode;
        inPoint44 = other.inPoint44;
        motionTweenRotateTimes = other.motionTweenRotateTimes;
        labelType = other.labelType;
        name = other.name;
        soundName = other.soundName;
        soundSync = other.soundSync;
        tweenType = other.tweenType;
        motionTweenSnap = other.motionTweenSnap;
        hasCustomEase = other.hasCustomEase;
        easeMethodName = other.easeMethodName;
        motionTweenRotate = other.motionTweenRotate;
        morphShape = other.morphShape is null ? null : new MorphShape(other.morphShape);
        frameColor = other.frameColor is null ? null : new Color(other.frameColor);
        library = other.library;
        elements = new List<Element>();
        eases = new List<IEase>();
        frameFilters = new List<Filter>();
        if (root is not null && !isBlank)
        {
            LoadElements(root);
            LoadEases(root);
            LoadFilters(root);
        }
        registeredForSoundItem = SoundName != DefaultValues.SoundName;
        if (registeredForSoundItem)
        {
            LibraryEventMessenger.Instance.RegisterReceiver(CorrespondingSoundItem!, this);
            CorrespondingSoundItem!.UseCount++;
        }
        actionScript = other.actionScript;
        blendMode = other.blendMode;
        visible = other.visible;
    }

    public void Dispose()
    {
        if (registeredForSoundItem)
        {
            LibraryEventMessenger.Instance.UnregisterReceiver(CorrespondingSoundItem!, this);
            CorrespondingSoundItem!.UseCount--;
        }
        CleanupElements();
    }

    public bool IsEmpty()
    {
        return !elements.Any();
    }
    private void CleanupElements()
    {
        foreach (Element element in elements)
        {
            if (element is Instance instance)
            {
                if (instance.CorrespondingItem is not null)
                {
                    LibraryEventMessenger.Instance.UnregisterReceiver(instance.CorrespondingItem, this);
                    instance.Dispose();
                }
            }
        }
    }
    // doesn't clear soundName
    public void ClearElements()
    {
        // unregister from library events
        CleanupElements();
        elements.Clear();
        root?.Element(ns + Element.ELEMENTS_NODEGROUP_IDENTIFIER)?.RemoveAll();
    }
    public Text AddNewText(Rectangle boundingRect, string characters = "")
    {
        Text text = new StaticText(boundingRect, characters, ns);
        elements.Add(text);
        root?.Element(ns + Element.ELEMENTS_NODEGROUP_IDENTIFIER)?.Add(text.Root);
        return text;
    }
    internal Instance? AddItem(Item item)
    {
        // need to create constructors that turn items into instances unless it's a soundItem
        if (item is SoundItem soundItem)
        {
            SoundName = soundItem.Name;
            return null;
        }
        if (item is SymbolItem symbolItem)
        {
            SymbolInstance symbolInstance = new SymbolInstance(symbolItem, library);
            elements.Add(symbolInstance);
            root?.Element(ns + Element.ELEMENTS_NODEGROUP_IDENTIFIER)?.Add(symbolInstance.Root);
            LibraryEventMessenger.Instance.RegisterReceiver(symbolInstance.CorrespondingItem!, this);
            return symbolInstance;
        }
        if (item is BitmapItem bitmapItem)
        {
            BitmapInstance bitmapInstance = new BitmapInstance(bitmapItem, library);
            elements.Add(bitmapInstance);
            root?.Element(ns + Element.ELEMENTS_NODEGROUP_IDENTIFIER)?.Add(bitmapInstance.Root);
            LibraryEventMessenger.Instance.RegisterReceiver(bitmapInstance.CorrespondingItem!, this);
            return bitmapInstance;
        }
        return null;
    }
    void ILibraryEventReceiver.OnLibraryEvent(object sender, LibraryEventMessenger.LibraryEventArgs e)
    {
        if (e.EventType == LibraryEventMessenger.LibraryEvent.ItemRenamed && soundName == e.OldName)
        {
            SoundName = e.NewName!;
        }
        if (e.EventType == LibraryEventMessenger.LibraryEvent.ItemRemoved)
        {
            if (SoundName == e.Item!.Name)
            {
                SoundName = DefaultValues.SoundName;
            }
            for (int i = elements.Count - 1; i >= 0; i--)
            {
                Element element = elements[i];
                if (element is Instance instance && instance.CorrespondingItem == e.Item)
                {
                    elements.RemoveAt(i);
                    instance.Root?.Remove();
                }
            }
        }
    }
    internal void CreateMotionTween(string? target = null, string? method = null)
    {
        target ??= "all";
        method ??= "none";
        KeyMode = (int)KeyModes.ClassicTween;
        TweenType = "motion";
        MotionTweenSnap = true;
        if (root?.Element(ns + IEase.EASES_NODEGROUP_IDENTIFIER) is null)
        {
            root?.Add(new XElement(ns + IEase.EASES_NODEGROUP_IDENTIFIER));
        }
        root?.Element(ns + IEase.EASES_NODEGROUP_IDENTIFIER)?.RemoveAll();
        XElement easeNode = new(ns + Ease.EASE_NODE_IDENTIFIER);
        easeNode.SetAttributeValue("target", target);
        easeNode.SetAttributeValue("method", method);
        root?.Element(ns + IEase.EASES_NODEGROUP_IDENTIFIER)?.Add(easeNode);
        eases.Add(new Ease(easeNode));
        EaseMethodName = method;
    }
    public void RemoveTween()
    {
        KeyMode = (int)KeyModes.Normal;
        TweenType = "none";
        MotionTweenSnap = false;
        root?.Element(ns + IEase.EASES_NODEGROUP_IDENTIFIER)?.Remove();
        eases.Clear();
        EaseMethodName = DefaultValues.EaseMethodName;
    }
    public double GetTweenMultiplier(int frameIndex, string target = "all")
    {
        if (!useSingleEaseCurve && target == "all") throw new ArgumentException($"Target {target} is not supported for multiple ease curve mode", nameof(target));
        if (frameIndex >= duration) throw new ArgumentOutOfRangeException(nameof(frameIndex), $"Frame index {frameIndex} is greater than duration {duration}");
        if (useSingleEaseCurve)
        {
            // bandaid fix for now; need to properly implement default eases simliar to matrices
            IEase allCurve = eases.FirstOrDefault(x => x.Target == "all") ?? new Ease(ns);
            return allCurve.GetMultiplier(frameIndex, duration);
        }
        IEase? targetEase = eases.FirstOrDefault(x => x.Target == target);
        if (targetEase is null) return (new Ease(ns) as IEase).GetMultiplier(frameIndex, duration);
        return targetEase.GetMultiplier(frameIndex, duration);
    }
}