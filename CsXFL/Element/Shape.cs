using System.Xml.Linq;

namespace CsXFL;
public class Shape : Element
{
    public const string SHAPE_NODE_IDENTIFIER = "DOMShape";
    private List<FillStyle> fills;
    private List<StrokeStyle> strokes;
    private List<Edge> edges;
    public List<FillStyle> Fills { get { return fills; } set { SetFills(value); } }
    public List<StrokeStyle> Strokes { get { return strokes; } set { SetStrokes(value); } }
    public List<Edge> Edges { get { return edges; } set { SetEdges(value); } }

    public Shape(XElement root) : base(root, "shape")
    {
        this.root = root;
        ns = root.Name.Namespace;
        fills = root.Element(ns + FillStyle.FILL_STYLES_NODEGROUP_IDENTIFIER)?.Elements(ns + FillStyle.FILL_STYLE_NODE_IDENTIFIER)?.Select(x => new FillStyle(x)).ToList() ?? new List<FillStyle>();
        strokes = root.Element(ns + StrokeStyle.STROKE_STYLES_NODEGROUP_IDENTIFIER)?.Elements(ns + StrokeStyle.STROKE_STYLE_NODE_IDENTIFIER)?.Select(x => new StrokeStyle(x)).ToList() ?? new List<StrokeStyle>();
        edges = root.Element(ns + Edge.EDGES_NODEGROUP_IDENTIFIER)?.Elements(ns + Edge.EDGE_NODE_IDENTIFIER)?.Select(x => new Edge(x)).ToList() ?? new List<Edge>();
    }
    public Shape(Shape other) : base(other)
    {
        fills = other.fills.Select(x => new FillStyle(x)).ToList();
        strokes = other.strokes.Select(x => new StrokeStyle(x)).ToList();
        edges = other.edges.Select(x => new Edge(x)).ToList();
        SetFills(fills);
        SetStrokes(strokes);
        SetEdges(edges);
    }
    private void SetFills(List<FillStyle> fills)
    {
        this.fills = fills;
        if (root is null) return;
        root.Element(ns + FillStyle.FILL_STYLES_NODEGROUP_IDENTIFIER)?.Remove();
        XElement fillsNode = new XElement(ns + FillStyle.FILL_STYLES_NODEGROUP_IDENTIFIER);
        foreach (FillStyle fill in fills)
        {
            fillsNode.Add(fill.Root);
        }
        root.Add(fillsNode);
    }
    private void SetStrokes(List<StrokeStyle> strokes)
    {
        this.strokes = strokes;
        if (root is null) return;
        root.Element(ns + StrokeStyle.STROKE_STYLES_NODEGROUP_IDENTIFIER)?.Remove();
        XElement strokesNode = new XElement(ns + StrokeStyle.STROKE_STYLES_NODEGROUP_IDENTIFIER);
        foreach (StrokeStyle stroke in strokes)
        {
            strokesNode.Add(stroke.Root);
        }
        root.Add(strokesNode);
    }
    private void SetEdges(List<Edge> edges)
    {
        this.edges = edges;
        if (root is null) return;
        root.Element(ns + Edge.EDGES_NODEGROUP_IDENTIFIER)?.Remove();
        XElement edgesNode = new XElement(ns + Edge.EDGES_NODEGROUP_IDENTIFIER);
        foreach (Edge edge in edges)
        {
            edgesNode.Add(edge.Root);
        }
        root.Add(edgesNode);
    }
}

public class FillStyle
{
    public const string FILL_STYLE_NODE_IDENTIFIER = "FillStyle",
    FILL_STYLES_NODEGROUP_IDENTIFIER = "fills";
    private readonly XElement root;
    private readonly XNamespace ns;
    private readonly int index;
    private SolidColor? solidColor;
    private LinearGradient? linearGradient;
    private RadialGradient? radialGradient;
    private BitmapFill? bitmapFill;
    public XElement Root { get { return root; } }
    public int Index { get { return index; } }
    public SolidColor? SolidColor { get { return solidColor; } set { SetSolidColor(value); } }
    public LinearGradient? LinearGradient { get { return linearGradient; } set { SetLinearGradient(value); } }
    public RadialGradient? RadialGradient { get { return radialGradient; } set { SetRadialGradient(value); } }
    public BitmapFill? BitmapFill { get { return bitmapFill; } set { SetBitmapFill(value); } }

    internal FillStyle(XElement root)
    {
        this.root = root;
        ns = root.Name.Namespace;
        index = (int?)root.Attribute("index") ?? -1;
        if (root.Element(ns + SolidColor.SOLID_COLOR_NODE_IDENTIFIER) != null)
        {
            solidColor = new SolidColor(root.Element(ns + SolidColor.SOLID_COLOR_NODE_IDENTIFIER)!);
        }
        else if (root.Element(ns + LinearGradient.LINEAR_GRADIENT_NODE_IDENTIFIER) != null)
        {
            linearGradient = new LinearGradient(root.Element(ns + LinearGradient.LINEAR_GRADIENT_NODE_IDENTIFIER)!);
        }
        else if (root.Element(ns + RadialGradient.RADIAL_GRADIENT_NODE_IDENTIFIER) != null)
        {
            radialGradient = new RadialGradient(root.Element(ns + RadialGradient.RADIAL_GRADIENT_NODE_IDENTIFIER)!);
        }
        else if (root.Element(ns + BitmapFill.BITMAPFILL_NODE_IDENTIFIER) != null)
        {
            bitmapFill = new BitmapFill(root.Element(ns + BitmapFill.BITMAPFILL_NODE_IDENTIFIER)!);
        }
    }
    public FillStyle(FillStyle other)
    {
        root = new XElement(other.Root);
        ns = other.ns;
        index = other.Index;
        solidColor = other.SolidColor is null ? null : new SolidColor(other.SolidColor);
        linearGradient = other.LinearGradient is null ? null : new LinearGradient(other.LinearGradient);
        radialGradient = other.RadialGradient is null ? null : new RadialGradient(other.RadialGradient);
        bitmapFill = other.BitmapFill is null ? null : new BitmapFill(other.BitmapFill);
    }
    private void SetSolidColor(SolidColor? color)
    {
        solidColor = color;
        XElement? solidColorNode = root.Element(ns + SolidColor.SOLID_COLOR_NODEGROUP_IDENTIFIER)?.Element(ns + SolidColor.SOLID_COLOR_NODE_IDENTIFIER);
        solidColorNode?.Remove();
        if (color is not null)
        {
            XElement? solidColorNodegroup = root.Element(ns + SolidColor.SOLID_COLOR_NODEGROUP_IDENTIFIER);
            if (solidColorNodegroup is null)
            {
                solidColorNodegroup = new XElement(ns + SolidColor.SOLID_COLOR_NODEGROUP_IDENTIFIER);
                root.Add(solidColorNodegroup);
            }
            solidColorNodegroup.Add(color.Root);
        }
    }
    private void SetLinearGradient(LinearGradient? gradient)
    {
        linearGradient = gradient;
        XElement? linearGradientNode = root.Element(ns + LinearGradient.LINEAR_GRADIENT_NODEGROUP_IDENTIFIER)?.Element(ns + LinearGradient.LINEAR_GRADIENT_NODE_IDENTIFIER);
        linearGradientNode?.Remove();
        if (gradient is not null)
        {
            XElement? linearGradientNodegroup = root.Element(ns + LinearGradient.LINEAR_GRADIENT_NODEGROUP_IDENTIFIER);
            if (linearGradientNodegroup is null)
            {
                linearGradientNodegroup = new XElement(ns + LinearGradient.LINEAR_GRADIENT_NODEGROUP_IDENTIFIER);
                root.Add(linearGradientNodegroup);
            }
            linearGradientNodegroup.Add(gradient.Root);
        }
    }
    private void SetRadialGradient(RadialGradient? gradient)
    {
        radialGradient = gradient;
        XElement? radialGradientNode = root.Element(ns + RadialGradient.RADIAL_GRADIENT_NODEGROUP_IDENTIFIER)?.Element(ns + RadialGradient.RADIAL_GRADIENT_NODE_IDENTIFIER);
        radialGradientNode?.Remove();
        if (gradient is not null)
        {
            XElement? radialGradientNodegroup = root.Element(ns + RadialGradient.RADIAL_GRADIENT_NODEGROUP_IDENTIFIER);
            if (radialGradientNodegroup is null)
            {
                radialGradientNodegroup = new XElement(ns + RadialGradient.RADIAL_GRADIENT_NODEGROUP_IDENTIFIER);
                root.Add(radialGradientNodegroup);
            }
            radialGradientNodegroup.Add(gradient.Root);
        }
    }
    private void SetBitmapFill(BitmapFill? fill)
    {
        bitmapFill = fill;
        XElement? bitmapFillNode = root.Element(ns + BitmapFill.BITMAPFILL_NODEGROUP_IDENTIFIER)?.Element(ns + BitmapFill.BITMAPFILL_NODEGROUP_IDENTIFIER);
        bitmapFillNode?.Remove();
        if (fill is not null)
        {
            XElement? bitmapFillNodegroup = root.Element(ns + BitmapFill.BITMAPFILL_NODEGROUP_IDENTIFIER);
            if (bitmapFillNodegroup is null)
            {
                bitmapFillNodegroup = new XElement(ns + BitmapFill.BITMAPFILL_NODEGROUP_IDENTIFIER);
                root.Add(bitmapFillNodegroup);
            }
            bitmapFillNodegroup.Add(fill.Root);
        }
    }
}
public class SolidColor
{
    public const string SOLID_COLOR_NODE_IDENTIFIER = "SolidColor",
    SOLID_COLOR_NODEGROUP_IDENTIFIER = "fill";
    public static class DefaultValues
    {
        public const string Color = "#000000";
        public const double Alpha = 1.0;
    }
    private readonly XElement root;
    private string color;
    private double alpha;
    public XElement Root { get { return root; } }
    public string Color { get { return color; } set { color = value; root?.SetOrRemoveAttribute("color", value, DefaultValues.Color); } }
    public double Alpha { get { return alpha; } set { alpha = value; root?.SetOrRemoveAttribute("alpha", value, DefaultValues.Alpha); } }
    internal SolidColor(XElement root)
    {
        this.root = root;
        color = root.Attribute("color")?.Value ?? DefaultValues.Color;
        alpha = (double?)root.Attribute("alpha") ?? DefaultValues.Alpha;
    }
    internal SolidColor(XNamespace ns)
    {
        color = DefaultValues.Color;
        alpha = DefaultValues.Alpha;
        root = new XElement(ns + SOLID_COLOR_NODE_IDENTIFIER);
    }
    public SolidColor(SolidColor other)
    {
        root = new XElement(other.root);
        color = other.color;
        alpha = other.alpha;
    }
}
public abstract class Gradient
{
    public static class DefaultValues
    {
        public const string SpreadMethod = "extend";
        public const string InterpolationMethod = "";
    }
    protected XElement? root;
    protected XNamespace ns;
    protected string spreadMethod, interpolationMethod;
    protected Matrix matrix;
    protected List<GradientEntry> gradientEntries;
    internal XElement? Root { get { return root; } }
    public string SpreadMethod { get { return spreadMethod; } set { spreadMethod = value; root?.SetOrRemoveAttribute("spreadMethod", value, DefaultValues.SpreadMethod); } }
    public string InterpolationMethod { get { return interpolationMethod; } set { interpolationMethod = value; root?.SetOrRemoveAttribute("interpolationMethod", value, DefaultValues.InterpolationMethod); } }
    public Matrix Matrix { get { return matrix; } set { SetMatrix(value); } }
    public List<GradientEntry> GradientEntries { get { return gradientEntries; } set { SetGradientEntries(value); } }
    private void SetMatrix(Matrix matrix)
    {
        // set values, not the matrix itself
        this.matrix.A = matrix.A;
        this.matrix.B = matrix.B;
        this.matrix.C = matrix.C;
        this.matrix.D = matrix.D;
        this.matrix.Tx = matrix.Tx;
        this.matrix.Ty = matrix.Ty;
    }
    internal Gradient(XNamespace ns)
    {
        root = null;
        this.ns = ns;
        spreadMethod = DefaultValues.SpreadMethod;
        interpolationMethod = DefaultValues.InterpolationMethod;
        matrix = new Matrix(ns, root);
        gradientEntries = new List<GradientEntry>();
    }
    internal Gradient(in XElement gradientNode)
    {
        root = gradientNode;
        ns = gradientNode.Name.Namespace;
        spreadMethod = gradientNode.Attribute("spreadMethod")?.Value ?? DefaultValues.SpreadMethod;
        interpolationMethod = gradientNode.Attribute("interpolationMethod")?.Value ?? DefaultValues.InterpolationMethod;
        matrix = gradientNode.Element(ns + Matrix.MATRIX_NODEGROUP_IDENTIFIER)?.Element(ns + Matrix.MATRIX_NODE_IDENTIFIER) is not null ? new Matrix(gradientNode.Element(ns + Matrix.MATRIX_NODEGROUP_IDENTIFIER)!.Element(ns + Matrix.MATRIX_NODE_IDENTIFIER), gradientNode) : new Matrix(ns, gradientNode);
        gradientEntries = gradientNode?.Elements(ns + GradientEntry.GRADIENTENTRY_NODE_IDENTIFIER).Select(e => new GradientEntry(e)).ToList() ?? new List<GradientEntry>();

    }
    internal Gradient(in Gradient other)
    {
        root = other.Root is null ? null : new XElement(other.Root);
        ns = other.ns;
        spreadMethod = other.spreadMethod;
        interpolationMethod = other.interpolationMethod;
        matrix = new Matrix(root?.Element(ns + Matrix.MATRIX_NODEGROUP_IDENTIFIER)?.Element(ns + Matrix.MATRIX_NODE_IDENTIFIER)!, root);
        gradientEntries = new List<GradientEntry>();
        var gradientEntriesCpy = other.gradientEntries.Select(e => new GradientEntry(e)).ToList();
        SetGradientEntries(gradientEntriesCpy);
    }
    private void SetGradientEntries(List<GradientEntry> gradientEntries)
    {
        root?.Elements(ns + GradientEntry.GRADIENTENTRY_NODE_IDENTIFIER)?.Remove();
        foreach (var gradientEntry in gradientEntries)
        {
            root?.Add(gradientEntry.Root);
        }
        this.gradientEntries = gradientEntries;
    }
}
public class RadialGradient : Gradient
{
    public const string RADIAL_GRADIENT_NODE_IDENTIFIER = "RadialGradient", RADIAL_GRADIENT_NODEGROUP_IDENTIFIER = "fill";
    public new static class DefaultValues
    {
        public const double FocalPointRatio = 0.0;
    }
    private double focalPointRatio;
    public double FocalPointRatio { get { return focalPointRatio; } set { focalPointRatio = value; root?.SetOrRemoveAttribute("focalPointRatio", value, DefaultValues.FocalPointRatio); } }
    internal RadialGradient(XNamespace ns) : base(ns) { }
    internal RadialGradient(in XElement radialGradientNode) : base(radialGradientNode)
    {
        focalPointRatio = (double?)radialGradientNode.Attribute("focalPointRatio") ?? DefaultValues.FocalPointRatio;
    }
    public RadialGradient(in RadialGradient other) : base(other)
    {
        focalPointRatio = other.focalPointRatio;
    }
}
public class LinearGradient : Gradient
{
    public const string LINEAR_GRADIENT_NODE_IDENTIFIER = "LinearGradient", LINEAR_GRADIENT_NODEGROUP_IDENTIFIER = "fill";
    internal LinearGradient(XNamespace ns) : base(ns) { }
    internal LinearGradient(in XElement linearGradientNode) : base(linearGradientNode) { }
    public LinearGradient(in LinearGradient other) : base(other) { }
}
public class BitmapFill
{
    public const string BITMAPFILL_NODE_IDENTIFIER = "BitmapFill",
    BITMAPFILL_NODEGROUP_IDENTIFIER = "fill";
    private readonly XElement? root;
    private readonly XNamespace ns;
    private string? bitmapPath;
    private Matrix matrix;
    public XElement? Root { get { return root; } }
    public string? BitmapPath { get { return bitmapPath; } set { bitmapPath = value; root?.SetAttributeValue("bitmapPath", value); } }
    public Matrix Matrix { get { return matrix; } set { SetMatrix(value); } }
    private void SetMatrix(Matrix matrix)
    {
        // set values, not the matrix itself
        this.matrix.A = matrix.A;
        this.matrix.B = matrix.B;
        this.matrix.C = matrix.C;
        this.matrix.D = matrix.D;
        this.matrix.Tx = matrix.Tx;
        this.matrix.Ty = matrix.Ty;
    }
    internal BitmapFill(XNamespace ns)
    {
        root = null;
        this.ns = ns;
        bitmapPath = null;
        matrix = new Matrix(ns, root);
    }
    internal BitmapFill(XElement bitmapFillNode)
    {
        root = bitmapFillNode;
        ns = bitmapFillNode.Name.Namespace;
        bitmapPath = bitmapFillNode.Attribute("bitmapPath")?.Value;
        matrix = bitmapFillNode.Element(ns + Matrix.MATRIX_NODEGROUP_IDENTIFIER)?.Element(ns + Matrix.MATRIX_NODE_IDENTIFIER) is not null ? new Matrix(bitmapFillNode.Element(ns + Matrix.MATRIX_NODEGROUP_IDENTIFIER)!.Element(ns + Matrix.MATRIX_NODE_IDENTIFIER), bitmapFillNode) : new Matrix(ns, bitmapFillNode);
    }
    internal BitmapFill(BitmapFill other)
    {
        root = other.Root is null ? null : new XElement(other.Root);
        ns = other.ns;
        bitmapPath = other.bitmapPath;
        matrix = new Matrix(root?.Element(ns + Matrix.MATRIX_NODEGROUP_IDENTIFIER)?.Element(ns + Matrix.MATRIX_NODE_IDENTIFIER)!, root);
    }
}
public class Edge
{
    public const string EDGE_NODE_IDENTIFIER = "Edge",
    EDGES_NODEGROUP_IDENTIFIER = "edges";
    private readonly XElement root;
    private readonly XNamespace ns;
    private int? fillStyle0, fillStyle1, strokeStyle;
    private string? edges, cubics;
    public XElement Root { get { return root; } }
    public int? FillStyle0 { get { return fillStyle0; } set { fillStyle0 = value; root.SetAttributeValue("fillStyle0", value); } }
    public int? FillStyle1 { get { return fillStyle1; } set { fillStyle1 = value; root.SetAttributeValue("fillStyle1", value); } }
    public int? StrokeStyle { get { return strokeStyle; } set { strokeStyle = value; root.SetAttributeValue("strokeStyle", value); } }
    public string? Edges { get { return edges; } set { edges = value; root.SetOrRemoveAttribute("edges", value, ""); } }
    public string? Cubics { get { return cubics; } set { cubics = value; root.SetOrRemoveAttribute("cubics", value, ""); } }
    public Edge(XElement root)
    {
        this.root = root;
        ns = root.Name.Namespace;
        fillStyle0 = (int?)root.Attribute("fillStyle0");
        fillStyle1 = (int?)root.Attribute("fillStyle1");
        strokeStyle = (int?)root.Attribute("strokeStyle");
        edges = root.Attribute("edges")?.Value;
        cubics = root.Attribute("cubics")?.Value;
    }
    public Edge(Edge other)
    {
        root = new XElement(other.Root);
        ns = other.ns;
        fillStyle0 = other.fillStyle0;
        fillStyle1 = other.fillStyle1;
        strokeStyle = other.strokeStyle;
        edges = other.edges;
        cubics = other.cubics;
    }
}
public class StrokeStyle
{
    public const string STROKE_STYLE_NODE_IDENTIFIER = "StrokeStyle",
    STROKE_STYLES_NODEGROUP_IDENTIFIER = "strokes";
    private readonly XElement root;
    private readonly XNamespace ns;
    private int index;
    private Stroke stroke;
    public int Index { get { return index; } }
    public Stroke Stroke { get { return stroke; } }
    public XElement Root { get { return root; } }
    public StrokeStyle(XElement root)
    {
        this.root = root;
        ns = root.Name.Namespace;
        index = (int?)root.Attribute("index") ?? -1;
        string strokeType = root.Elements().First().Name.LocalName;
        switch (strokeType)
        {
            case SolidStroke.SOLID_STROKE_NODE_IDENTIFIER:
                stroke = new SolidStroke(root.Element(ns + SolidStroke.SOLID_STROKE_NODE_IDENTIFIER)!);
                break;
            case DashedStroke.DASHED_STROKE_NODE_IDENTIFIER:
                stroke = new DashedStroke(root.Element(ns + DashedStroke.DASHED_STROKE_NODE_IDENTIFIER)!);
                break;
            case DottedStroke.DOTTED_STROKE_NODE_IDENTIFIER:
                stroke = new DottedStroke(root.Element(ns + DottedStroke.DOTTED_STROKE_NODE_IDENTIFIER)!);
                break;
            case RaggedStroke.RAGGED_STROKE_NODE_IDENTIFIER:
                stroke = new RaggedStroke(root.Element(ns + RaggedStroke.RAGGED_STROKE_NODE_IDENTIFIER)!);
                break;
            case StippleStroke.STIPPLE_STROKE_NODE_IDENTIFIER:
                stroke = new StippleStroke(root.Element(ns + StippleStroke.STIPPLE_STROKE_NODE_IDENTIFIER)!);
                break;
            case HatchedStroke.HATCHED_STROKE_NODE_IDENTIFIER:
                stroke = new HatchedStroke(root.Element(ns + HatchedStroke.HATCHED_STROKE_NODE_IDENTIFIER)!);
                break;
            default:
                throw new ArgumentException("Unknown stroke type: " + strokeType);

        }
    }
    public StrokeStyle(StrokeStyle other)
    {
        root = new XElement(other.Root);
        ns = other.ns;
        index = other.index;
        if (other.stroke is SolidStroke ss)
        {
            stroke = new SolidStroke(ss);
        }
        else if (other.stroke is DashedStroke ds)
        {
            stroke = new DashedStroke(ds);
        }
        else if (other.stroke is DottedStroke dos)
        {
            stroke = new DottedStroke(dos);
        }
        else if (other.stroke is RaggedStroke rs)
        {
            stroke = new RaggedStroke(rs);
        }
        else if (other.stroke is StippleStroke sts)
        {
            stroke = new StippleStroke(sts);
        }
        else if (other.stroke is HatchedStroke hs)
        {
            stroke = new HatchedStroke(hs);
        }
        else throw new Exception("Unknown stroke type: " + other.stroke);
        SetStroke(stroke);
    }
    private void SetStroke(Stroke stroke)
    {
        root.Element(ns + SolidStroke.SOLID_STROKE_NODE_IDENTIFIER)?.Remove();
        root.Element(ns + DashedStroke.DASHED_STROKE_NODE_IDENTIFIER)?.Remove();
        root.Element(ns + DottedStroke.DOTTED_STROKE_NODE_IDENTIFIER)?.Remove();
        root.Element(ns + RaggedStroke.RAGGED_STROKE_NODE_IDENTIFIER)?.Remove();
        root.Element(ns + StippleStroke.STIPPLE_STROKE_NODE_IDENTIFIER)?.Remove();
        root.Element(ns + HatchedStroke.HATCHED_STROKE_NODE_IDENTIFIER)?.Remove();
        if (stroke is SolidStroke ss)
        {
            root.Add(ss.Root);
        }
        else if (stroke is DashedStroke ds)
        {
            root.Add(ds.Root);
        }
        else if (stroke is DottedStroke dos)
        {
            root.Add(dos.Root);
        }
        else if (stroke is RaggedStroke rs)
        {
            root.Add(rs.Root);
        }
        else if (stroke is StippleStroke sts)
        {
            root.Add(sts.Root);
        }
        else if (stroke is HatchedStroke hs)
        {
            root.Add(hs.Root);
        }
        this.stroke = stroke;
    }
}
public abstract class Stroke
{
    public static class DefaultValues
    {
        public const string Caps = "round";
        public const bool PixelHinting = false;
        public const string Joints = "round";
        public const int MiterLimit = 3;
        public const double Weight = 1.0;
        public const string ScaleMode = "none";

    }
    protected readonly XElement root;
    protected readonly XNamespace ns;
    private bool pixelHinting;
    private string scaleMode, caps, joints;
    private int miterLimit;
    private double weight;
    private SolidColor? solidColor;
    private LinearGradient? linearGradient;
    private RadialGradient? radialGradient;
    private BitmapFill? bitmapFill;
    public XElement Root { get { return root; } }
    public string ScaleMode { get { return scaleMode; } set { scaleMode = value; root?.SetOrRemoveAttribute("scaleMode", value, DefaultValues.ScaleMode); } }
    public string Caps { get { return caps; } set { caps = value; root?.SetOrRemoveAttribute("caps", value, DefaultValues.Caps); } }
    public bool PixelHinting { get { return pixelHinting; } set { pixelHinting = value; root?.SetOrRemoveAttribute("pixelHinting", value, DefaultValues.PixelHinting); } }
    public string Joints { get { return joints; } set { joints = value; root?.SetOrRemoveAttribute("joints", value, DefaultValues.Joints); } }
    public double Weight { get { return weight; } set { weight = value; root?.SetOrRemoveAttribute("weight", value, DefaultValues.Weight); } }
    public int MiterLimit { get { return miterLimit; } set { miterLimit = value; root?.SetOrRemoveAttribute("miterLimit", value, DefaultValues.MiterLimit); } }
    public SolidColor? SolidColor { get { return solidColor; } set { SetSolidColor(value); } }
    public LinearGradient? LinearGradient { get { return linearGradient; } set { SetLinearGradient(value); } }
    public RadialGradient? RadialGradient { get { return radialGradient; } set { SetRadialGradient(value); } }
    public BitmapFill? BitmapFill { get { return bitmapFill; } set { SetBitmapFill(value); } }
    internal Stroke(XElement root)
    {
        this.root = root;
        ns = root.Name.Namespace;
        scaleMode = root.Attribute("scaleMode")?.Value ?? DefaultValues.ScaleMode;
        caps = root.Attribute("caps")?.Value ?? DefaultValues.Caps;
        pixelHinting = (bool?)root.Attribute("pixelHinting") ?? DefaultValues.PixelHinting;
        joints = root.Attribute("joints")?.Value ?? DefaultValues.Joints;
        weight = (double?)root.Attribute("weight") ?? DefaultValues.Weight;
        miterLimit = (int?)root.Attribute("miterLimit") ?? DefaultValues.MiterLimit;
        if (root.Element(ns + SolidColor.SOLID_COLOR_NODEGROUP_IDENTIFIER)!.Element(ns + SolidColor.SOLID_COLOR_NODE_IDENTIFIER) != null)
            solidColor = new SolidColor(root.Element(ns + SolidColor.SOLID_COLOR_NODEGROUP_IDENTIFIER)!.Element(ns + SolidColor.SOLID_COLOR_NODE_IDENTIFIER)!);
        else if (root.Element(ns + LinearGradient.LINEAR_GRADIENT_NODEGROUP_IDENTIFIER)!.Element(ns + LinearGradient.LINEAR_GRADIENT_NODE_IDENTIFIER) != null)
            linearGradient = new LinearGradient(root.Element(ns + LinearGradient.LINEAR_GRADIENT_NODEGROUP_IDENTIFIER)!.Element(ns + LinearGradient.LINEAR_GRADIENT_NODE_IDENTIFIER)!);
        else if (root.Element(ns + RadialGradient.RADIAL_GRADIENT_NODEGROUP_IDENTIFIER)!.Element(ns + RadialGradient.RADIAL_GRADIENT_NODE_IDENTIFIER) != null)
            radialGradient = new RadialGradient(root.Element(ns + RadialGradient.RADIAL_GRADIENT_NODEGROUP_IDENTIFIER)!.Element(ns + RadialGradient.RADIAL_GRADIENT_NODE_IDENTIFIER)!);
        else if (root.Element(ns + BitmapFill.BITMAPFILL_NODEGROUP_IDENTIFIER)!.Element(ns + BitmapFill.BITMAPFILL_NODEGROUP_IDENTIFIER) != null)
            bitmapFill = new BitmapFill(root.Element(ns + BitmapFill.BITMAPFILL_NODEGROUP_IDENTIFIER)!.Element(ns + BitmapFill.BITMAPFILL_NODEGROUP_IDENTIFIER)!);
    }
    public Stroke(Stroke other)
    {
        root = new XElement(other.Root);
        ns = other.ns;
        scaleMode = other.scaleMode;
        caps = other.caps;
        pixelHinting = other.pixelHinting;
        joints = other.joints;
        weight = other.weight;
        miterLimit = other.miterLimit;
        SetSolidColor(other.SolidColor is null ? null : new SolidColor(other.SolidColor));
        SetLinearGradient(other.LinearGradient is null ? null : new LinearGradient(other.LinearGradient));
        SetRadialGradient(other.RadialGradient is null ? null : new RadialGradient(other.RadialGradient));
        SetBitmapFill(other.BitmapFill is null ? null : new BitmapFill(other.BitmapFill));
    }
    private void SetSolidColor(SolidColor? color)
    {
        solidColor = color;
        XElement? solidColorNode = root.Element(ns + SolidColor.SOLID_COLOR_NODEGROUP_IDENTIFIER)?.Element(ns + SolidColor.SOLID_COLOR_NODE_IDENTIFIER);
        solidColorNode?.Remove();
        if (color is not null)
        {
            XElement? solidColorNodegroup = root.Element(ns + SolidColor.SOLID_COLOR_NODEGROUP_IDENTIFIER);
            if (solidColorNodegroup is null)
            {
                solidColorNodegroup = new XElement(ns + SolidColor.SOLID_COLOR_NODEGROUP_IDENTIFIER);
                root.Add(solidColorNodegroup);
            }
            solidColorNodegroup.Add(color.Root);
        }
    }
    private void SetLinearGradient(LinearGradient? gradient)
    {
        linearGradient = gradient;
        XElement? linearGradientNode = root.Element(ns + LinearGradient.LINEAR_GRADIENT_NODEGROUP_IDENTIFIER)?.Element(ns + LinearGradient.LINEAR_GRADIENT_NODE_IDENTIFIER);
        linearGradientNode?.Remove();
        if (gradient is not null)
        {
            XElement? linearGradientNodegroup = root.Element(ns + LinearGradient.LINEAR_GRADIENT_NODEGROUP_IDENTIFIER);
            if (linearGradientNodegroup is null)
            {
                linearGradientNodegroup = new XElement(ns + LinearGradient.LINEAR_GRADIENT_NODEGROUP_IDENTIFIER);
                root.Add(linearGradientNodegroup);
            }
            linearGradientNodegroup.Add(gradient.Root);
        }
    }
    private void SetRadialGradient(RadialGradient? gradient)
    {
        radialGradient = gradient;
        XElement? radialGradientNode = root.Element(ns + RadialGradient.RADIAL_GRADIENT_NODEGROUP_IDENTIFIER)?.Element(ns + RadialGradient.RADIAL_GRADIENT_NODE_IDENTIFIER);
        radialGradientNode?.Remove();
        if (gradient is not null)
        {
            XElement? radialGradientNodegroup = root.Element(ns + RadialGradient.RADIAL_GRADIENT_NODEGROUP_IDENTIFIER);
            if (radialGradientNodegroup is null)
            {
                radialGradientNodegroup = new XElement(ns + RadialGradient.RADIAL_GRADIENT_NODEGROUP_IDENTIFIER);
                root.Add(radialGradientNodegroup);
            }
            radialGradientNodegroup.Add(gradient.Root);
        }
    }
    private void SetBitmapFill(BitmapFill? fill)
    {
        bitmapFill = fill;
        XElement? bitmapFillNode = root.Element(ns + BitmapFill.BITMAPFILL_NODEGROUP_IDENTIFIER)?.Element(ns + BitmapFill.BITMAPFILL_NODEGROUP_IDENTIFIER);
        bitmapFillNode?.Remove();
        if (fill is not null)
        {
            XElement? bitmapFillNodegroup = root.Element(ns + BitmapFill.BITMAPFILL_NODEGROUP_IDENTIFIER);
            if (bitmapFillNodegroup is null)
            {
                bitmapFillNodegroup = new XElement(ns + BitmapFill.BITMAPFILL_NODEGROUP_IDENTIFIER);
                root.Add(bitmapFillNodegroup);
            }
            bitmapFillNodegroup.Add(fill.Root);
        }
    }
}
public class SolidStroke : Stroke
{
    new public static class DefaultValues
    {
        public const string SolidStyle = "";
    }
    public const string SOLID_STROKE_NODE_IDENTIFIER = "SolidStroke";
    private List<WidthMarker>? widthMarkers;
    private string solidStyle;
    public string SolidStyle { get { return solidStyle; } set { solidStyle = value; root?.SetOrRemoveAttribute("solidStyle", value, DefaultValues.SolidStyle); } }
    public List<WidthMarker>? WidthMarkers { get { return widthMarkers; } set { SetWidthMarkers(value); } }
    public SolidStroke(XElement root) : base(root)
    {
        widthMarkers = root.Element(ns + WidthMarker.WIDTH_MARKERS_NODEGROUP_IDENTIFIER) is null ? null : root.Element(ns + WidthMarker.WIDTH_MARKERS_NODEGROUP_IDENTIFIER)!.Elements(ns + WidthMarker.WIDTH_MARKER_NODE_IDENTIFIER).Select(x => new WidthMarker(x)).ToList();
        solidStyle = root.Attribute("solidStyle")?.Value ?? DefaultValues.SolidStyle;
    }
    public SolidStroke(SolidStroke other) : base(other)
    {
        widthMarkers = other.widthMarkers?.Select(x => new WidthMarker(x)).ToList();
        SetWidthMarkers(widthMarkers);
        solidStyle = other.solidStyle;
    }
    private void SetWidthMarkers(List<WidthMarker>? widthMarkers)
    {
        root.Element(ns + WidthMarker.WIDTH_MARKERS_NODEGROUP_IDENTIFIER)?.Remove();
        if (widthMarkers is not null)
        {
            XElement widthMarkersNode = new(ns + WidthMarker.WIDTH_MARKERS_NODEGROUP_IDENTIFIER);
            foreach (WidthMarker widthMarker in widthMarkers)
            {
                widthMarkersNode.Add(widthMarker.Root);
            }
            root.Add(widthMarkersNode);
        }
    }
}
public class DashedStroke : Stroke
{
    public const string DASHED_STROKE_NODE_IDENTIFIER = "DashedStroke";
    public DashedStroke(XElement root) : base(root) { }
    public DashedStroke(DashedStroke other) : base(other) { }
}
public class DottedStroke : Stroke
{
    public const string DOTTED_STROKE_NODE_IDENTIFIER = "DottedStroke";
    private int dotSpace;
    public int DotSpace { get { return dotSpace; } set { dotSpace = value; root?.SetOrRemoveAttribute("dotSpace", value, 0); } }
    public DottedStroke(XElement root) : base(root)
    {
        dotSpace = (int?)root.Attribute("dotSpace") ?? throw new ArgumentNullException(nameof(root));
    }

    public DottedStroke(DottedStroke other) : base(other)
    {
        dotSpace = other.dotSpace;
    }

}
public class RaggedStroke : Stroke
{
    public const string RAGGED_STROKE_NODE_IDENTIFIER = "RaggedStroke";
    public RaggedStroke(XElement root) : base(root) { }
    public RaggedStroke(RaggedStroke other) : base(other) { }
}
public class StippleStroke : Stroke
{
    public const string STIPPLE_STROKE_NODE_IDENTIFIER = "StippleStroke";
    public StippleStroke(XElement root) : base(root) { }
    public StippleStroke(StippleStroke other) : base(other) { }
}
public class HatchedStroke : Stroke
{
    public const string HATCHED_STROKE_NODE_IDENTIFIER = "HatchedStroke";
    public HatchedStroke(XElement root) : base(root) { }
    public HatchedStroke(HatchedStroke other) : base(other) { }
}
public class WidthMarker
{
    public const string WIDTH_MARKER_NODE_IDENTIFIER = "WidthMarker",
    WIDTH_MARKERS_NODEGROUP_IDENTIFIER = "VariablePointWidth";
    private XElement root;
    public XElement Root { get { return root; } }
    private double position, left, right;
    private string? type;
    public double Position { get { return position; } set { position = value; root?.SetAttributeValue("position", value); } }
    public double Left { get { return left; } set { left = value; root?.SetAttributeValue("left", value); } }
    public double Right { get { return right; } set { right = value; root?.SetAttributeValue("right", value); } }
    public string? Type { get { return type; } set { type = value; root?.SetAttributeValue("type", value); } }
    internal WidthMarker(XElement root)
    {
        this.root = root;
        position = (double?)root.Attribute("position") ?? throw new ArgumentNullException(nameof(root));
        left = (double?)root.Attribute("left") ?? throw new ArgumentNullException(nameof(root));
        right = (double?)root.Attribute("right") ?? throw new ArgumentNullException(nameof(root));
        type = (string?)root.Attribute("type");
    }
    public WidthMarker(WidthMarker other)
    {
        root = new XElement(other.Root);
        position = other.position;
        left = other.left;
        right = other.right;
        type = other.type;
    }
}