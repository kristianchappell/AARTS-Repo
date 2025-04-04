using System;
using Mediapipe;
using Mediapipe.Tasks.Vision.ImageSegmenter;
using UnityEngine;
using RunningMode = Mediapipe.Tasks.Vision.Core.RunningMode;
using System.Collections.Concurrent;
using System.Collections.Generic;


namespace Model
{

    public class MediapipeObjectSegmenterModelManager
    {
        private readonly ImageSegmenter graph;

        private readonly Dictionary<string, Action<ImageMPResultWrapper<ImageSegmenterResult>>> callbacks = new();
        private readonly ConcurrentDictionary<long, Texture2D> outputInputLookup = new();
        private readonly RunningMode runningMode;

        private static class Config
        {
            public static readonly float CATEGORY_CONFIDENCE = 0.3f;
            public static readonly bool OUTPUT_CATEGORY_MASK = true;
        }
        public MediapipeObjectSegmenterModelManager(byte[] modelAssetBuffer, Mediapipe.Tasks.Vision.Core.RunningMode runningMode)
        {
            this.runningMode = runningMode;
            if (runningMode != Mediapipe.Tasks.Vision.Core.RunningMode.LIVE_STREAM)
            {
                graph = ImageSegmenter.CreateFromOptions(new ImageSegmenterOptions(
                    new Mediapipe.Tasks.Core.BaseOptions(
                        modelAssetBuffer: modelAssetBuffer
                        ),
                    runningMode: runningMode,
                    outputCategoryMask: Config.OUTPUT_CATEGORY_MASK
                    ));
            } else
            {
                graph = ImageSegmenter.CreateFromOptions(new ImageSegmenterOptions(
                    new Mediapipe.Tasks.Core.BaseOptions(
                        modelAssetBuffer: modelAssetBuffer
                        ),
                    runningMode: runningMode,
                    outputCategoryMask: true,
                    resultCallback: (i, _, timestampMs) =>
                    {
                        if (!outputInputLookup.ContainsKey(timestampMs)) return;
                        foreach (var cb in callbacks.Values)
                        {
                            cb(new ImageMPResultWrapper<ImageSegmenterResult>(
                                i,
                                outputInputLookup.GetValueOrDefault(timestampMs)
                            ));
                        }
                        outputInputLookup.Remove(timestampMs, out var _);
                        foreach (var timestamp in outputInputLookup.Keys)
                        {
                            if (timestamp < timestampMs)
                            {
                                outputInputLookup.Remove(timestamp, out var texture);
                                CustomTextureManager.ScheduleDeletion(texture);
                            }
                        }
                    }
                ));
            }
        }
        private int imageCounter = 0; // Counter for image filenames
        
        public void Single(Texture2D image, long timestamp)
        {
            Image img = new Image(image.format.ToImageFormat(), image);
            switch (runningMode)
            {
                case Mediapipe.Tasks.Vision.Core.RunningMode.LIVE_STREAM:
                    outputInputLookup[timestamp] = image;
                    graph.SegmentAsync(img, timestamp);
                    break;
                case Mediapipe.Tasks.Vision.Core.RunningMode.IMAGE:
                    var result = graph.Segment(img);
                    break;
                case Mediapipe.Tasks.Vision.Core.RunningMode.VIDEO:
                    var videoResult = graph.SegmentForVideo(img, timestamp);
                    foreach (var cb in callbacks.Values)
                    {
                        cb(new ImageMPResultWrapper<ImageSegmenterResult>(videoResult, image));
                    }
                    break;

            }
        }
        public void AddCallback(string name, Action<ImageMPResultWrapper<ImageSegmenterResult>> callback)
        {
            callbacks[name] = callback;        
        }
        public void RemoveCallback(string name)
        {
            callbacks.Remove(name);
        }
    }
}

