﻿using ColossalFramework;
using DaylightClassic.Options;
using UnityEngine;
using UnityEngine.Networking.Match;

namespace DaylightClassic
{
    public static class DaylightClassic
    {
        public const string GameObjectName = "DaylightClassicMonitor";

        private const string Europe = "LUTeurope";
        private const string Sunny = "LUTSunny";
        private const string North = "LUTNorth";
        private const string Tropical = "LUTTropical";

        private static Texture3DWrapper _europeanClassic;
        private static Texture3DWrapper _sunnyClassic;
        private static Texture3DWrapper _northClassic;
        private static Texture3DWrapper _tropicalClassic;

        private static Texture3DWrapper _europeanAd;
        private static Texture3DWrapper _sunnyAd;
        private static Texture3DWrapper _northAd;
        private static Texture3DWrapper _tropicalAd;

        private const float IntensityClassic = 3.318695f;
        private const float ExposureClassic = 1.0f;
        private static float _intensityAd = -1.0f;
        private static float _exposureAd = -1.0f;
        private static bool _ingame;
        private static float _lonAd = -1.0f;
        private static float _latAd = -1.0f;

        private static Color _skyTintAd = Color.clear;
        private static Color _groundColorAd = Color.clear;
        private static Vector3 _waveLengthsAd;

        private static readonly Gradient ColorClassic = new Gradient()
        {
            colorKeys = new GradientColorKey[8]
                {
                    new GradientColorKey((Color) new Color32((byte) 55, (byte) 66, (byte) 77, byte.MaxValue), 0.23f),
                    new GradientColorKey((Color) new Color32((byte) 245, (byte) 173, (byte) 84, byte.MaxValue), 0.26f),
                    new GradientColorKey((Color) new Color32((byte) 252, (byte) 222, (byte) 186, byte.MaxValue), 0.29f),
                    new GradientColorKey((Color) new Color32((byte) 255, (byte) 255, (byte) 255, byte.MaxValue), 0.35f),
                    new GradientColorKey((Color) new Color32((byte) 255, (byte) 255, (byte) 255, byte.MaxValue), 0.65f),
                    new GradientColorKey((Color) new Color32((byte) 252, (byte) 222, (byte) 186, byte.MaxValue), 0.71f),
                    new GradientColorKey((Color) new Color32((byte) 245, (byte) 173, (byte) 84, byte.MaxValue), 0.74f),
                    new GradientColorKey((Color) new Color32((byte) 55, (byte) 66, (byte) 77, byte.MaxValue), 0.77f)
                },
            alphaKeys = new GradientAlphaKey[2]
                {
                    new GradientAlphaKey(1f, 0.0f),
                    new GradientAlphaKey(1f, 1f)
                }
        };

        private static Gradient _colorAd;

        public static void Initialize()
        {
            Reset();

            var go = new GameObject("DaylightClassicProperties");
            go.AddComponent<DaylightClassicProperties>();
            _ingame = true;
        }

        public static void Reset()
        {
            var go = GameObject.FindObjectOfType<DaylightClassicProperties>();
            if (go != null)
            {
                GameObject.Destroy(go);
            }
            _ingame = false;
            _europeanClassic = null;
            _tropicalClassic = null;
            _northClassic = null;
            _sunnyClassic = null;
            _europeanAd = null;
            _tropicalAd = null;
            _northAd = null;
            _sunnyAd = null;
            _intensityAd = -1.0f;
            _lonAd = -1.0f;
            _latAd = -1.0f;
            _skyTintAd = Color.clear;
            _waveLengthsAd = Vector3.zero;
            //TODO(earalov): destroy textures?
        }

        public static void ReplaceLuts(bool toClassic)
        {
            if (!_ingame)
            {
                return;
            }
            var renderProperties = Object.FindObjectOfType<RenderProperties>();
            for (var i = 0; i < ColorCorrectionManager.instance.m_BuiltinLUTs.Length; i++)
            {
                var builtinLutName = ColorCorrectionManager.instance.m_BuiltinLUTs[i].name;
                var builtinLut = ColorCorrectionManager.instance.m_BuiltinLUTs[i];
                Texture3DWrapper replacement;
                switch (builtinLutName)
                {
                    case Europe:
                        if (_europeanAd == null)
                        {
                            _europeanAd = builtinLut;
                        }
                        if (_europeanClassic == null)
                        {
                            _europeanClassic = Util.LoadTexture("DaylightClassic.lut.EuropeanClassic.png", Europe);
                        }
                        replacement = toClassic ? _europeanClassic : _europeanAd;
                        break;
                    case Tropical:
                        if (_tropicalAd == null)
                        {
                            _tropicalAd = builtinLut;
                        }
                        if (_tropicalClassic == null)
                        {
                            _tropicalClassic = Util.LoadTexture("DaylightClassic.lut.TropicalClassic.png", Tropical);
                        }
                        replacement = toClassic ? _tropicalClassic : _tropicalAd;
                        break;
                    case North:
                        if (_northAd == null)
                        {
                            _northAd = builtinLut;
                        }
                        if (_northClassic == null)
                        {
                            _northClassic = Util.LoadTexture("DaylightClassic.lut.BorealClassic.png", North);
                        }
                        replacement = toClassic ? _northClassic : _northAd;
                        break;
                    case Sunny:
                        if (_sunnyAd == null)
                        {
                            _sunnyAd = builtinLut;
                        }
                        if (_sunnyClassic == null)
                        {
                            _sunnyClassic = Util.LoadTexture("DaylightClassic.lut.TemperateClassic.png", Sunny);
                        }
                        replacement = toClassic ? _sunnyClassic : _sunnyAd;
                        break;
                    default:
                        continue;
                }
                if (builtinLutName == renderProperties.m_ColorCorrectionLUT.name)
                {
                    renderProperties.m_ColorCorrectionLUT = replacement;
                }
                ColorCorrectionManager.instance.m_BuiltinLUTs[i] = replacement;
            }

            var size = ColorCorrectionManager.instance.items.Length;
            var lastSelection = ColorCorrectionManager.instance.lastSelection;
            ColorCorrectionManager.instance.currentSelection = (lastSelection + 1) % size;
            ColorCorrectionManager.instance.currentSelection = lastSelection;
        }

        public static void ReplaceSunlightIntensity(bool toClassic)
        {
            if (!_ingame)
            {
                return;
            }
            var prop = Object.FindObjectOfType<DayNightProperties>();


            if (_intensityAd < 0)
            {
                _intensityAd = prop.m_SunIntensity;
            }
            if (_exposureAd < 0)
            {
                _exposureAd = prop.m_Exposure;
            }
            prop.m_SunIntensity = toClassic ? IntensityClassic : _intensityAd;
            prop.m_Exposure = toClassic ? ExposureClassic : _exposureAd;
        }

        public static void ReplaceSunlightColor(bool toClassic)
        {
            if (!_ingame)
            {
                return;
            }
            var prop = Object.FindObjectOfType<DayNightProperties>();
            if (_colorAd == null)
            {
                _colorAd = prop.m_LightColor;
            }
            prop.m_LightColor = toClassic ? ColorClassic : _colorAd;
        }

        public static void ReplaceFogEffect(bool toClassic)
        {
            if (!_ingame)
            {
                return;
            }
            var gameObject = GameObject.Find(GameObjectName);
            if (toClassic)
            {
                if (gameObject == null)
                {
                    gameObject = new GameObject(GameObjectName);
                    gameObject.AddComponent<DayNightCycleMonitor>();
                }
            }
            else
            {
//                if (gameObject != null)
//                {
//                    Object.Destroy(gameObject);
//                }
            }

        }

        public static void ReplaceLatLong(bool toClassic)
        {
            if (!_ingame)
            {
                return;
            }
            var prop = Object.FindObjectOfType<DayNightProperties>();
            if (_lonAd < 0.0f)
            {
                _lonAd = prop.m_Longitude;
            }
            if (_latAd < 0.0f)
            {
                _latAd = prop.m_Latitude;
            }
            var env = Util.GetEnv();
            if (toClassic)
            {
                if (env == "Europe") //London
                {
                    prop.m_Latitude = 51.5072f;
                    prop.m_Longitude = -0.1275f;
                }
                else if (env == "North")
                {
                    //TODO(earalov): rotate skybox
                }
                else if (env == "Sunny") //Malta
                {
                    prop.m_Latitude = 35.8833f;
                    prop.m_Longitude = 14.5000f;
                }
                else if (env == "Tropical") //Mecca
                {
                    prop.m_Latitude = 21.4167f;
                    prop.m_Longitude = 39.8167f;
                }
            }
            else
            {
                prop.m_Latitude = _latAd;
                prop.m_Longitude = _lonAd;
            }
        }

        public static void ReplaceFogColor(bool toClassic)
        {
            if (!_ingame /*|| OptionsHolder.Options.fogEffect*/)
            {
                return;
            }
            ReplaceFogColorImpl(toClassic);
        }

        internal static void ReplaceFogColorImpl(bool toClassic)
        {
            var prop = Object.FindObjectOfType<DayNightProperties>();
            if (_skyTintAd == Color.clear)
            {
                _skyTintAd = prop.m_SkyTint;
            }
            if (_groundColorAd == Color.clear)
            {
                _groundColorAd = prop.m_GroundColor;
            }
            if (_waveLengthsAd == Vector3.zero)
            {
                _waveLengthsAd = prop.m_WaveLengths;
            }
            var properties = GameObject.FindObjectOfType<DaylightClassicProperties>();
            prop.m_SkyTint = toClassic ? properties.SkyTintClassic : _skyTintAd;
            prop.m_WaveLengths = toClassic ? properties.WaveLengthsClassic : _waveLengthsAd;
            prop.m_GroundColor = toClassic ? properties.GroundColorClassic : _groundColorAd;
        }
    }
}