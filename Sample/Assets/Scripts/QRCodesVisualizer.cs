// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using UnityEngine;

// ... (existing using statements)

namespace Microsoft.MixedReality.SampleQRCodes
{
    public class QRCodesVisualizer : MonoBehaviour
    {
        public GameObject qrCodePrefab;
        public GameObject cubeQR;  // Assign this in the Unity Inspector to the CubeQR GameObject

        private SortedDictionary<System.Guid, GameObject> qrCodesObjectsList;
        private bool clearExisting = false;
        private System.Guid targetQRCodeId = System.Guid.Empty;  // ID of the QR code to attach CubeQR to
        private bool isCubeQRAttached = false;

        struct ActionData
        {
            public enum Type
            {
                Added,
                Updated,
                Removed
            };
            public Type type;
            public Microsoft.MixedReality.QR.QRCode qrCode;

            public ActionData(Type type, Microsoft.MixedReality.QR.QRCode qRCode) : this()
            {
                this.type = type;
                qrCode = qRCode;
            }
        }

        private Queue<ActionData> pendingActions = new Queue<ActionData>();

        // Use this for initialization
        void Start()
        {
            Debug.Log("QRCodesVisualizer start");
            qrCodesObjectsList = new SortedDictionary<System.Guid, GameObject>();

            QRCodesManager.Instance.QRCodesTrackingStateChanged += Instance_QRCodesTrackingStateChanged;
            QRCodesManager.Instance.QRCodeAdded += Instance_QRCodeAdded;
            QRCodesManager.Instance.QRCodeUpdated += Instance_QRCodeUpdated;
            QRCodesManager.Instance.QRCodeRemoved += Instance_QRCodeRemoved;
            if (qrCodePrefab == null)
            {
                throw new System.Exception("Prefab not assigned");
            }
        }

        private void Instance_QRCodesTrackingStateChanged(object sender, bool status)
        {
            if (!status)
            {
                clearExisting = true;
            }
        }

        private void Instance_QRCodeAdded(object sender, QRCodeEventArgs<Microsoft.MixedReality.QR.QRCode> e)
        {
            Debug.Log("QRCodesVisualizer Instance_QRCodeAdded");

            lock (pendingActions)
            {
                pendingActions.Enqueue(new ActionData(ActionData.Type.Added, e.Data));
            }
        }

        private void Instance_QRCodeUpdated(object sender, QRCodeEventArgs<Microsoft.MixedReality.QR.QRCode> e)
        {
            Debug.Log("QRCodesVisualizer Instance_QRCodeUpdated");

            lock (pendingActions)
            {
                pendingActions.Enqueue(new ActionData(ActionData.Type.Updated, e.Data));
            }
        }

        private void Instance_QRCodeRemoved(object sender, QRCodeEventArgs<Microsoft.MixedReality.QR.QRCode> e)
        {
            Debug.Log("QRCodesVisualizer Instance_QRCodeRemoved");

            lock (pendingActions)
            {
                pendingActions.Enqueue(new ActionData(ActionData.Type.Removed, e.Data));
            }
        }

        private void HandleEvents()
        {
            lock (pendingActions)
            {
                while (pendingActions.Count > 0)
                {
                    var action = pendingActions.Dequeue();
                    if (action.type == ActionData.Type.Added || action.type == ActionData.Type.Updated)
                    {
                        // Check if the QR code data is "Q67"
                        if (action.qrCode.Data == "Q67")
                        {
                            targetQRCodeId = action.qrCode.Id;  // Store the ID of Q67
                            isCubeQRAttached = true;  // Set the flag indicating the CubeQR should be attached

                            // Create a new GameObject for Q67 if it doesn't exist
                            if (!qrCodesObjectsList.ContainsKey(action.qrCode.Id))
                            {
                                GameObject qrCodeObject = Instantiate(qrCodePrefab, Vector3.zero, Quaternion.identity);
                                qrCodeObject.GetComponent<SpatialGraphNodeTracker>().Id = action.qrCode.SpatialGraphNodeId;
                                qrCodeObject.GetComponent<QRCode>().qrCode = action.qrCode;
                                qrCodesObjectsList.Add(action.qrCode.Id, qrCodeObject);
                            }

                            // Attach CubeQR to the QR code GameObject
                            AttachCubeQRToQRCode(qrCodesObjectsList[action.qrCode.Id]);
                        }
                        else
                        {
                            // Handle other QR codes as usual
                            if (!qrCodesObjectsList.ContainsKey(action.qrCode.Id))
                            {
                                GameObject qrCodeObject = Instantiate(qrCodePrefab, Vector3.zero, Quaternion.identity);
                                qrCodeObject.GetComponent<SpatialGraphNodeTracker>().Id = action.qrCode.SpatialGraphNodeId;
                                qrCodeObject.GetComponent<QRCode>().qrCode = action.qrCode;
                                qrCodesObjectsList.Add(action.qrCode.Id, qrCodeObject);
                            }
                        }
                    }
                    else if (action.type == ActionData.Type.Removed)
                    {
                        if (qrCodesObjectsList.ContainsKey(action.qrCode.Id))
                        {
                            Destroy(qrCodesObjectsList[action.qrCode.Id]);
                            qrCodesObjectsList.Remove(action.qrCode.Id);
                        }

                        // If the removed QR code is Q67, detach CubeQR
                        if (action.qrCode.Id == targetQRCodeId)
                        {
                            DetachCubeQR();
                        }
                    }
                }
            }
            if (clearExisting)
            {
                clearExisting = false;
                foreach (var obj in qrCodesObjectsList)
                {
                    Destroy(obj.Value);
                }
                qrCodesObjectsList.Clear();
                DetachCubeQR();  // Detach CubeQR when clearing all QR codes
            }
        }

        private void AttachCubeQRToQRCode(GameObject qrCodeObject)
        {
            if (cubeQR != null)
            {
                cubeQR.transform.SetParent(qrCodeObject.transform);  // Attach CubeQR to the QR code
                cubeQR.transform.localPosition = Vector3.zero;  // Reset position relative to the QR code
                cubeQR.transform.localRotation = Quaternion.identity;  // Reset rotation relative to the QR code
            }
        }

        private void DetachCubeQR()
        {
            if (cubeQR != null && isCubeQRAttached)
            {
                cubeQR.transform.SetParent(null);  // Detach CubeQR from the QR code
                isCubeQRAttached = false;  // Reset the attachment flag
            }
        }

        // Update is called once per frame
        void Update()
        {
            HandleEvents();
        }
    }
}


