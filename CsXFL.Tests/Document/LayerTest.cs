using System.Xml.Linq;
using System.Reflection;
namespace CsXFL.Tests;
public class LayerTests
{
    // test file has these layers:
    /*
    Layer_1
    Layer_2
    Layer_3
    Layer_4
    folder
        folderlayer
        folderfolder
            folderfolderlayer
        folderlayer_2
    nonfolderlayer
    */
    // ONLY Layer_2 has any content, each non-folder layer has one
    // keyframe and has 3766 frames, except for Layer_1, which has 2 keyframes at the start.
    [Fact]
    public void ClearKeyframe_ShouldRemoveKeyframe_WhenOnTypicalKeyframe()
    {
        // Arrange
        Document doc = new("TestAssets/DOMDocument.xml");
        Timeline timeline = doc.GetTimeline(0);
        Layer layer = timeline.Layers[0];
        Frame removed = layer.KeyFrames[1];
        XElement frameRoot = removed.Root!;
        int numFrames = layer.GetFrameCount();
        int numKeyframes = layer.KeyFrames.Count;
        // Act
        layer.ClearKeyframe(1);
        // Assert
        Assert.Equal(numFrames, layer.GetFrameCount());
        Assert.Equal(numKeyframes - 1, layer.KeyFrames.Count);
        Assert.DoesNotContain(removed, layer.KeyFrames);
        Assert.True(layer.Root!.Elements().All(e => e.Name != frameRoot.Name || e != frameRoot));
    }
    [Fact]
    public void ClearKeyframe_ShouldBeNoop_OnNonKeyframe()
    {
        // Arrange
        Document doc = new("TestAssets/DOMDocument.xml");
        Timeline timeline = doc.GetTimeline(0);
        Layer layer = timeline.Layers[1];
        // Act
        bool result = layer.ClearKeyframe(1);
        // Assert
        Assert.False(result);

    }
    [Fact]
    public void ConvertToKeyframes_ShouldConvertSingleFrameToKeyframe()
    {
        // Arrange
        Document doc = new("TestAssets/DOMDocument.xml");
        Timeline timeline = doc.GetTimeline(0);
        Layer layer = timeline.Layers[0];
        int startFrame = 5;
        int endFrame = 5;
        int numKeyframes = layer.KeyFrames.Count;
        int numFrames = layer.GetFrameCount();

        // Act
        bool result = layer.ConvertToKeyframes(startFrame, endFrame);

        // Assert
        Assert.True(result);
        Assert.True(layer.GetFrameCount() == numFrames);
        Assert.True(layer.KeyFrames.Count == numKeyframes + 1);
        Assert.True(layer.GetFrame(startFrame).StartFrame == startFrame);
    }

    [Fact]
    public void ConvertToKeyframes_ShouldConvertRangeOfFramesToKeyframes()
    {
        // Arrange
        Document doc = new("TestAssets/DOMDocument.xml");
        Timeline timeline = doc.GetTimeline(0);
        Layer layer = timeline.Layers[0];
        int startFrame = 10;
        int endFrame = 15;
        int numKeyframes = layer.KeyFrames.Count;
        int numFrames = layer.GetFrameCount();

        // Act
        bool result = layer.ConvertToKeyframes(startFrame, endFrame);

        // Assert
        Assert.True(result);
        Assert.True(layer.GetFrameCount() == numFrames);
        Assert.True(layer.KeyFrames.Count == numKeyframes + (endFrame - startFrame + 1));
        for (int i = startFrame; i <= endFrame; i++)
        {
            Assert.True(layer.GetFrame(i).StartFrame == i);
        }
    }

    [Fact]
    public void ConvertToKeyframes_ShouldNotConvertExistingKeyframes()
    {
        // Arrange
        Document doc = new("TestAssets/DOMDocument.xml");
        Timeline timeline = doc.GetTimeline(0);
        Layer layer = timeline.Layers[0];
        int startFrame = 0;
        int endFrame = 1;
        int numKeyframes = layer.KeyFrames.Count;
        int numFrames = layer.GetFrameCount();

        // Act
        bool result = layer.ConvertToKeyframes(startFrame, endFrame);

        // Assert
        Assert.False(result);
        Assert.True(layer.GetFrameCount() == numFrames);
        Assert.True(layer.KeyFrames.Count == numKeyframes);
        for (int i = startFrame; i <= endFrame; i++)
        {
            Assert.True(layer.GetFrame(i).StartFrame == i);
        }
    }

    [Fact]
    public void InsertFrames_ShouldInsertCorrectNumberOfFrames()
    {
        // Arrange
        Document doc = new("TestAssets/DOMDocument.xml");
        Timeline timeline = doc.GetTimeline(0);
        Layer layer = timeline.Layers[0];
        int numFrames = layer.GetFrameCount();

        // Act
        layer.InsertFrames(5,0);

        // Assert
        Assert.True(layer.GetFrameCount() == numFrames + 5);
        Assert.True(layer.IsKeyFrame(6));   // new keyframe should be at 6
        Assert.True(layer.IsKeyFrame(0));   // start keyframe still exists
    }
    
    [Fact]
    public void RemoveFrames_ShouldRemoveCorrectNumberOfFrames()
    {
        // Arrange
        Document doc = new("TestAssets/DOMDocument.xml");
        Timeline timeline = doc.GetTimeline(0);
        Layer layer = timeline.Layers[0];
        int numFrames = layer.GetFrameCount();
        int keyFrames = layer.KeyFrames.Count;

        // Act
        layer.RemoveFrames(5,0);

        // Assert
        Assert.True(layer.GetFrameCount() == numFrames - 5);
        Assert.True(layer.IsKeyFrame(0));   // start keyframe still exists
        Assert.True(layer.KeyFrames.Count == keyFrames - 1); // 1 keyframe should be gone in this instance
    }

    [Fact]
    public void CreateMotionTween_ShouldSetEndFrameEqual_WhenEndFrameIsNull()
    {
        // Arrange
        Document doc = new("TestAssets/DOMDocument.xml");
        Timeline timeline = doc.GetTimeline(0);
        Layer layer = timeline.Layers[1];
        int startFrame = 69;
        int nextKeyFrame = layer.GetFrame(70).StartFrame + layer.GetFrame(69).Duration;


        // Act
        // layer.ConvertToKeyframes(400, 718);
        layer.CreateMotionTween(startFrame);
        

        // Assert
        Assert.False(layer.GetFrame(startFrame).IsEmpty());

        Assert.True(layer.GetFrame(startFrame).TweenType == "motion");
        // Assert.False(layer.GetFrame(nextKeyFrame).TweenType == "motion");

        Assert.True(layer.GetFrame(startFrame).MotionTweenSnap);
        // Assert.False(layer.GetFrame(nextKeyFrame).MotionTweenSnap);

        Assert.True(layer.GetFrame(startFrame).EaseMethodName == "none");
        // Reflection nonsense under construction
        // Type frameType = typeof(Frame);
        // FieldInfo[] fields = frameType.GetFields(
        //                  BindingFlags.NonPublic | 
        //                  BindingFlags.Instance);
        // Assert.True(layer.GetFrame(startFrame));

    }



}