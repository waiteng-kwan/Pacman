using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Client
{
    public class UiCamera : MonoBehaviour
    {
        public static UiCamera CameraInstance;

        private void Awake()
        {
            CameraInstance = this;
        }
    }
}