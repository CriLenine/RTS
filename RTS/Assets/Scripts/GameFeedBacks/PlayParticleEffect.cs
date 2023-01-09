using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using static UnityEngine.RuleTile.TilingRuleOutput;

namespace RTS.Feedback
{
    [System.Serializable]
    [GameFeedback(120, 153, 255, "PlayParticleEffect")]
    public class PlayParticleEffect : GameFeedback
    {
        [SerializeField] private ParticleSystem _particleSystemPrefab;

        protected override void Execute(GameObject gameObject)
        {

            if (_particleSystemPrefab.gameObject == null ) return;
            Vector3 position = gameObject.transform.position;


            ParticleSystem particleSystem = GameObject.Instantiate(_particleSystemPrefab, position, Quaternion.identity);

            particleSystem.Play();
        }

        public override string ToString()
        {
            return $"Play {_particleSystemPrefab}";
        }
    }
}

