using System;
using System.Collections;
using SLRGTk.Camera;
using SLRGTk.Common;
using SLRGTk.Model;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

    public class ARCamera : MonoBehaviour, ICamera, ICallback<MPVisionInput> {
        private readonly CallbackManager<MPVisionInput> _callbackManagerProxy = new();

        public bool polling = true;

        public void Poll() {
            Debug.Log("Polling");
            polling = true;
            // TODO: start the camera
        }

        public void Pause() {
            // turns off polling
            polling = false;
            // TODO:  pause the camera
        }

        private IEnumerator Run() {
            if (polling) {
                RenderTexture tempRT = /*...*/ null; //TODO: assign camera value
                // essentially going through all the callbacks and calling them on an image
                var req = AsyncGPUReadback.Request(tempRT, 0, TextureFormat.RGBA32, (AsyncGPUReadbackRequest request) => {
                    // var dest = new Texture2D(
                    //     _webCamTexture.videoRotationAngle % 180 == 0 ? _webCamTexture.width : _webCamTexture.height, // webCamTexture.width, 
                    //     _webCamTexture.videoRotationAngle % 180 == 0 ? _webCamTexture.height : _webCamTexture.width, // webCamTexture.height, 
                    //     TextureFormat.RGBA32,
                    //     false);                        
                    // checks through each callback
                    foreach (var callback in _callbackManagerProxy.callbacks) {
                        // checks to see if GPU is giving some kind of error
                        if (request.hasError)
                            Debug.LogError("GPU readback error.");
                        // calls the callback on the image
                        // TODO: synchronize in a queue
                        callback.Value(new MPVisionInput(request.GetData<byte>(), DateTimeOffset.Now.ToUnixTimeMilliseconds(), _webCamTexture.width, _webCamTexture.height));
                    }
                    // releasing the temporary workspace to be used in the next run through
                    // RenderTexture.ReleaseTemporary(tempRT);
                });
                // pauses the execution until we get the value of req is set and then returns
                yield return req;
            }
        }

        public void Update() {
            StartCoroutine(Run());
        }

        public void AddCallback(string callbackName, Action<MPVisionInput> callback) {
            Debug.Log("Adding Callback");
            _callbackManagerProxy.AddCallback(callbackName, callback);
        }
        public void RemoveCallback(string callbackName) {
            _callbackManagerProxy.RemoveCallback(callbackName);
        }
        public void TriggerCallbacks(MPVisionInput value) {
            _callbackManagerProxy.TriggerCallbacks(value);
        }
        public void ClearCallbacks() {
            _callbackManagerProxy.ClearCallbacks();
        }
    }
