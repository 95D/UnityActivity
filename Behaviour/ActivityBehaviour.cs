using UnityEngine;

namespace Viento.UnityActivity
{
    public abstract class ActivityBehaviour : MonoBehaviour, IBackButtonEventListener
    {
        private ActivityManager activityManager;

        private ActivityLifecycle _lifecycle = ActivityLifecycle.FIRST;
        public ActivityLifecycle Lifecycle => _lifecycle;

        public float Transition(ActivityLifecycle next)
        {
            var delayTime = OnTransition(_lifecycle, next);
            _lifecycle = next;
            return delayTime;
        }

        protected abstract float OnTransition(ActivityLifecycle current, ActivityLifecycle next);

        public virtual ActivityResultBundle GetResultBundle() => ActivityResultBundle.EMPTY_RESULT;

        public abstract void OnResultFromTop(ActivityResultBundle result);

        protected void Finish()
        {
            activityManager.PopActivity(this);
        }

        private void Awake()
        {
            activityManager = transform.parent.GetComponent<ActivityManager>();
        }
        private void OnDestroy()
        {
            activityManager = null;
        }

        public void OnClickBackButton()
        {
            if (activityManager != null)
            {
                activityManager.PopActivity(this);
            }
        }
    }
}