using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Futulabs
{
    public class BasketHoop : MonoBehaviour
    {
        public GameManager game;
        public BasketTrigger upperTrigger;
        public BasketTrigger lowerTrigger;
        private bool UpperOK = false;
        private GameObject objectOfInterest;
        private float MaxTime = 3f;
        private float dt = 0;

        void Update()
        {
            dt += Time.deltaTime;
        }

        public void Triggered(BasketTrigger trig, GameObject bBall)
        {
            if (upperTrigger == trig) //You triggered the upper part, save the object
            {
                dt = 0;
                UpperOK = true;
                objectOfInterest = bBall;
            }
            if (lowerTrigger == trig && bBall == objectOfInterest && dt <= MaxTime)
            {
                game.BasketScore();
            }
        }
    }
}
