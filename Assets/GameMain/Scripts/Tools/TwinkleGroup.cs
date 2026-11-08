using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

namespace EndlessAbyss
{
    public class TwinkleGroup : MonoBehaviour
    {
        public float twinkleTime = 0.7f;
        public Vector2 intervalRange = new Vector2(0.5f, 1f);
        public Vector2 frequencyRange = new Vector2(0.1f, 0.5f);
        
        private float[] _nextTwinkleTimes;

        private void Awake()
        {
            _nextTwinkleTimes = new float[transform.childCount];
            for (var i = 0; i < _nextTwinkleTimes.Length; i++)
            {
                _nextTwinkleTimes[i] = Time.time + RandomFrequencyTime();
                var tf = transform.GetChild(i);
                tf.localScale = Vector3.zero;
            }
        }

        private void Update()
        {
            for (var i = 0; i < _nextTwinkleTimes.Length; i++)
            {
                if (Time.time >= _nextTwinkleTimes[i])
                {
                    PlayTwinkleAnimation(i);
                    _nextTwinkleTimes[i] = Time.time + twinkleTime + RandomFrequencyTime() + RandomIntervalTime();
                }
            }
        }

        private void PlayTwinkleAnimation(int childIndex)
        {
            var tf = transform.GetChild(childIndex);
            tf.DOScale(Vector3.one, twinkleTime * 0.5f).SetLoops(2, LoopType.Yoyo);
        }
        
        private float RandomFrequencyTime()
        {
            return Random.Range(frequencyRange.x, frequencyRange.y);
        }
        private float RandomIntervalTime()
        {
            return Random.Range(intervalRange.x, intervalRange.y);
        }
    }
}
