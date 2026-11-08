using System.Collections.Generic;
using UnityEngine;

namespace EndlessAbyss
{
    public class ScreenShakeManager : MonoBehaviour
    {
        class ShakeInstance
        {
            public float duration;
            public float time;
            public float strength;
            public float frequency;
            public bool useDamper;
        }

        private readonly List<ShakeInstance> _shakes = new();
        private Vector3 _originalPos;
        private Quaternion _originalRot;

        void Awake()
        {
            _originalPos = transform.localPosition;
            _originalRot = transform.localRotation;
        }

        void LateUpdate()
        {
            Vector3 posOffset = Vector3.zero;
            Vector3 rotOffset = Vector3.zero;

            for (var i = _shakes.Count - 1; i >= 0; i--)
            {
                var s = _shakes[i];

                s.time += Time.deltaTime;

                if (s.time >= s.duration)
                {
                    _shakes.RemoveAt(i);
                    continue;
                }


                var damper = 1f;
                if (s.useDamper)
                {
                    damper -= s.time / s.duration;
                }

                var noiseX = (Mathf.PerlinNoise(Time.time * s.frequency, 0) - 0.5f) * 2f;
                var noiseY = (Mathf.PerlinNoise(0, Time.time * s.frequency) - 0.5f) * 2f;

                posOffset += new Vector3(noiseX, noiseY, 0) * s.strength * 0.1f * damper;
                rotOffset += new Vector3(noiseY, noiseX, noiseX) * s.strength * 0.1f * damper * 2f;
            }

            transform.localPosition = _originalPos + posOffset;
            transform.localRotation = _originalRot * Quaternion.Euler(rotOffset);
        }

        public void Shake(float duration, float strength, float frequency = 25f,bool useDamper = true)
        {
            _shakes.Add(new ShakeInstance
            {
                duration = duration,
                strength = strength,
                frequency = frequency,
                useDamper = useDamper,
                time = 0
            });
        }
    }
}