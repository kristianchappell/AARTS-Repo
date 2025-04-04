using System;
using Mediapipe;
using Mediapipe.Tasks.Vision.ObjectDetector;
using UnityEngine;
using RunningMode = Mediapipe.Tasks.Vision.Core.RunningMode;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Mediapipe.Tasks.Components.Containers;


namespace Model
{

    public class MediapipeObjectDetectorModelManager
    {
        private readonly ObjectDetector graph;

        private readonly Dictionary<string, Action<ImageMPResultWrapper<DetectionResult>>> callbacks = new();
        private readonly ConcurrentDictionary<long, Texture2D> outputInputLookup = new();
        private readonly RunningMode runningMode;

        private static class Config
        {
            public static readonly int MAX_RESULTS = 6;
            public static readonly float MIN_SCORE_THRESHOLD = 0.1f;
        }
        public MediapipeObjectDetectorModelManager(byte[] modelAssetBuffer, Mediapipe.Tasks.Vision.Core.RunningMode runningMode)
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
                        foreach (var cb in callbacks.Values)
                        {
                            cb(new ImageMPResultWrapper<DetectionResult>(
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
                    graph.DetectAsync(img, timestamp);
                    break;
                case Mediapipe.Tasks.Vision.Core.RunningMode.IMAGE:
                    var result = graph.Detect(img);
                    break;
                case Mediapipe.Tasks.Vision.Core.RunningMode.VIDEO:
                    var videoResult = graph.DetectForVideo(img, timestamp);
                    foreach (var cb in callbacks.Values)
                    {
                        cb(new ImageMPResultWrapper<DetectionResult>(videoResult, image));
                    }
                    break;

            }
        }
        public void AddCallback(string name, Action<ImageMPResultWrapper<DetectionResult>> callback)
        {
            callbacks[name] = callback;
        }
        public void RemoveCallback(string name)
        {
            callbacks.Remove(name);
        }
    }
}

