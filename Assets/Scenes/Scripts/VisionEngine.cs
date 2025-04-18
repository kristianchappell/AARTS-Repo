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
        public DetectionResult? Detection = null;
        public ImageSegmenterResult? Segmentation = null;
    }
    public class VisionEngine : MonoBehaviour {
        public StreamCamera frontCamera;
        public ARCamera backCamera;
        public MPHands hands;
        public MPObjDet objDet;
        public MPSegmenter segmenter;
        public Buffer<HandLandmarkerResult> buffer = new();
        public LiteRTPopsignIsolatedSLR recognizer;

        private long currentTimestamp;
        public VisionEngineState currentState;
        public ClassPredictions? handResult = null;
        
        private ConcurrentDictionary<long, VisionEngineState> stateBuffer;

        [SerializeField] TextAsset detebytes;
        [SerializeField] TextAsset segbytes;

        public void Awake() {

            Debug.Log(WebCamTexture.devices[1]);

            hands = new MPHands(Resources.Load<TextAsset>("hand_landmarker.task").bytes);
            recognizer = new LiteRTPopsignIsolatedSLR(Resources.Load<TextAsset>("563-double-lstm-120-cpu.tflite").bytes,
                Resources.Load<TextAsset>("signsList").text.Split("\n").Select(line => line.Trim()).ToList());
            
            
            objDet = new MPObjDet(detebytes.bytes);
            segmenter = new MPSegmenter(segbytes.bytes);

            frontCamera = gameObject.AddComponent<StreamCamera>();
            frontCamera.Pause();
            backCamera = gameObject.AddComponent<ARCamera>();
            PermissionManager.RequestCameraPermission();
            frontCamera.AddCallback("HandLandmarker", input => {
                hands.Run(input);
            });
            backCamera.AddCallback("ObjectDetector", input => {
                objDet.Run(input);
            });
            backCamera.AddCallback("ImageSegmenter", input => {
                segmenter.Run(input);
            });
            backCamera.AddCallback("Debug", _ => throw new System.Exception("IDK"));
            objDet.AddCallback("VisionEngine State", AddState);
            segmenter.AddCallback("VisionEngine State", AddState);
            hands.AddCallback("BufferFiller", output => {
                if (output.Result.handLandmarks != null && output.Result.handLandmarks.Count > 0) {
                    // TODO: clear the buffer if there are too many blanks in succession
                    buffer.AddElement(output.Result);
                }
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
                
                handResult = result;
            });
        }

        private void CheckOrAddState(long timestamp) {
            if (!stateBuffer.ContainsKey(timestamp)) {
                stateBuffer.TryAdd(timestamp, new VisionEngineState());
            }
        }
        
        public void AddState(MPObjDetOutput output) {
            CheckOrAddState(output.Timestamp);
            stateBuffer[output.Timestamp].Detection = output.Result;
            CheckUpdate(output.Timestamp);
            
        }
        public void AddState(MPSegmenterOutput output) {
            CheckOrAddState(output.Timestamp);
            stateBuffer[output.Timestamp].Segmentation = output.Result;
            CheckUpdate(output.Timestamp);
        }

        public void CheckUpdate(long timestamp) {
            if (stateBuffer[timestamp].Detection != null &&
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

        public void StopAllCameras() {
            backCamera.Pause();
            frontCamera.Pause();
        }

        public void StartBackCamera() {
            frontCamera.Pause();
            backCamera.Poll();
        }

        public void StartFrontCamera() {
            backCamera.Pause();
            frontCamera.Poll();
        }
    }
}