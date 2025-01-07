
namespace Viento.UnityActivity
{
    public enum ActivityStartType
    {
        INSERT, /* Start activity with inserting into activity stack */
        REPLACE, /* Start activity with replacing with last activity item of stack */
        NEW_TASK, /* Start activity with clearing activity stack */
    }
}