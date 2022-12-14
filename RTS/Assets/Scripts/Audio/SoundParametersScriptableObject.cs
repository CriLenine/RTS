using UnityEngine;

namespace RTS
{
    [CreateAssetMenu(fileName = "SoundParameters", menuName = "RTS/Audio/Sound Parameters")]
    public class SoundParametersScriptableObject : ScriptableObject
    {
        public SoundParameters Parameters => _Parameters;

        [SerializeField] private SoundParameters _Parameters = new SoundParameters(null);


        public static implicit operator SoundParameters(SoundParametersScriptableObject parameters)
        {
            return parameters._Parameters;
        }
    }
}
