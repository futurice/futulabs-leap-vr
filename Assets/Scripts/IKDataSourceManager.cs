using UnityEngine;
using Leap.Unity;

namespace Futulabs
{ 
    public class IKDataSourceManager : Singleton<IKDataSourceManager>
    {
        [Header("DataSources")]
        [SerializeField]
        private LeapServiceProvider _leapServiceProvider;

        public Leap.Controller LeapController
        {
            get
            {
                return _leapServiceProvider.GetLeapController();
            }
        }
    }
}
