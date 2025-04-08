using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Mediapipe.Tasks.Components.Containers;
using Mediapipe.Tasks.Vision.HandLandmarker;
using Mediapipe.Tasks.Vision.ImageSegmenter;
using SLRGTk.Camera;
using SLRGTk.Common;
using SLRGTk.Model;
using UnityEngine;

namespace Model {

    public class VisionEngineState {
        public HandLandmarkerResult? Landmarks = null;
        public DetectionResult? Detection = null;
        public ImageSegmenterResult? Segmentation = null;
    }
    public class VisionEngine : MonoBehaviour {
        public StreamCamera camera;
        public MPHands hands;
        public MPObjDet objDet;
        public MPSegmenter segmenter;
        public Buffer<HandLandmarkerResult> buffer = new();
        public LiteRTPopsignIsolatedSLR recognizer;

        private long currentTimestamp;
        private VisionEngineState currentState;
        
        private ConcurrentDictionary<long, VisionEngineState> stateBuffer;

        public VisionEngine() {
            hands = new MPHands(Resources.Load<TextAsset>("hand_landmarker.task").bytes);
            recognizer = new LiteRTPopsignIsolatedSLR(Resources.Load<TextAsset>("563-double-lstm-120-cpu.tflite").bytes,
                Resources.Load<TextAsset>("signsList").text.Split("\n").Select(line => line.Trim()).ToList());
            
            
            objDet = new MPObjDet(Resources.Load<TextAsset>("efficientdet_lite0_float32.tflite").bytes);
            segmenter = new MPSegmenter(Resources.Load<TextAsset>("deeplab_v3.tflite").bytes);

            camera = gameObject.AddComponent<StreamCamera>();
            PermissionManager.RequestCameraPermission();
            camera.AddCallback("HandLandmarker", input => {
                hands.Run(input);
            });
            camera.AddCallback("ObjectDetector", input => {
                objDet.Run(input);
            });
            camera.AddCallback("ImageSegmenter", input => {
                segmenter.Run(input);
            });
            hands.AddCallback("BufferFiller", output => {
                if (output.Result.handLandmarks != null && output.Result.handLandmarks.Count > 0) {
                    // TODO: clear the buffer if there are too many blanks in succession
                    buffer.AddElement(output.Result);
                }
            });
            hands.AddCallback("StateUpdater", output => {
                
            });
            buffer.AddCallback("BufferPrinter", result => {
                Debug.Log("Buffer Triggered at: " + result.Count  + " Elements");
            });
            buffer.AddCallback("RecoginzerChain", result => {
                recognizer.Run(new(result));
                buffer.Clear();
            });
            recognizer.AddCallback("Recoginzer Result", result => {
                Debug.Log("Recoginzer Result: " + result);
            });
        }

        private void CheckOrAddState(long timestamp) {
            if (!stateBuffer.ContainsKey(timestamp)) {
                stateBuffer.TryAdd(timestamp, new VisionEngineState());
            }
        }

        public void AddState(long timestamp, HandLandmarkerResult landmarks) {
            CheckOrAddState(timestamp);
            stateBuffer[timestamp].Landmarks = landmarks;
            CheckUpdate(timestamp);
        }
        public void AddState(long timestamp, DetectionResult detection) {
            CheckOrAddState(timestamp);
            stateBuffer[timestamp].Detection = detection;
            CheckUpdate(timestamp);
            
        }
        public void AddState(long timestamp, ImageSegmenterResult segmentation) {
            CheckOrAddState(timestamp);
            stateBuffer[timestamp].Segmentation = segmentation;
            CheckUpdate(timestamp);
        }

        public void CheckUpdate(long timestamp) {
            if (stateBuffer[timestamp].Landmarks != null &&
                stateBuffer[timestamp].Detection != null &&
                stateBuffer[timestamp].Segmentation != null) {
                if (timestamp > currentTimestamp) {
                    currentTimestamp = timestamp;
                    currentState = stateBuffer[timestamp];
                }
            }
            foreach (var stateTimestamp in stateBuffer)
            {
                if (timestamp < currentTimestamp)
                {
                    stateBuffer.Remove(timestamp, out var texture);
                }
            }
        }
    }
}