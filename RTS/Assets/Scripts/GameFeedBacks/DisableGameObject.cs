using RTS.Feedback;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RTS.Feedback
{
    [System.Serializable]
    [GameFeedback(100, 200, 255, "DisableGameobject")]
    public class DisableGameObject : GameFeedback
    {
        protected override void Execute(GameObject gameObject)
        {
            gameObject.SetActive(false);
        }

        public override string ToString()
        {
            return $"DisableGameobject";
        }
    }

}
