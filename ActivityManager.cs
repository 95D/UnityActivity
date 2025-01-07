using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Viento.UnityActivity
{
    public class ActivityManager : MonoBehaviour
    {
        private List<ActivityBehaviour> activityStack = new();

        private static string TAG = "ActivityManager";

        public ActivityBehaviour TopActivity
        {
            get
            {
                return activityStack.Count == 0 ? null : activityStack.Last();
            }
        }

        public void StartActivity(
            IActivityFactory activityFactory,
            ActivityStartType startType = ActivityStartType.INSERT)
        {
            Debug.Log(string.Format("{0}: Start new activity by factory {1}", TAG, activityFactory));
            var activity = activityFactory.Create(transform);
            StartActivity(activity, startType);
        }

        public void StartActivity(
            GameObject prefab,
            ActivityStartType startType = ActivityStartType.INSERT)
        {
            Debug.Log(string.Format("{0}: Start new activity by prefab {1}", TAG, prefab.name));
            var activity = Instantiate(prefab).GetComponent<ActivityBehaviour>();
            if (activity == null)
            {
                Debug.LogWarning(string.Format("{0}: {0} isn't activity prefab", TAG, prefab.name));
                return;
            }
            StartActivity(activity, startType);
        }

        private void StartActivity(ActivityBehaviour newActivity, ActivityStartType startType)
        {
            Debug.Log(string.Format("{0}: Start new activity, StartType: {1}", TAG, startType));
            var currentActivity = activityStack.Count == 0 ? null : activityStack.Last();
            var activitiesWillBeDestoyed = new List<ActivityBehaviour>();
            if (startType == ActivityStartType.REPLACE && currentActivity != null)
            {
                activityStack.Remove(currentActivity);
                activitiesWillBeDestoyed.Add(currentActivity);
            }
            else if (startType == ActivityStartType.NEW_TASK)
            {
                activitiesWillBeDestoyed.AddRange(activityStack);
                activityStack.Clear();
            }
            activityStack.Add(newActivity);
            StartCoroutine(
                StartActivityTransition(
                    newActivity: newActivity,
                    currentActivity: currentActivity,
                    activitiesWillBeDestroyed: activitiesWillBeDestoyed));

        }

        private IEnumerator StartActivityTransition(
            ActivityBehaviour newActivity,
            ActivityBehaviour currentActivity,
            List<ActivityBehaviour> activitiesWillBeDestroyed)
        {
            Debug.Log(string.Format("{0}: Start new activity transition", TAG));
            if (currentActivity != null)
            {
                var delayTimePause = currentActivity.Transition(ActivityLifecycle.PAUSED);
                Debug.Log(string.Format("{0}: Activity {1} is paused [{2} secs]", TAG, currentActivity.name, delayTimePause));
                yield return new WaitForSecondsRealtime(delayTimePause);
            }
            Debug.Log(string.Format("{0}: Clean up {1} activities", TAG, activitiesWillBeDestroyed.Count));
            foreach (var activity in activitiesWillBeDestroyed)
            {
                Debug.Log(string.Format("{0}: Clean up activity {1}", TAG, activity.name));
                Destroy(activity.gameObject);
            }
            Debug.Log(string.Format("{0}: Add activity {1} into stack", TAG, newActivity.name));
            var delayTimeCreate = newActivity.Transition(ActivityLifecycle.CREATED);
            Debug.Log(string.Format("{0}: Activity {1} is created [{2} secs]", TAG, newActivity.name, delayTimeCreate));
            yield return new WaitForSecondsRealtime(delayTimeCreate);
            var delayTimeStart = newActivity.Transition(ActivityLifecycle.STARTED);
            Debug.Log(string.Format("{0}: Activity {1} is started [{2} secs]", TAG, newActivity.name, delayTimeStart));
            yield return new WaitForSecondsRealtime(delayTimeStart);
            var delayTimeResume = newActivity.Transition(ActivityLifecycle.RESUMED);
            Debug.Log(string.Format("{0}: Activity {1} is resumed [{2} secs]", TAG, newActivity.name, delayTimeResume));
            yield return new WaitForSecondsRealtime(delayTimeResume);
        }

        public void PopActivity(ActivityBehaviour activity)
        {
            // Stack management
            Debug.Log(string.Format("{0}: Pop current activity", TAG));
            Debug.Log(string.Format("{0}: Stack management start", TAG));
            var popActivity = activityStack.Count == 0 ? null : activityStack.Last();
            if (popActivity == null)
            {
                Debug.LogError(string.Format("{0}: Activity stack is empty", TAG));
                return;
            }
            if (popActivity != activity)
            {
                Debug.LogError(
                    string.Format(
                        "{0}: Activity {1} isn't top activity. System only could pop top activity.",
                        TAG,
                        activity.name));
                return;
            }
            activityStack.Remove(activity);
            Debug.Log(string.Format("{0}: Stack management done", TAG));
            var lastActivity = activityStack.Count == 0 ? null : activityStack.Last();

            // Transition animation, change state
            StartCoroutine(PopActivityTransition(activity, lastActivity));
        }

        public IEnumerator PopActivityTransition(ActivityBehaviour popActivity, ActivityBehaviour lastActivity)
        {
            Debug.Log(string.Format("{0}: Pop current activity transition", TAG));
            var delayTimePause = popActivity.Transition(ActivityLifecycle.PAUSED);
            Debug.Log(string.Format("{0}: Activity {1} is paused [{2} secs]", TAG, popActivity.name, delayTimePause));
            yield return new WaitForSecondsRealtime(delayTimePause);

            var delayTimeClose = popActivity.Transition(ActivityLifecycle.CLOSED);
            Debug.Log(string.Format("{0}: Activity {1} is closed [{2} secs]", TAG, popActivity.name, delayTimeClose));
            yield return new WaitForSecondsRealtime(delayTimeClose);

            activityStack.Remove(popActivity);
            Debug.Log(string.Format("{0}: {1} is removed from stack", TAG, popActivity));
            var resultBundle = popActivity.GetResultBundle();
            Destroy(popActivity.gameObject);
            Debug.Log(string.Format("{0}: Pop activity {1} from stack", TAG, popActivity.name));
            if (lastActivity != null)
            {
                lastActivity.OnResultFromTop(resultBundle);
                Debug.Log(string.Format("{0}: {1} handled result from {2}", TAG, lastActivity, popActivity));
                var delayTimeResume = lastActivity.Transition(ActivityLifecycle.RESUMED);
                Debug.Log(string.Format("{0}: Activity {1} is resumed [{2} secs]", TAG, lastActivity.name, delayTimeResume));
                yield return new WaitForSecondsRealtime(delayTimeResume);
            }
            yield return 0f;
        }
    }
}