using UnityEngine;

namespace Viento.UnityActivity
{
    public interface IActivityFactory
    {
        public ActivityBehaviour Create(Transform parent);
    }
}