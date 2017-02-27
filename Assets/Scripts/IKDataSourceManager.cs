using UnityEngine;
using Leap.Unity;

namespace Futulabs
{ 
    public class IKDataSourceManager : Singleton<IKDataSourceManager>
    {
        [Header("DataSources")]
        [SerializeField]
        private LeapServiceProvider _leapServiceProvider;
        [SerializeField]
        private KinectManager _kinectManager;

        public Leap.Controller LeapController
        {
            get
            {
                return _leapServiceProvider.GetLeapController();
            }
        }

        public KinectManager KinectManager
        {
            get
            {
                return _kinectManager;
            }
        }
    }
}
