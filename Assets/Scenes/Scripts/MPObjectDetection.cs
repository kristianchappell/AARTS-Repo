
using Mediapipe;
using Mediapipe.Tasks.Vision.ObjectDetector;
using UnityEngine;
using RunningMode = Mediapipe.Tasks.Vision.Core.RunningMode;
using System.Collections;
using System.Linq;
using UnityEngine.Video;
using System.Collections.Generic;
using UnityEngine.Rendering;

namespace Model
{
    public class MPObjectDetection
    {
        private ObjectDetector graph;
        private readonly TextAsset detectionModel = new TextAsset(Application.dataPath + "Assets/SLR-GTk/Package/Dependencies/com.github.homuler.mediapipe/PackageResources/MediaPipe/efficientdet_lite0_float32.bytes");
        
        public MPObjectDetection(TextAsset model)
        {
            graph = ObjectDetector.CreateFromOptions(
                new ObjectDetectorOptions(
                    new Mediapipe.Tasks.Core.BaseOptions(
                        modelAssetBuffer: model.bytes
                    ),
                    runningMode: RunningMode.IMAGE,
                    maxResults: 6,
                    scoreThreshold: 0.1f
                )
            );
        }
        private bool Intersect(Vector2 vec, Mediapipe.Tasks.Components.Containers.Detection det)
        {
            var bbox = det.boundingBox;
            return (bbox.left <= vec[0] && bbox.right >= vec[0]) && (bbox.top <= vec[1] && bbox.bottom >= vec[1]);
        }

        public ArrayList ProcessScreen(Texture2D rt, Vector2 tapLoc)
        {
            int pixelWidth = rt.width;
            int pixelHeight = rt.height;

            var detectionResults = graph.Detect(new Mediapipe.Image(TextureFormat.RGBA32.ToImageFormat(), rt));

            //Debug.Log(detectionResults);
            
            ArrayList detections = new ArrayList();
            string bestName = "";
            float score = -100.0f;
            Mediapipe.Tasks.Components.Containers.Detection detObj;
            if (detectionResults.detections != null)
            {
                foreach (Mediapipe.Tasks.Components.Containers.Detection det in detectionResults.detections)
                {
                    if (Intersect(tapLoc, det))
                    {
                        foreach (Mediapipe.Tasks.Components.Containers.Category cat in det.categories)
                        {
                            Debug.Log(cat.categoryName);
                            if (cat.score >= score || isValid(cat.categoryName))
                            {
                                score = cat.score;
                                bestName = cat.categoryName;
                                detObj = det;
                            }
                        }
                    }
                }
            }
            //Debug.Log(score);
            //Debug.Log(bestName);
            if (bestName != "")
            {
                detections.Add(bestName);
            }
            Debug.Log(bestName);
            return detections;
        }

        private bool isValid(string catName)
        {
            VideoClip[] videoClips = Resources.LoadAll<VideoClip>("SigningVideos/dpan_source_videos");

            List<string> aslList = videoClips.Select(clip => clip.name).ToList();

            return aslList.Contains(catName);
        }
    }

}

