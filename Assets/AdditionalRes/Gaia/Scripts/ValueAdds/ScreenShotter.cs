﻿using UnityEngine;
using System.Collections;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.UI;
#endif

namespace Gaia
{
    /// <summary>
    /// A simple screen shot taker - with thanks to @jamiepollard on the Gaia forum 
    /// Adapted from the original script here:
    /// http://answers.unity3d.com/questions/22954/how-to-save-a-picture-take-screenshot-from-a-camer.html
    /// </summary>
    public class ScreenShotter : MonoBehaviour
    {
        /// <summary>
        /// Key the screenshotter is bound to
        /// </summary>
        public KeyCode m_screenShotKey = KeyCode.F12;

        /// <summary>
        /// The storage format, JPG is smaller, PNG is higher quality
        /// </summary>
        public Gaia.GaiaConstants.StorageFormat m_imageFormat = GaiaConstants.StorageFormat.JPG;

        /// <summary>
        /// The target directory for the screenshots i.e. /Assets/targetdir
        /// </summary>
        public string m_targetDirectory = "Screenshots";

        /// <summary>
        /// Target screenshot width
        /// </summary>
        public int m_targetWidth = 1900;

        /// <summary>
        /// Target screenshot height
        /// </summary>
        public int m_targetHeight = 1200;

        /// <summary>
        /// If set the actual screen dimensions will be used instead of target dimensions
        /// </summary>
        public bool m_useScreenSize = false;

        /// <summary>
        /// The screen shot camera
        /// </summary>
        public Camera m_mainCamera;

        /// <summary>
        /// A toggle to cause the next updatre to take a shot
        /// </summary>
        private bool m_takeShot = false;

        /// <summary>
        /// A toggle to cause an asset db refresh
        /// </summary>
        private bool m_refreshAssetDB = false;

        /// <summary>
        /// Texture used for the watermark
        /// </summary>
        public Texture2D m_watermark;

        /// <summary>
        /// Sets up the camera if not already done
        /// </summary>
        void OnEnable()
        {
            if (m_mainCamera == null)
            {
                m_mainCamera = Camera.main;
            }

            //Create the target directory
            var path = Path.Combine(Application.dataPath, m_targetDirectory);
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
                #if UNITY_EDITOR
                AssetDatabase.Refresh();
                #endif
            }
        }

        /// <summary>
        /// Get unity to refresh the files list
        /// </summary>
        void OnDisable()
        {
            //Refresh the asset database
            if (m_refreshAssetDB)
            {
                m_refreshAssetDB = false;
                #if UNITY_EDITOR
                    AssetDatabase.Refresh();
                #endif
            }
        }

        /// <summary>
        /// Assigns a name to the screen shot - by default puts in in the assets directory
        /// </summary>
        /// <param name="width">Width of screenshot</param>
        /// <param name="height">Height of screenshot</param>
        /// <returns>Screen shot name and full path</returns>
        private string ScreenShotName(int width, int height)
        {
            var path = Path.Combine(Application.dataPath, m_targetDirectory);
            path = path.Replace('\\', '/');

            if (path[path.Length-1] == '/')
            {
                path = path.Substring(0, path.Length - 1);
            }

            if (m_imageFormat == GaiaConstants.StorageFormat.JPG)
            {
                return string.Format("{0}/Grab {1} w{2}h{3} x{4}y{5}z{6}r{7}.jpg",
                                     path,
                                     System.DateTime.Now.ToString("yyyyMMddHHmmss"),
                                     width,
                                     height,
                                     (int)m_mainCamera.transform.position.x,
                                     (int)m_mainCamera.transform.position.y,
                                     (int)m_mainCamera.transform.position.z,
                                     (int)m_mainCamera.transform.rotation.eulerAngles.y
                                     );
            }
            else
            {
                return string.Format("{0}/Grab {1} w{2}h{3} x{4}y{5}z{6}r{7}.png",
                                     path,
                                     System.DateTime.Now.ToString("yyyyMMdd HHmmss"),
                                     width,
                                     height,
                                     (int)m_mainCamera.transform.position.x,
                                     (int)m_mainCamera.transform.position.y,
                                     (int)m_mainCamera.transform.position.z,
                                     (int)m_mainCamera.transform.rotation.eulerAngles.y
                                     );
            }

        }

        /// <summary>
        /// Call this to take a screen shot in next late update
        /// </summary>
        public void TakeHiResShot()
        {
            m_takeShot = true;
        }

        /// <summary>
        /// Takes the actual screen shot when the key is pressed or takeshot is true
        /// </summary>
        void LateUpdate()
        {
            if (Input.GetKeyDown(m_screenShotKey) || m_takeShot)
            {
                //Pick up and use the actual screen dimensions
                if (m_useScreenSize == true)
                {
                    m_targetWidth = Screen.width;
                    m_targetHeight = Screen.height;
                }

                m_refreshAssetDB = true;
                var rt = new RenderTexture(m_targetWidth, m_targetHeight, 24);
                m_mainCamera.targetTexture = rt;
                var screenShot = new Texture2D(m_targetWidth, m_targetHeight, TextureFormat.RGB24, false);
                m_mainCamera.Render();
                RenderTexture.active = rt;
                screenShot.ReadPixels(new Rect(0, 0, m_targetWidth, m_targetHeight), 0, 0);
                m_mainCamera.targetTexture = null;
                RenderTexture.active = null; // JC: added to avoid errors
                Destroy(rt);

                if (m_watermark != null)
                {
                    Gaia.Utils.MakeTextureReadable(m_watermark);
                    screenShot = AddWatermark(screenShot, m_watermark);
                }

                var bytes = screenShot.EncodeToJPG();
                var filename = ScreenShotName(m_targetWidth, m_targetHeight);
                Gaia.Utils.WriteAllBytes(filename, bytes);
                m_takeShot = false;
                Debug.Log(string.Format("Took screenshot to: {0}", filename));
            }
        }

        public Texture2D AddWatermark(Texture2D background, Texture2D watermark)
        {
            var startX = background.width - watermark.width - 10;
            var endX = startX + watermark.width;
            //int startY = background.height - watermark.height - 20;
            var startY = 8;
            var endY = startY + watermark.height;

            for (var x = startX; x < endX; x++)
            {
                for (var y = startY; y < endY; y++)
                {
                    var bgColor = background.GetPixel(x, y);
                    var wmColor = watermark.GetPixel(x - startX, y - startY);
                    var final_color = Color.Lerp(bgColor, wmColor, wmColor.a / 1.0f);
                    background.SetPixel(x, y, final_color);
                }
            }

            background.Apply();
            return background;
        }
    }
}