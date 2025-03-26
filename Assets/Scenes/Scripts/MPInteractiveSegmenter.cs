using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Mediapipe;
using Mediapipe.Tasks.Vision.ImageSegmenter;
using Mediapipe.Tasks.Vision.ObjectDetector;
using Mediapipe.Tasks.Components.Containers;
using Model;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using RunningMode = Mediapipe.Tasks.Vision.Core.RunningMode;
using ImageProcessingOptions = Mediapipe.Tasks.Vision.Core.ImageProcessingOptions;
using UnityEngine.Rendering;

namespace Model
{
    public class MediapipeInteractiveSegmenterModelManager: MonoBehaviour
    {
        private ImageSegmenter graph;
        private ObjectDetector graph2;
        [SerializeField] private TextAsset segmenterModel; //segmenter
        [SerializeField] private TextAsset detectionModel; //classifier
        [SerializeField] private UnityEngine.Camera camera;
        [SerializeField] private Vector2 tapLoc;

        private RenderTexture rt;

        //private readonly Dictionary<string, Action<ImageMPResultWrapper<ImageSegmenterResult>>> callbacks = new();
        //private readonly ConcurrentDictionary<long, Texture2D> outputInputLookup = new();
        //private readonly RunningMode runningMode;


        //public MediapipeInteractiveSegmenterModelManager(byte[] modelAssetBuffer, Mediapipe.Tasks.Vision.Core.RunningMode runningMode)
        //{
        //    this.runningMode = runningMode;
        //    graph = ImageSegmenter.CreateFromOptions(new ImageSegmenterOptions(
        //        new Mediapipe.Tasks.Core.BaseOptions(
        //            modelAssetBuffer: modelAssetBuffer
        //        ),
        //        outputCategoryMask: true,
        //        outputConfidenceMasks: false,
        //        runningMode: runningMode
        //    ));
        //}
        //public Image Single(Texture2D image, double x=0.5, double y=0.5)
        //{
        //    Image img = new Image(image.format.ToImageFormat(), image);
        //    ImageProcessingOptions roi = new ImageProcessingOptions(regionOfInterest: new RectF(x, y, x, y),
        //                                                            rotationDegrees: 0);
        //    return graph.Segment(img, roi).categoryMask;

        //}

        void Awake()
        {

            rt = RenderTexture.GetTemporary(camera.pixelWidth, camera.pixelHeight);
            //graph = ImageSegmenter.CreateFromOptions(
            //    new ImageSegmenterOptions(
            //            new Mediapipe.Tasks.Core.BaseOptions(
            //                modelAssetBuffer: segmenterModel.bytes
            //            ),
            //            runningMode: RunningMode.LIVE_STREAM,
            //            outputCategoryMask: true,
            //            outputConfidenceMasks: false,
            //            resultCallback: (segmentResults, image, timestamp) => {
            //                Debug.Log(segmentResults.categoryMask);
            //            }
            //        )
            //    );

            bool intersect(Vector2 vec, Mediapipe.Tasks.Components.Containers.Detection det)
            {
                var bbox = det.boundingBox;
                return (bbox.left <= vec[0] && bbox.right >= vec[0]) && (bbox.top <= vec[1] && bbox.bottom >= vec[1]);
            }

            graph2 = ObjectDetector.CreateFromOptions(
                new ObjectDetectorOptions(
                        new Mediapipe.Tasks.Core.BaseOptions(
                            modelAssetBuffer: detectionModel.bytes
                        ),
                        runningMode: RunningMode.LIVE_STREAM,
                        maxResults: 6,
                        scoreThreshold: 0.2f,
                        resultCallback: (detectionResults, image, timestamp) => {
                            if (tapLoc == null) {
                                tapLoc = new Vector2(0, 0);
                            }


                            if (detectionResults.detections != null) {
                                foreach (Mediapipe.Tasks.Components.Containers.Detection det in detectionResults.detections) {
                                    Debug.Log(det);
                                    Debug.Log(intersect(tapLoc, det));
                                    Debug.Log(det.categories[0].categoryName);
                                }
                            } else {
                                Debug.Log(null);
                            }
                        }
                    )
                );


            camera.targetTexture = rt;
        }

        private long i = 0;
        private void Update()
        {
            i++;
            var req = AsyncGPUReadback.Request(rt, 0, TextureFormat.RGBA32, (AsyncGPUReadbackRequest request) => {
                graph2.DetectAsync(new Mediapipe.Image(TextureFormat.RGBA32.ToImageFormat(), camera.pixelWidth, camera.pixelHeight, TextureFormat.RGBA32.ToImageFormat().NumberOfChannels() * camera.pixelWidth, request.GetData<byte>()), (long) Time.realtimeSinceStartup*1000);
            });
            Debug.Log(Time.realtimeSinceStartup);
        }
    }
}
