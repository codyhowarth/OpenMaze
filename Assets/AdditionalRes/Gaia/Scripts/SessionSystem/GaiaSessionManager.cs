﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Gaia.FullSerializer;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Gaia
{
    /// <summary>
    /// Gaia scene tecording and playback system
    /// </summary>
    [ExecuteInEditMode]
    public class GaiaSessionManager : MonoBehaviour
    {
        /// <summary>
        /// Use for co-routine simulation
        /// </summary>
        public IEnumerator m_updateSessionCoroutine;

        /// <summary>
        /// Use for co-routine simulation
        /// </summary>
        public IEnumerator m_updateOperationCoroutine;

        /// <summary>
        /// Used to signal a cancelled playback
        /// </summary>
        private bool m_cancelPlayback = false;

        /// <summary>
        /// The session we are managing
        /// </summary>
        public GaiaSession m_session;

        /// <summary>
        /// Public variables used by the terrain generator
        /// </summary>
        public bool m_genShowRandomGenerator = false;
        public bool m_genShowTerrainHelper = false;
        public Gaia.GaiaConstants.GeneratorBorderStyle m_genBorderStyle = Gaia.GaiaConstants.GeneratorBorderStyle.Water;
        public int m_genNumStampsToGenerate = 10;
        public float m_genScaleWidth = 10f;
        public float m_genScaleHeight = 4f;
        public float m_genChanceOfHills = 0.7f;
        public float m_genChanceOfIslands = 0f;
        public float m_genChanceOfLakes = 0f;
        public float m_genChanceOfMesas = 0.1f;
        public float m_genChanceOfMountains = 0.1f;
        public float m_genChanceOfPlains = 0f;
        public float m_genChanceOfRivers = 0.1f; //
        public float m_genChanceOfValleys = 0f;
        public float m_genChanceOfVillages = 0f; //
        public float m_genChanceOfWaterfalls = 0f; //

        [fsIgnore]
        public Stamper m_currentStamper = null;

        [fsIgnore]
        public Spawner m_currentSpawner = null;

        [fsIgnore]
        public DateTime m_lastUpdateDateTime = DateTime.Now;

        [fsIgnore]
        public ulong m_progress = 0;

        /// <summary>
        /// Private vairables used by the terrain generator
        /// </summary>
        private List<string> m_genHillStamps = new List<string>();
        private List<string> m_genIslandStamps = new List<string>();
        private List<string> m_genLakeStamps = new List<string>();
        private List<string> m_genMesaStamps = new List<string>();
        private List<string> m_genMountainStamps = new List<string>();
        private List<string> m_genPlainsStamps = new List<string>();
        private List<string> m_genRiverStamps = new List<string>();
        private List<string> m_genValleyStamps = new List<string>();
        private List<string> m_genVillageStamps = new List<string>();
        private List<string> m_genWaterfallStamps = new List<string>();

        /// <summary>
        /// Get the session manager
        /// </summary>
        public static GaiaSessionManager GetSessionManager(bool pickupExistingTerrain = false)
        {
            //Find or create gaia
            var gaiaObj = GameObject.Find("Gaia");
            if (gaiaObj == null)
            {
                gaiaObj = new GameObject("Gaia");
            }
            GaiaSessionManager sessionMgr = null;
            var mgrObj = GameObject.Find("Session Manager");
            if (mgrObj == null)
            {
                mgrObj = new GameObject("Session Manager");
                sessionMgr = mgrObj.AddComponent<GaiaSessionManager>();
                sessionMgr.CreateSession(pickupExistingTerrain);
                mgrObj.transform.parent = gaiaObj.transform;
                mgrObj.transform.position = Gaia.TerrainHelper.GetActiveTerrainCenter();
            }
            else
            {
                sessionMgr = mgrObj.GetComponent<GaiaSessionManager>();
            }
            return sessionMgr;
        }

        /// <summary>
        /// Current lock status of the session
        /// </summary>
        /// <returns>True if the session is locked false otherwise</returns>
        public bool IsLocked()
        {
            if (m_session == null)
            {
                CreateSession();
            }
            return m_session.m_isLocked;
        }

        /// <summary>
        /// Lock the session, return previous lock state
        /// </summary>
        /// <returns>Previous lock state</returns>
        public bool LockSession()
        {
            if (m_session == null)
            {
                CreateSession();
            }

            var prevLockState = m_session.m_isLocked;
            m_session.m_isLocked = true;
            if (prevLockState == false)
            {
                SaveSession();
            }
            return prevLockState;
        }

        /// <summary>
        /// Un lock the session, return previous lock state
        /// </summary>
        /// <returns>Previous lock state</returns>
        public bool UnLockSession()
        {
            if (m_session == null)
            {
                CreateSession();
            }

            var prevLockState = m_session.m_isLocked;
            m_session.m_isLocked = false;
            if (prevLockState == true)
            {
                SaveSession();
            }
            return prevLockState;
        }

        /// <summary>
        /// Add an operation to the session
        /// </summary>
        /// <param name="operation"></param>
        public void AddOperation(GaiaOperation operation)
        {
            if (IsLocked())
            {
                Debug.Log("Cant add operation on locked session");
                return;
            }
            m_session.m_operations.Add(operation);
            SaveSession();
        }

        /// <summary>
        /// Get the operation with the supplied index
        /// </summary>
        /// <param name="operationIdx">Operation index</param>
        /// <returns>Operation or null if index out of brounds</returns>
        public GaiaOperation GetOperation(int operationIdx)
        {
            if (m_session == null)
            {
                CreateSession();
            }
            if (operationIdx < 0 || operationIdx >= m_session.m_operations.Count)
            {
                return null;
            }
            return m_session.m_operations[operationIdx];
        }

        /// <summary>
        /// Remove the operation at the supplied index - ignores if undex out of bounds
        /// </summary>
        /// <param name="operationIdx">Operation index</param>
        public void RemoveOperation(int operationIdx)
        {
            if (IsLocked())
            {
                Debug.Log("Cant remove operation on locked session");
                return;
            }
            if (operationIdx < 0 || operationIdx >= m_session.m_operations.Count)
            {
                return;
            }
            m_session.m_operations.RemoveAt(operationIdx);
            SaveSession();
        }

        /// <summary>
        /// Add a resources file if its not already there
        /// </summary>
        /// <param name="resource">Resource to be added</param>
        public void AddResource(GaiaResource resource)
        {
            if (IsLocked())
            {
                Debug.Log("Cant add resource on locked session");
                return;
            }
            if (resource != null)
            {
                if (!m_session.m_resources.ContainsKey(resource.m_resourcesID + resource.name))
                {
                    //Get the raw resource and add it into the dictionary
                    #if UNITY_EDITOR
                    var so = new ScriptableObjectWrapper();
                    so.m_name = resource.m_name;
                    so.m_fileName = AssetDatabase.GetAssetPath(resource);
                    so.m_content = Gaia.Utils.ReadAllBytes(so.m_fileName);
                    if (so.m_content != null && so.m_content.GetLength(0) > 0)
                    {
                        m_session.m_resources.Add(resource.m_resourcesID + resource.name, so);
                        SaveSession();
                    }
                    #endif
                }
            }
        }

        /// <summary>
        /// Add a defaults file if its not already there
        /// </summary>
        /// <param name="defaults">Resource to be added</param>
        public void AddDefaults(GaiaDefaults defaults)
        {
            if (IsLocked())
            {
                Debug.Log("Cant add defaults on locked session");
                return;
            }

            if (defaults != null)
            {
                //Get the raw resource and add it into the dictionary
                #if UNITY_EDITOR
                m_session.m_defaults = new ScriptableObjectWrapper();
                m_session.m_defaults.m_name = "Defaults";
                m_session.m_defaults.m_fileName = AssetDatabase.GetAssetPath(defaults);
                m_session.m_defaults.m_content = Gaia.Utils.ReadAllBytes(m_session.m_defaults.m_fileName);
                SaveSession();
                #endif
            }
        }

        /// <summary>
        /// Add the preview image
        /// </summary>
        /// <param name="image">The image to add</param>
        public void AddPreviewImage(Texture2D image)
        {
            if (IsLocked())
            {
                Debug.Log("Cant add preview on locked session");
                return;
            }
            m_session.m_previewImageWidth = image.width;
            m_session.m_previewImageHeight = image.height;
            m_session.m_previewImageBytes = image.GetRawTextureData();
            SaveSession();
        }

        /// <summary>
        /// Whether or not the session has a preview image
        /// </summary>
        /// <returns>Returns true if the session has a preview image</returns>
        public bool HasPreviewImage()
        {
            if (m_session.m_previewImageWidth > 0 && m_session.m_previewImageHeight > 0 && m_session.m_previewImageBytes.GetLength(0) > 0)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Remove the preview image
        /// </summary>
        public void RemovePreviewImage()
        {
            if (IsLocked())
            {
                Debug.Log("Cant remove preview on locked session");
                return;
            }
            m_session.m_previewImageWidth = 0;
            m_session.m_previewImageHeight = 0;
            m_session.m_previewImageBytes = new byte[0];
            SaveSession();
        }

        /// <summary>
        /// Get the embedded preview image or null
        /// </summary>
        /// <returns>Embedded preview image or null</returns>
        public Texture2D GetPreviewImage()
        {
            if (m_session.m_previewImageBytes.GetLength(0) == 0)
            {
                return null;
            }

            var image = new Texture2D(m_session.m_previewImageWidth, m_session.m_previewImageHeight, TextureFormat.ARGB32, false);
            image.LoadRawTextureData(m_session.m_previewImageBytes);
            image.Apply();

            //Do a manual colour mod if in linear colour space
            #if UNITY_EDITOR
            if (PlayerSettings.colorSpace == ColorSpace.Linear)
            {
                var pixels = image.GetPixels();
                for (var idx = 0; idx < pixels.GetLength(0); idx++)
                {
                    pixels[idx] = pixels[idx].gamma;
                }
                image.SetPixels(pixels);
                image.Apply();
            }
            #endif



            image.name = m_session.m_name;
            return image;
        }

        /// <summary>
        /// Force unity to save the session
        /// </summary>
        public void SaveSession()
        {
            #if UNITY_EDITOR
            EditorUtility.SetDirty(m_session);
            AssetDatabase.SaveAssets();
            #endif
        }

        /// <summary>
        /// Start editor updates
        /// </summary>
        public void StartEditorUpdates()
        {
            #if UNITY_EDITOR
            EditorApplication.update += EditorUpdate;
            #endif
        }

        /// <summary>
        /// Stop editor updates
        /// </summary>
        public void StopEditorUpdates()
        {
            //For editor update purposes
            m_currentSpawner = null;
            m_currentStamper = null;
            m_updateOperationCoroutine = null;
            m_updateSessionCoroutine = null;

            #if UNITY_EDITOR
            EditorApplication.update -= EditorUpdate;
            #endif
        }

        /// <summary>
        /// This is executed only in the editor - using it to simulate co-routine execution and update execution
        /// </summary>
        void EditorUpdate()
        {
            if (m_cancelPlayback)
            {
                if (m_currentSpawner != null)
                {
                    m_currentSpawner.CancelSpawn();
                }
                if (m_currentStamper != null)
                {
                    m_currentStamper.CancelStamp();
                }
                StopEditorUpdates();
            }
            else
            {
                if (m_updateSessionCoroutine == null && m_updateOperationCoroutine == null)
                {
                    StopEditorUpdates();
                }
                else
                {
                    if (m_updateOperationCoroutine != null)
                    {
                        m_updateOperationCoroutine.MoveNext();
                    }
                    else
                    {
                        m_updateSessionCoroutine.MoveNext();
                    }
                }
            }
        }

        /// <summary>
        /// Will create a session - and if in the editor, also save it to disk
        /// </summary>
        /// <returns></returns>
        public GaiaSession CreateSession(bool pickupExistingTerrain = false)
        {
            m_session = ScriptableObject.CreateInstance<Gaia.GaiaSession>();

            m_session.m_description = "Rocking out at Creativity Central! If you like Gaia please consider rating it :)";

            //Grab the sea level from the default resources file
            var settings = Gaia.Utils.GetGaiaSettings();
            if (settings != null)
            {
                if (settings.m_currentDefaults != null)
                {
                    m_session.m_seaLevel = settings.m_currentDefaults.m_seaLevel;
                }
            }

            //Lets see if we can pick up some defaults from the extisting terrain if there is one
            var t = Gaia.TerrainHelper.GetActiveTerrain();
            if (t != null)
            {
                m_session.m_terrainWidth = (int)t.terrainData.size.x;
                m_session.m_terrainDepth = (int)t.terrainData.size.z;
                m_session.m_terrainHeight = (int)t.terrainData.size.y;

                //Pick up existing terrain
                if (pickupExistingTerrain)
                {
                    var defaults = ScriptableObject.CreateInstance<Gaia.GaiaDefaults>();
                    defaults.UpdateFromTerrain();

                    var resources = ScriptableObject.CreateInstance<Gaia.GaiaResource>();
                    resources.UpdatePrototypesFromTerrain();
                    resources.ChangeSeaLevel(m_session.m_seaLevel);

                    AddDefaults(defaults);
                    AddResource(resources);
                    AddOperation(defaults.GetTerrainCreationOperation(resources));
                }
            }
            else
            {
                if (settings != null && settings.m_currentDefaults != null)
                {
                    m_session.m_terrainWidth = settings.m_currentDefaults.m_terrainSize;
                    m_session.m_terrainDepth = settings.m_currentDefaults.m_terrainHeight;
                    m_session.m_terrainHeight = settings.m_currentDefaults.m_terrainSize;
                }
            }

            #if UNITY_EDITOR
            AssetDatabase.CreateAsset(m_session, string.Format("Assets/Gaia/Data/GS-{0:yyyyMMdd-HHmmss}.asset", DateTime.Now));
            AssetDatabase.SaveAssets();
            #endif
            return m_session;
        }

        /// <summary>
        /// Set the session sea level - this will influence the spawners and the resources they use
        /// </summary>
        /// <param name="seaLevel"></param>
        public void SetSeaLevel(float seaLevel)
        {
            m_session.m_seaLevel = seaLevel;
        }

        /// <summary>
        /// Get the session sea level
        /// </summary>
        /// <returns></returns>
        public float GetSeaLevel()
        {
            return m_session.m_seaLevel;
        }


        /// <summary>
        /// Reset the session
        /// </summary>
        public void ResetSession()
        {
            //Check we have a session
            if (m_session == null)
            {
                Debug.LogError("Can not erase the session as there is no existing session!");
                return;
            }

            //Check session not locked
            if (m_session.m_isLocked == true)
            {
                Debug.LogError("Can not erase the session as it is locked!");
                return;
            }

            if (m_session.m_operations.Count > 1)
            {
                //Keep the create terrain operation
                var firstOp = m_session.m_operations[0];
                m_session.m_operations.Clear();
                if (firstOp.m_operationType == GaiaOperation.OperationType.CreateTerrain)
                {
                    AddOperation(firstOp);
                }
            }
        }


        /// <summary>
        /// Create randomise the stamps in a session
        /// </summary>
        public void RandomiseStamps()
        {
            //Check we have a session
            if (m_session == null)
            {
                Debug.LogError("Can not randomise stamps as there is no existing session!");
                return;
            }

            //Check session not locked
            if (m_session.m_isLocked == true)
            {
                Debug.LogError("Can not randomise stamps as the existing session is locked!");
                return;
            }

            //Check we have an active terrain (really should be able to create one)
            var terrain = TerrainHelper.GetActiveTerrain();
            if (terrain == null)
            {
                //Pick up current settings
                var settings = (GaiaSettings)Utils.GetAssetScriptableObject("GaiaSettings");
                if (settings == null)
                {
                    Debug.LogError("Can not randomise stamps as we are missing the terrain and settings!");
                    return;                
                }

                //Grab defaults n settings
                var defaults = settings.m_currentDefaults;
                var resources = settings.m_currentResources;

                if (defaults == null || resources == null)
                {
                    Debug.LogError("Can not randomise stamps as we are missing the terrain defaults or resources!");
                    return;                
                }

                //Create the terrain
                defaults.CreateTerrain(resources);
            }

            //Get its bounds
            var terrainBounds = new Bounds();
            TerrainHelper.GetTerrainBounds(terrain, ref terrainBounds);

            //Create stamper
            var gaiaObj = GameObject.Find("Gaia");
            if (gaiaObj == null)
            {
                gaiaObj = new GameObject("Gaia");
            }
            Stamper stamper = null;
            var stamperObj = GameObject.Find("Stamper");
            if (stamperObj == null)
            {
                stamperObj = new GameObject("Stamper");
                stamperObj.transform.parent = gaiaObj.transform;
                stamper = stamperObj.AddComponent<Stamper>();
            }
            else
            {
                stamper = stamperObj.GetComponent<Stamper>();
            }

            //Ok now randomly get some stamps and assemble them into a scene
            for (var stampIdx = 0; stampIdx < m_genNumStampsToGenerate; stampIdx++)
            {
                var stampPath = "";
                var featureType = GaiaConstants.FeatureType.Hills;

                #if UNITY_EDITOR

                if (stampIdx == 0)
                {
                    if (m_genBorderStyle != GaiaConstants.GeneratorBorderStyle.Mountains)
                    {
                        featureType = GetWeightedRandomFeatureType();
                        stampPath = GetRandomStampPath(featureType);
                    }
                    else
                    {
                        featureType = GaiaConstants.FeatureType.Mountains;
                        stampPath = GetRandomMountainFieldPath();
                    }
                }
                else
                {
                    featureType = GetWeightedRandomFeatureType();
                    stampPath = GetRandomStampPath(featureType);
                }

                //Check to see if we got something useful - if not then drop out
                if (string.IsNullOrEmpty(stampPath))
                {
                    continue;
                }
                stampPath = stampPath.Replace('\\', '/');
                stampPath = stampPath.Replace(Application.dataPath + "/", "Assets/");
                #endif

                //Do the basic load and initialise
                stamper.LoadStamp(stampPath);
                stamper.FitToTerrain();
                stamper.HidePreview();

                //Then customise
                if (stampIdx == 0)
                {
                    var fullWidth = stamper.m_width;
                    PositionStamp(terrainBounds, stamper, featureType);
                    stamper.m_rotation = 0f;
                    stamper.m_x = 0f;
                    stamper.m_z = 0f;
                    stamper.m_width = fullWidth;
                    if (m_genBorderStyle == GaiaConstants.GeneratorBorderStyle.Mountains)
                    {
                        stamper.m_distanceMask = new AnimationCurve(new Keyframe(1f, 1f), new Keyframe(1f, 1f));
                        stamper.m_areaMaskMode = GaiaConstants.ImageFitnessFilterMode.ImageGreyScale;
                        stamper.m_imageMask = Gaia.Utils.GetAsset("Island Mask 1.jpg", typeof(Texture2D)) as Texture2D;
                        stamper.m_imageMaskNormalise = true;
                        stamper.m_imageMaskInvert = true;
                    }
                    else
                    {
                        stamper.m_distanceMask = new AnimationCurve(new Keyframe(1f, 1f), new Keyframe(1f, 1f));
                        stamper.m_areaMaskMode = GaiaConstants.ImageFitnessFilterMode.ImageGreyScale;
                        stamper.m_imageMask = Gaia.Utils.GetAsset("Island Mask 1.jpg", typeof(Texture2D)) as Texture2D;
                        stamper.m_imageMaskNormalise = true;
                        stamper.m_imageMaskInvert = false;
                    }
                }
                else
                {
                    PositionStamp(terrainBounds, stamper, featureType);

                    //Randomly make to an inverted stamp and subtract it
                    var featureSelector = UnityEngine.Random.Range(0f, 1f);

                    if (featureSelector < 0.1f)
                    {
                        stamper.m_stampOperation = GaiaConstants.FeatureOperation.LowerHeight;
                        stamper.m_invertStamp = true;
                    }
                    else if (featureSelector < 0.35f)
                    {
                        stamper.m_stampOperation = GaiaConstants.FeatureOperation.StencilHeight;
                        stamper.m_normaliseStamp = true;

                        if (featureType == GaiaConstants.FeatureType.Rivers || featureType == GaiaConstants.FeatureType.Lakes)
                        {
                            stamper.m_invertStamp = true;
                            stamper.m_stencilHeight = UnityEngine.Random.Range(-80f, -5f);
                        }
                        else
                        {
                            if (UnityEngine.Random.Range(0f, 1f) < 0.5f)
                            {
                                stamper.m_invertStamp = true;
                                stamper.m_stencilHeight = UnityEngine.Random.Range(-80f, -5f);
                            }
                            else
                            {
                                stamper.m_invertStamp = false;
                                stamper.m_stencilHeight = UnityEngine.Random.Range(5f, 80f);
                            }
                        }
                    }
                    else
                    {
                        stamper.m_stampOperation = GaiaConstants.FeatureOperation.RaiseHeight;
                        stamper.m_invertStamp = false;
                    }

                    //Also explore stenciling rivers
                    //- normalise - invert - negative height

                }

                //And finally update and add to session
                stamper.UpdateStamp();
                stamper.AddToSession(GaiaOperation.OperationType.Stamp, "Stamping " + stamper.m_stampPreviewImage.name);
            }
        }

        private void PositionStamp(Bounds bounds, Stamper stamper, GaiaConstants.FeatureType stampType)
        {
            var stampBaseLevel = 0f;
            var stampMinHeight = 0f;
            var stampMaxHeight = 0f;
            var fullHeight = stamper.m_height * 4f;

            var terrainWaterLevel = 0f;
            if (m_session.m_terrainHeight > 0f)
            {
                terrainWaterLevel = m_session.m_seaLevel / (float)m_session.m_terrainHeight;
            }

            //Get some basic info about the stamp and then make semi intelligent decisions about the size and placement
            if (stamper.GetHeightRange(ref stampBaseLevel, ref stampMinHeight, ref stampMaxHeight))
            {
                //stampRange = stampMaxHeight - stampMinHeight;
                //Debug.Log(string.Format("Base {0:0.000} Min {1:0.000} Max {2:0.000} Range {3:0.000} Water {4:0.000}", stampBaseLevel, stampMinHeight, stampMaxHeight, stampRange, terrainWaterLevel));

                //By default we are raising height
                stamper.m_stampOperation = GaiaConstants.FeatureOperation.RaiseHeight;

                //And not inverting or any other weirdness
                stamper.m_invertStamp = false;
                stamper.m_normaliseStamp = false;

                //Set stamp width, height and rotation
                stamper.m_rotation = UnityEngine.Random.Range(-179, 179f);
                stamper.m_width = UnityEngine.Random.Range(0.7f, 1.3f) * m_genScaleWidth;
                stamper.m_height = UnityEngine.Random.Range(0.7f, 1.3f) * m_genScaleHeight; // *stampRange;

                //Set stamp offset accounting for water level
                var relativeHeight = (stamper.m_height / fullHeight) * m_session.m_terrainHeight;
                var relativeZero = relativeHeight / 2f;
                var waterTrue = terrainWaterLevel * m_session.m_terrainHeight;
                stamper.m_stickBaseToGround = false;
                stamper.m_y = relativeZero + waterTrue - (stampBaseLevel * relativeHeight);

                //Set stamp position
                var offsetRange = 1f;
                if (m_genBorderStyle == GaiaConstants.GeneratorBorderStyle.None)
                {
                    stamper.m_x = UnityEngine.Random.Range(-bounds.extents.x, bounds.extents.x);
                    stamper.m_z = UnityEngine.Random.Range(-bounds.extents.z, bounds.extents.z);
                }
                else
                {
                    offsetRange = 0.65f;
                    stamper.m_x = UnityEngine.Random.Range(-(bounds.extents.x * offsetRange), bounds.extents.x * offsetRange);
                    stamper.m_z = UnityEngine.Random.Range(-(bounds.extents.z * offsetRange), bounds.extents.z * offsetRange);
                }

                //Set the mask
                stamper.m_distanceMask = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0f));
                stamper.m_areaMaskMode = GaiaConstants.ImageFitnessFilterMode.None;
                stamper.m_imageMask = null;
            }
        }

        /// <summary>
        /// Get a semi random feature type weighted by its relative chance of success
        /// </summary>
        /// <returns>Feature</returns>
        GaiaConstants.FeatureType GetWeightedRandomFeatureType()
        {
            //Choose a random number
            var randomPick = UnityEngine.Random.Range(0f, 1f);

            //Work out the random number ranges
            var sumRanges = m_genChanceOfHills + m_genChanceOfIslands + m_genChanceOfLakes + m_genChanceOfMesas + m_genChanceOfMountains +
                m_genChanceOfPlains + m_genChanceOfRivers + m_genChanceOfValleys  + m_genChanceOfVillages + m_genChanceOfWaterfalls;

            //Stop divide by zero
            if (sumRanges == 0f)
            {
                sumRanges = 1f;
            }

            //Set our way through it - crude but effective
            var currStep = 0f;
            var nextStep = 0f;

            nextStep = currStep + (m_genChanceOfHills / sumRanges);
            if (randomPick >= currStep && randomPick < nextStep)
            {
                return GaiaConstants.FeatureType.Hills;
            }

            currStep = nextStep;
            nextStep = currStep + (m_genChanceOfIslands / sumRanges);
            if (randomPick >= currStep && randomPick < nextStep)
            {
                return GaiaConstants.FeatureType.Islands;
            }

            currStep = nextStep;
            nextStep = currStep + (m_genChanceOfLakes / sumRanges);
            if (randomPick >= currStep && randomPick < nextStep)
            {
                return GaiaConstants.FeatureType.Lakes;
            }

            currStep = nextStep;
            nextStep = currStep + (m_genChanceOfMesas / sumRanges);
            if (randomPick >= currStep && randomPick < nextStep)
            {
                return GaiaConstants.FeatureType.Mesas;
            }

            currStep = nextStep;
            nextStep = currStep + (m_genChanceOfMountains / sumRanges);
            if (randomPick >= currStep && randomPick < nextStep)
            {
                return GaiaConstants.FeatureType.Mountains;
            }

            currStep = nextStep;
            nextStep = currStep + (m_genChanceOfPlains / sumRanges);
            if (randomPick >= currStep && randomPick < nextStep)
            {
                return GaiaConstants.FeatureType.Plains;
            }

            currStep = nextStep;
            nextStep = currStep + (m_genChanceOfRivers / sumRanges);
            if (randomPick >= currStep && randomPick < nextStep)
            {
                return GaiaConstants.FeatureType.Rivers;
            }

            currStep = nextStep;
            nextStep = currStep + (m_genChanceOfValleys / sumRanges);
            if (randomPick >= currStep && randomPick < currStep)
            {
                return GaiaConstants.FeatureType.Valleys;
            }

            currStep = nextStep;
            nextStep = currStep + (m_genChanceOfVillages / sumRanges);
            if (randomPick >= currStep && randomPick < currStep)
            {
                return GaiaConstants.FeatureType.Villages;
            }

            currStep = nextStep;
            nextStep = currStep + (m_genChanceOfWaterfalls / sumRanges);
            if (randomPick >= currStep && randomPick < currStep)
            {
                return GaiaConstants.FeatureType.Waterfalls;
            }

            //We should never get to here - and we did its because they fed us BS
            return (GaiaConstants.FeatureType)UnityEngine.Random.Range(2, 7);
        }

        /// <summary>
        /// Grab a random stamp path of given feature type
        /// </summary>
        /// <param name="featureType"></param>
        /// <returns></returns>
        public string GetRandomStampPath(GaiaConstants.FeatureType featureType)
        {
            switch (featureType)
            {
                case GaiaConstants.FeatureType.Adhoc:
                    return "";

                case GaiaConstants.FeatureType.Bases:
                    return "";

                case GaiaConstants.FeatureType.Hills:
                    if (m_genHillStamps.Count == 0)
                    {
                        m_genHillStamps = Gaia.Utils.GetGaiaStampsList(GaiaConstants.FeatureType.Hills);
                    }
                    if (m_genHillStamps.Count == 0)
                    {
                        return "";
                    }
                    return m_genHillStamps[UnityEngine.Random.Range(0, m_genHillStamps.Count - 1)];

                case GaiaConstants.FeatureType.Islands:
                    if (m_genIslandStamps.Count == 0)
                    {
                        m_genIslandStamps = Gaia.Utils.GetGaiaStampsList(GaiaConstants.FeatureType.Islands);
                    }
                    if (m_genIslandStamps.Count == 0)
                    {
                        return "";
                    }
                    return m_genIslandStamps[UnityEngine.Random.Range(0, m_genIslandStamps.Count - 1)];

                case GaiaConstants.FeatureType.Lakes:
                    if (m_genLakeStamps.Count == 0)
                    {
                        m_genLakeStamps = Gaia.Utils.GetGaiaStampsList(GaiaConstants.FeatureType.Lakes);
                    }
                    if (m_genLakeStamps.Count == 0)
                    {
                        return "";
                    }
                    return m_genLakeStamps[UnityEngine.Random.Range(0, m_genLakeStamps.Count - 1)];

                case GaiaConstants.FeatureType.Mesas:
                    if (m_genMesaStamps.Count == 0)
                    {
                        m_genMesaStamps = Gaia.Utils.GetGaiaStampsList(GaiaConstants.FeatureType.Mesas);
                    }
                    if (m_genMesaStamps.Count == 0)
                    {
                        return "";
                    }
                    return m_genMesaStamps[UnityEngine.Random.Range(0, m_genMesaStamps.Count - 1)];

                case GaiaConstants.FeatureType.Mountains:
                    if (m_genMountainStamps.Count == 0)
                    {
                        m_genMountainStamps = Gaia.Utils.GetGaiaStampsList(GaiaConstants.FeatureType.Mountains);
                    }
                    if (m_genMountainStamps.Count == 0)
                    {
                        return "";
                    }
                    return m_genMountainStamps[UnityEngine.Random.Range(0, m_genMountainStamps.Count - 1)];

                case GaiaConstants.FeatureType.Plains:
                    if (m_genPlainsStamps.Count == 0)
                    {
                        m_genPlainsStamps = Gaia.Utils.GetGaiaStampsList(GaiaConstants.FeatureType.Plains);
                    }
                    if (m_genPlainsStamps.Count == 0)
                    {
                        return "";
                    }
                    return m_genPlainsStamps[UnityEngine.Random.Range(0, m_genPlainsStamps.Count - 1)];

                case GaiaConstants.FeatureType.Rivers:
                    if (m_genRiverStamps.Count == 0)
                    {
                        m_genRiverStamps = Gaia.Utils.GetGaiaStampsList(GaiaConstants.FeatureType.Rivers);
                    }
                    if (m_genRiverStamps.Count == 0)
                    {
                        return "";
                    }
                    return m_genRiverStamps[UnityEngine.Random.Range(0, m_genRiverStamps.Count - 1)];

                case GaiaConstants.FeatureType.Rocks:
                    return "";

                case GaiaConstants.FeatureType.Valleys:
                    if (m_genValleyStamps.Count == 0)
                    {
                        m_genValleyStamps = Gaia.Utils.GetGaiaStampsList(GaiaConstants.FeatureType.Valleys);
                    }
                    if (m_genValleyStamps.Count == 0)
                    {
                        return "";
                    }
                    return m_genValleyStamps[UnityEngine.Random.Range(0, m_genValleyStamps.Count - 1)];

                case GaiaConstants.FeatureType.Villages:
                    if (m_genVillageStamps.Count == 0)
                    {
                        m_genVillageStamps = Gaia.Utils.GetGaiaStampsList(GaiaConstants.FeatureType.Villages);
                    }
                    if (m_genVillageStamps.Count == 0)
                    {
                        return "";
                    }
                    return m_genVillageStamps[UnityEngine.Random.Range(0, m_genVillageStamps.Count - 1)];

                case GaiaConstants.FeatureType.Waterfalls:
                    if (m_genWaterfallStamps.Count == 0)
                    {
                        m_genWaterfallStamps = Gaia.Utils.GetGaiaStampsList(GaiaConstants.FeatureType.Waterfalls);
                    }
                    if (m_genWaterfallStamps.Count == 0)
                    {
                        return "";
                    }
                    return m_genWaterfallStamps[UnityEngine.Random.Range(0, m_genWaterfallStamps.Count - 1)];

                default:
                    return "";
            }
        }

        public string GetRandomMountainFieldPath()
        {
            if (m_genMountainStamps.Count == 0)
            {
                m_genMountainStamps = Gaia.Utils.GetGaiaStampsList(GaiaConstants.FeatureType.Mountains);
            }
            if (m_genMountainStamps.Count == 0)
            {
                return "";
            }

            string stampPath;
            var idx = 0;
            var fields = 0;

            //Count fields
            for (idx = 0; idx < m_genMountainStamps.Count; idx++)
            {
                stampPath = m_genMountainStamps[idx];
                if (stampPath.Contains("Field"))
                {
                    fields++;
                }
            }

            //Now choose one
            var hits = 0;
            var luckyNumber = UnityEngine.Random.Range(0, fields-1);
            for (idx = 0; idx < m_genMountainStamps.Count; idx++)
            {
                stampPath = m_genMountainStamps[idx];
                if (stampPath.Contains("Field"))
                {
                    if (hits == luckyNumber)
                    {
                        return stampPath;
                    }
                    hits++;
                }
            }
            return "";
        }

        /// <summary>
        /// Find or create the relevant object and apply this operation to it
        /// </summary>
        /// <param name="operationIdx"></param>
        public GameObject Apply(int operationIdx)
        {
            if (operationIdx < 0 || operationIdx >= m_session.m_operations.Count)
            {
                Debug.LogWarning(string.Format("Can not Apply operation because the index {0} is out of bounds.", operationIdx));
                return null;
            }

            var operation = m_session.m_operations[operationIdx];

            //Get or create the game object that generated this operations
            var go = FindOrCreateObject(operation);

            //Exit out if we couldnt find what generated the operation
            if (go == null)
            {
                return go;
            }

            //Deserialise stamp if necessary
            var stamper = go.GetComponent<Stamper>();
            if (stamper != null && operation.m_operationType == GaiaOperation.OperationType.Stamp)
            {
                stamper.DeSerialiseJson(operation.m_operationDataJson[0]);

                //Grab the embedded resources that were exported and associate them
                stamper.m_resources = Gaia.Utils.GetAsset(
                    ScriptableObjectWrapper.GetSessionedFileName(m_session.GetSessionFileName(), stamper.m_resourcesPath), typeof(Gaia.GaiaResource))
                    as Gaia.GaiaResource;

                if (stamper.m_resources == null)
                {
                    ExportSessionResource(stamper.m_resourcesPath);
                    stamper.m_resources = Gaia.Utils.GetAsset(
                        ScriptableObjectWrapper.GetSessionedFileName(m_session.GetSessionFileName(), stamper.m_resourcesPath), typeof(Gaia.GaiaResource))
                        as Gaia.GaiaResource;
                }

                stamper.m_seaLevel = m_session.m_seaLevel;
            }

            //Deserialise spawner if necessary
            var spawner = go.GetComponent<Spawner>();
            if (spawner != null && operation.m_operationType == GaiaOperation.OperationType.Spawn)
            {
                spawner.DeSerialiseJson(operation.m_operationDataJson[0]);

                //Grab the embedded resources that were exported and associate them
                spawner.m_resources = Gaia.Utils.GetAsset(
                    ScriptableObjectWrapper.GetSessionedFileName(m_session.GetSessionFileName(), spawner.m_resourcesPath), typeof(Gaia.GaiaResource)) 
                    as Gaia.GaiaResource;

                if (spawner.m_resources == null)
                {
                    ExportSessionResource(spawner.m_resourcesPath);
                    spawner.m_resources = Gaia.Utils.GetAsset(
                        ScriptableObjectWrapper.GetSessionedFileName(m_session.GetSessionFileName(), spawner.m_resourcesPath), typeof(Gaia.GaiaResource)) 
                        as Gaia.GaiaResource;
                }

                if (spawner.m_resources == null)
                {
                    Debug.LogError("Unable to get resources file for " + spawner.name);
                }
                else
                {
                    //Apply any missing assets to the terrain
                    spawner.AssociateAssets();
                    var missingResources = spawner.GetMissingResources();
                    if (missingResources.GetLength(0) > 0)
                    {
                        spawner.AddResourcesToTerrain(missingResources);
                    }

                    //Make sure spawner using same sea level as the session
                    spawner.m_resources.ChangeSeaLevel(m_session.m_seaLevel);
                }
            }

            //Return the game object
            return go;
        }

        /// <summary>
        /// Play an entire session back. Kicks off the playback as a co-routine.
        /// </summary>
        public void PlaySession()
        {
            m_cancelPlayback = false;

            //Force an export of the assets it uses - we want everything to run off these instead of the original in case there is a conflict
            ExportSessionResources();

            //The process accordingly
            #if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                m_updateSessionCoroutine = PlaySessionCoRoutine();
                StartEditorUpdates();
            }
            else
            {
                StartCoroutine(PlaySessionCoRoutine());
            }
            #else
                StartCoroutine(PlaySessionCoRoutine());
            #endif
        }


        /// <summary>
        /// Playback a session as a co-routine
        /// </summary>
        public IEnumerator PlaySessionCoRoutine()
        {
            //Debug.Log("Playing session " + m_session.m_name);

            m_progress = 0;

            if (Application.isPlaying)
            {
                for (var idx = 0; idx < m_session.m_operations.Count; idx++)
                {
                    if (!m_cancelPlayback)
                    {
                        if (m_session.m_operations[idx].m_isActive)
                        {
                            yield return StartCoroutine(PlayOperationCoRoutine(idx));
                        }
                    }
                }
            }
            else
            {
                for (var idx = 0; idx < m_session.m_operations.Count; idx++)
                {
                    if (!m_cancelPlayback)
                    {
                        if (m_session.m_operations[idx].m_isActive)
                        {
                            m_updateOperationCoroutine = PlayOperationCoRoutine(idx);
                            yield return new WaitForSeconds(0.2f);
                        }
                    }
                }
            }

            #if UNITY_EDITOR
            SceneView.RepaintAll();
            #endif

            Debug.Log("Finished playing session " + m_session.m_name);

            m_updateSessionCoroutine = null;
        }


        /// <summary>
        /// Playback an operation - kicks off the coroutine
        /// </summary>
        /// <param name="opIdx"></param>
        public void PlayOperation(int opIdx)
        {
            m_cancelPlayback = false;

            #if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                m_updateOperationCoroutine = PlayOperationCoRoutine(opIdx);
                StartEditorUpdates();
            }
            else
            {
                StartCoroutine(PlayOperationCoRoutine(opIdx));
            }
            #else
            StartCoroutine(PlayOperationCoRoutine(opIdx));
            #endif
        }

        /// <summary>
        /// Plays back an operation as a co-routine
        /// </summary>
        /// <param name="operationIdx"></param>
        /// <returns></returns>
        public IEnumerator PlayOperationCoRoutine(int operationIdx)
        {
            //Check operation index
            if (operationIdx < 0 || operationIdx >= m_session.m_operations.Count)
            {
                Debug.LogWarning(string.Format("Operation index {0} is out of bounds.", operationIdx));
                m_updateOperationCoroutine = null;
                yield break;
            }

            //Check if active
            if (!m_session.m_operations[operationIdx].m_isActive)
            {
                Debug.LogWarning(string.Format("Operation '{0}' is not active. Ignoring.", m_session.m_operations[operationIdx].m_description));
                m_updateOperationCoroutine = null;
                yield break;
            }


            //Stop this operation from adding more to the session
            var lockState = m_session.m_isLocked;
            m_session.m_isLocked = true;

            //Grab operation and let world know about it
            var operation = m_session.m_operations[operationIdx];
            //Debug.Log("Playing: " + operation.m_description);

            //Get or create the operation game object, and apply the operation to it
            var go = Apply(operationIdx);

            //Now invoke the necessary code to play it
            Stamper stamper = null;
            Spawner spawner = null;
            if (go != null)
            {
                stamper = go.GetComponent<Stamper>();
                spawner = go.GetComponent<Spawner>();
            }

            switch (operation.m_operationType)
            {
                case GaiaOperation.OperationType.CreateTerrain:
                    if (Gaia.TerrainHelper.GetActiveTerrainCount() == 0)
                    {
                        if (m_session.m_defaults != null && m_session.m_defaults.m_content.GetLength(0) > 0)
                        {
                            #if UNITY_EDITOR
                            var defaults = Gaia.Utils.GetAsset(
                                Gaia.ScriptableObjectWrapper.GetSessionedFileName(m_session.m_name, m_session.m_defaults.m_fileName), typeof(Gaia.GaiaDefaults)) 
                                as Gaia.GaiaDefaults;
                            if (defaults == null)
                            {
                                ExportSessionDefaults();
                                defaults = Gaia.Utils.GetAsset(
                                    Gaia.ScriptableObjectWrapper.GetSessionedFileName(m_session.m_name, m_session.m_defaults.m_fileName), typeof(Gaia.GaiaDefaults)) 
                                    as Gaia.GaiaDefaults;
                            }
                            if (defaults == null)
                            {
                                Debug.LogWarning("Could not create terrain - unable to locate exported defaults");
                            }
                            else
                            {
                                //Now try and locate the resources and pass the into the terrain creation
                                if (operation.m_operationDataJson != null && operation.m_operationDataJson.GetLength(0) == 2)
                                {
                                    var resources = Gaia.Utils.GetAsset(
                                        Gaia.ScriptableObjectWrapper.GetSessionedFileName(m_session.m_name, operation.m_operationDataJson[1]), typeof(Gaia.GaiaResource)) 
                                        as Gaia.GaiaResource;
                                    if (resources == null)
                                    {
                                        ExportSessionResource(operation.m_operationDataJson[1]);
                                        resources = Gaia.Utils.GetAsset(
                                            m_session.GetSessionFileName() + "_" + Path.GetFileName(operation.m_operationDataJson[1]), typeof(Gaia.GaiaResource)) 
                                            as Gaia.GaiaResource;
                                    }
                                    if (resources != null)
                                    {
                                        defaults.CreateTerrain(resources);
                                    }
                                    else
                                    {
                                        defaults.CreateTerrain();
                                    }
                                }
                                else
                                {
                                    defaults.CreateTerrain();
                                }
                            }
                            #endif
                        }
                    }
                    break;
                case GaiaOperation.OperationType.FlattenTerrain:
                    if (stamper != null)
                    {
                        stamper.FlattenTerrain();
                    }
                    break;
                case GaiaOperation.OperationType.SmoothTerrain:
                    if (stamper != null)
                    {
                        stamper.SmoothTerrain();
                    }
                    break;
                case GaiaOperation.OperationType.ClearDetails:
                    if (stamper != null)
                    {
                        stamper.ClearDetails();
                    }
                    break;
                case GaiaOperation.OperationType.ClearTrees:
                    if (stamper != null)
                    {
                        stamper.ClearTrees();
                    }
                    break;
                case GaiaOperation.OperationType.Stamp:
                    if (stamper != null)
                    {
                        m_currentStamper = stamper;
                        m_currentSpawner = null;
                        if (!Application.isPlaying)
                        {
                            stamper.HidePreview();
                            stamper.Stamp();
                            while (stamper.IsStamping())
                            {
                                if ((DateTime.Now - m_lastUpdateDateTime).Milliseconds > 250)
                                {
                                    m_lastUpdateDateTime = DateTime.Now; //Forces an editor refresh
                                    m_progress++;
                                }
                                yield return new WaitForSeconds(0.2f);
                            }
                        }
                        else
                        {
                            yield return StartCoroutine(stamper.ApplyStamp());
                        }
                    }
                    break;
                case GaiaOperation.OperationType.StampUndo:
                    if (stamper != null)
                    {
                        stamper.Undo();
                    }
                    break;
                case GaiaOperation.OperationType.StampRedo:
                    if (stamper != null)
                    {
                        stamper.Redo();
                    }
                    break;
                case GaiaOperation.OperationType.Spawn:
                    if (spawner!= null)
                    {
                        m_currentStamper = null;
                        m_currentSpawner = spawner;

                        if (!Application.isPlaying)
                        {
                            spawner.RunSpawnerIteration();
                            while (spawner.IsSpawning())
                            {
                                if ((DateTime.Now - m_lastUpdateDateTime).Milliseconds > 250)
                                {
                                    m_lastUpdateDateTime = DateTime.Now; //Forces an editor refresh
                                    m_progress++;
                                }
                                yield return new WaitForSeconds(0.2f);
                            }
                        }
                        else
                        {
                            //yield return StartCoroutine(spawner.R);
                        }
                    }
                    break;
                case GaiaOperation.OperationType.SpawnReset:
                    break;
                default:
                    break;
            }

            //Return session lock state to what it was before
            m_session.m_isLocked = lockState;

            //Signal an end
            m_updateOperationCoroutine = null;
        }

        /// <summary>
        /// Cancel the playback
        /// </summary>
        public void CancelPlayback()
        {
            m_cancelPlayback = true;
            if (m_currentStamper != null)
            {
                m_currentStamper.CancelStamp();
            }
            if (m_currentSpawner != null)
            {
                m_currentSpawner.CancelSpawn();
            }
        }

        /// <summary>
        /// Export all current session defaults and resources - particularly relevant when importing from someone elses system
        /// </summary>
        public void ExportSessionResources()
        {
            //Make sure we have a session export directory
            var path = "Assets/GaiaSessions/";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            //Then create one for this session
            path = Path.Combine(path, Gaia.Utils.FixFileName(m_session.m_name));
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            //Export the defaults if they exist
            if (m_session.m_defaults != null && m_session.m_defaults.m_content.GetLength(0) > 0)
            {
                var exportName = Path.Combine(path, Gaia.ScriptableObjectWrapper.GetSessionedFileName(m_session.m_name, m_session.m_defaults.m_fileName));
                Gaia.Utils.WriteAllBytes(exportName, m_session.m_defaults.m_content);
            }

            //Export all the resources
            foreach (var kvp in m_session.m_resources)
            {
                var exportName = Path.Combine(path, Gaia.ScriptableObjectWrapper.GetSessionedFileName(m_session.m_name, kvp.Value.m_fileName));
                Gaia.Utils.WriteAllBytes(exportName, kvp.Value.m_content);
            }

            #if UNITY_EDITOR
            AssetDatabase.Refresh();
            #endif
        }


        /// <summary>
        /// Export the current session defaults - particularly relevant when importing from someone elses system
        /// </summary>
        public void ExportSessionDefaults()
        {
            //Make sure we have a session export directory
            var path = "Assets/GaiaSessions/";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            //Then create one for this session
            path = Path.Combine(path, Gaia.Utils.FixFileName(m_session.m_name));
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            //Export the defaults if they exist
            if (m_session.m_defaults != null && m_session.m_defaults.m_content.GetLength(0) > 0)
            {
                var exportName = Path.Combine(path, Gaia.ScriptableObjectWrapper.GetSessionedFileName(m_session.m_name, m_session.m_defaults.m_fileName));
                Gaia.Utils.WriteAllBytes(exportName, m_session.m_defaults.m_content);
            }

            #if UNITY_EDITOR
            AssetDatabase.Refresh();
            #endif
        }

        /// <summary>
        /// Export the specific resource specified
        /// </summary>
        public void ExportSessionResource(string resourcePath)
        {
            //Make sure we have a session export directory
            var path = "Assets/GaiaSessions/";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            //Then create one for this session
            path = Path.Combine(path, Gaia.Utils.FixFileName(m_session.m_name));
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            //Export the matching resource
            foreach (var kvp in m_session.m_resources)
            {
                if (Path.GetFileName(resourcePath).ToLower() == Path.GetFileName(kvp.Value.m_fileName).ToLower())
                {
                    var exportName = Path.Combine(path, Gaia.ScriptableObjectWrapper.GetSessionedFileName(m_session.m_name, kvp.Value.m_fileName));
                    Gaia.Utils.WriteAllBytes(exportName, kvp.Value.m_content);
                }
            }

            #if UNITY_EDITOR
            AssetDatabase.Refresh();
            #endif
        }

        /// <summary>
        /// Draw gizmos
        /// </summary>
        void OnDrawGizmosSelected()
        {
            if (m_session != null)
            {
                var bounds = new Bounds();
                if (TerrainHelper.GetTerrainBounds(transform.position, ref bounds) == true)
                {
                    //Terrain dimensions
                    Gizmos.color = Color.white;
                    Gizmos.DrawWireCube(bounds.center, bounds.size);

                    //Water dimensions
                    bounds.center = new Vector3(bounds.center.x, m_session.m_seaLevel, bounds.center.z);
                    bounds.size = new Vector3(bounds.size.x, 0.05f, bounds.size.z);
                    Gizmos.color = new Color(Color.blue.r, Color.blue.g, Color.blue.b, Color.blue.a / 4f);
                    Gizmos.DrawCube(bounds.center, bounds.size);
                }
            }
        }


        #region Object location and creation scripts

        /// <summary>
        /// Find or create the object that created this operation
        /// </summary>
        /// <param name="operation">The operation</param>
        /// <returns>The object if possible or null</returns>
        GameObject FindOrCreateObject(GaiaOperation operation)
        {
            if (operation.m_generatedByType == "Gaia.Stamper")
            {
                //See if we can locate it in the existing stamps
                var stampers = GameObject.FindObjectsOfType<Stamper>();
                for (var stampIdx = 0; stampIdx < stampers.GetLength(0); stampIdx++)
                {
                    if (stampers[stampIdx].m_stampID == operation.m_generatedByID && stampers[stampIdx].name == operation.m_generatedByName)
                    {
                        return stampers[stampIdx].gameObject;
                    }
                }
                //If we couldnt find this - then add it
                return ShowStamper(operation.m_generatedByName, operation.m_generatedByID);
            }
            else if (operation.m_generatedByType == "Gaia.Spawner")
            {
                //See if we can locate it in the existing stamps
                var spawners = GameObject.FindObjectsOfType<Spawner>();
                for (var spawnerIdx = 0; spawnerIdx < spawners.GetLength(0); spawnerIdx++)
                {
                    if (spawners[spawnerIdx].m_spawnerID == operation.m_generatedByID && spawners[spawnerIdx].name == operation.m_generatedByName)
                    {
                        return spawners[spawnerIdx].gameObject;
                    }
                }
                //If we couldnt find this - the add it
                return CreateSpawner(operation.m_generatedByName, operation.m_generatedByID);
            }
            return null;
        }

        /// <summary>
        /// Select or create a stamper
        /// </summary>
        GameObject ShowStamper(string name, string id)
        {
            var gaiaObj = GameObject.Find("Gaia");
            if (gaiaObj == null)
            {
                gaiaObj = new GameObject("Gaia");
            }
            var stamperObj = GameObject.Find(name);
            if (stamperObj == null)
            {
                stamperObj = new GameObject(name);
                stamperObj.transform.parent = gaiaObj.transform;
                var stamper = stamperObj.AddComponent<Stamper>();
                stamper.m_stampID = id;
                stamper.HidePreview();
                stamper.m_seaLevel = m_session.m_seaLevel;
            }
            return stamperObj;
        }

        /// <summary>
        /// Create and show spawner
        /// </summary>
        GameObject CreateSpawner(string name, string id)
        {
            var gaiaObj = GameObject.Find("Gaia");
            if (gaiaObj == null)
            {
                gaiaObj = new GameObject("Gaia");
            }
            var spawnerObj = new GameObject(name);
            spawnerObj.transform.parent = gaiaObj.transform;
            var spawner = spawnerObj.AddComponent<Spawner>();
            spawner.m_spawnerID = id;
            return spawnerObj;
        }

        #endregion

    }
}
