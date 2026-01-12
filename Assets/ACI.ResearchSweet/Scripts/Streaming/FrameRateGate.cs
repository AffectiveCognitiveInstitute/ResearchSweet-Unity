using UnityEngine;

namespace ResearchSweet
{
    public class FrameRateGate
    {
        private float interval = 1.0f;
        private float lastTime = -Mathf.Infinity;

        public FrameRateGate(int fps)
        {
            SetRate(fps);
        }

        public void SetRate(int fps)
        {
            interval = 1.0f / Mathf.Max(1, fps);
        }

        public bool ShouldProcess()
        {
            if (Time.unscaledTime - lastTime >= interval)
            {
                lastTime = Time.unscaledTime;
                return true;
            }

            return false;
        }
    }
}
