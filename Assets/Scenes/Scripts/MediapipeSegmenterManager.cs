using System;
using Mediapipe;
using Mediapipe.Tasks.Vision.ImageSegmenter;
using UnityEngine;
using RunningMode = Mediapipe.Tasks.Vision.Core.RunningMode;
using System.Collections.Concurrent;
using System.Collections.Generic;
using SLRGTk.Common;
using SLRGTk.Model;
using Unity.Collections;


namespace Model
{

    public class MPSegmenterOutput {
        public NativeArray<byte> OriginalImage { get; }
        public int Width { get; }
        public int Height { get; }
        
        public long Timestamp { get; }
        public ImageSegmenterResult Result { get; }


        // constructor
        public MPSegmenterOutput(NativeArray<byte> originalImage, ImageSegmenterResult result, int width, int height, long timestamp) {
            OriginalImage = originalImage;
            Result = result;
            Width = width;
            Height = height;
            Timestamp = timestamp;
        }
    }
    public class MPSegmenter
    {
        private readonly ImageSegmenter graph;

        private readonly Dictionary<string, Action<MPSegmenterOutput>> callbacks = new();
        private readonly ConcurrentDictionary<long, MPVisionInput> outputInputLookup = new();
        private readonly RunningMode runningMode;

        private static class Config
        {
            public static readonly float CATEGORY_CONFIDENCE = 0.3f;
            public static readonly bool OUTPUT_CATEGORY_MASK = true;
        }
        public MPSegmenter(byte[] modelAssetBuffer, Mediapipe.Tasks.Vision.Core.RunningMode runningMode = Mediapipe.Tasks.Vision.Core.RunningMode.LIVE_STREAM)
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
                            var matchedImg = outputInputLookup.GetValueOrDefault(timestampMs);
                            cb(new MPSegmenterOutput(
                                matchedImg.Image,
                                i,
                                matchedImg.Width,
                                matchedImg.Height,
                                timestampMs
                            ));
                        }
                        outputInputLookup.Remove(timestampMs, out var _);
                        foreach (var timestamp in outputInputLookup.Keys)
                        {
                            if (timestamp < timestampMs)
                            {
                                outputInputLookup.Remove(timestamp, out var texture);
                            }
                        }
                    }
                ));
            }
        }
        private int imageCounter = 0; // Counter for image filenames
        
        public void Run(MPVisionInput input)
        {
            var img = new Image(TextureFormat.RGBA32.ToImageFormat(), input.Width, input.Height, TextureFormat.RGBA32.ToImageFormat().NumberOfChannels() * input.Width, input.Image);
            switch (runningMode)
            {
                case Mediapipe.Tasks.Vision.Core.RunningMode.LIVE_STREAM:
                    outputInputLookup[input.Timestamp] = input;
                    graph.SegmentAsync(img, input.Timestamp);
                    break;
                case Mediapipe.Tasks.Vision.Core.RunningMode.IMAGE:
                    var result = graph.Segment(img);
                    break;
                case Mediapipe.Tasks.Vision.Core.RunningMode.VIDEO:
                    var videoResult = graph.SegmentForVideo(img, input.Timestamp);
                    foreach (var cb in callbacks.Values)
                    {
                        cb(new MPSegmenterOutput(input.Image, videoResult, input.Width, input.Height, input.Timestamp));
                    }
                    break;

            }
        }
        public void AddCallback(string name, Action<MPSegmenterOutput> callback)
        {
            callbacks[name] = callback;        
        }
        public void RemoveCallback(string name)
        {
            callbacks.Remove(name);
        }
    }
}

