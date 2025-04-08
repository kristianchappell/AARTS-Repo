using System;
using Mediapipe;
using Mediapipe.Tasks.Vision.ObjectDetector;
using UnityEngine;
using RunningMode = Mediapipe.Tasks.Vision.Core.RunningMode;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Mediapipe.Tasks.Components.Containers;
using SLRGTk.Common;
using SLRGTk.Model;
using Unity.Collections;


namespace Model
{

    public class MPObjDetOutput {
        public NativeArray<byte> OriginalImage { get; }
        public int Width { get; }
        public int Height { get; }
        public DetectionResult Result { get; }


        // constructor
        public MPObjDetOutput(NativeArray<byte> originalImage, DetectionResult result, int width, int height) {
            OriginalImage = originalImage;
            Result = result;
            Width = width;
            Height = height;
        }
    }
    public class MPObjDet
    {
        private readonly ObjectDetector graph;

        private readonly Dictionary<string, Action<MPObjDetOutput>> callbacks = new();
        private readonly ConcurrentDictionary<long, MPVisionInput> outputInputLookup = new();
        private readonly RunningMode runningMode;

        private static class Config
        {
            public static readonly int MAX_RESULTS = 6;
            public static readonly float MIN_SCORE_THRESHOLD = 0.1f;
        }
        public MPObjDet(byte[] modelAssetBuffer, Mediapipe.Tasks.Vision.Core.RunningMode runningMode = Mediapipe.Tasks.Vision.Core.RunningMode.LIVE_STREAM)
        {
            this.runningMode = runningMode;
            if (runningMode != Mediapipe.Tasks.Vision.Core.RunningMode.LIVE_STREAM)
            {
                graph = ObjectDetector.CreateFromOptions(new ObjectDetectorOptions(
                    new Mediapipe.Tasks.Core.BaseOptions(
                        modelAssetBuffer: modelAssetBuffer
                        ),
                    runningMode: runningMode,
                    maxResults: Config.MAX_RESULTS,
                    scoreThreshold: Config.MIN_SCORE_THRESHOLD
                    ));
            }
            else
            {
                graph = ObjectDetector.CreateFromOptions(new ObjectDetectorOptions(
                    new Mediapipe.Tasks.Core.BaseOptions(
                        modelAssetBuffer: modelAssetBuffer
                        ),
                    runningMode: runningMode,
                    maxResults: Config.MAX_RESULTS,
                    scoreThreshold: Config.MIN_SCORE_THRESHOLD,
                    resultCallback: (i, _, timestampMs) =>
                    {
                        if (!outputInputLookup.ContainsKey(timestampMs)) return;
                        foreach (var cb in callbacks.Values) {
                            var matchedImg = outputInputLookup.GetValueOrDefault(timestampMs);
                            cb(new MPObjDetOutput(
                                matchedImg.Image,
                                i,
                                matchedImg.Width,
                                matchedImg.Height
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
                    graph.DetectAsync(img, input.Timestamp);
                    break;
                case Mediapipe.Tasks.Vision.Core.RunningMode.IMAGE:
                    var result = graph.Detect(img);
                    break;
                case Mediapipe.Tasks.Vision.Core.RunningMode.VIDEO:
                    var videoResult = graph.DetectForVideo(img, input.Timestamp);
                    foreach (var cb in callbacks.Values)
                    {
                        cb(new MPObjDetOutput(input.Image, videoResult, input.Width, input.Height));
                    }
                    break;

            }
        }
        public void AddCallback(string name, Action<MPObjDetOutput> callback)
        {
            callbacks[name] = callback;
        }
        public void RemoveCallback(string name)
        {
            callbacks.Remove(name);
        }
    }
}

