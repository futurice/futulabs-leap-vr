using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Futulabs
{

    public class AudioManager : Singleton<AudioManager>
    {
        [Header("AudioManager")]
        [SerializeField]
        private AudioSource _effectAudioSource;
        public List<GameAudioClip> _gameAudioClips;

        private Dictionary<GameAudioClipType, AudioClip> _audioClipDictionary;

        private Dictionary<GameAudioClipType, AudioClip> AudioClipDictionary
        {
            get
            {
                try
                {
                    if (_audioClipDictionary == null)
                    {
                        _audioClipDictionary = new Dictionary<GameAudioClipType, AudioClip>();
                        int numGameAudioClips = _gameAudioClips.Count;

                        for (int i = 0; i < numGameAudioClips; ++i)
                        {
                            _audioClipDictionary.Add(_gameAudioClips[i].audioClipType, _gameAudioClips[i].audioClip);
                        }
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogErrorFormat("AudioManager AudioClipDictionary: Error while creating AudioClipDictionary - {0}", e.Message);
                }

                return _audioClipDictionary;
            }
        }

        public AudioClip GetAudioClip(GameAudioClipType type)
        {
            AudioClip audioClip = null;
            AudioClipDictionary.TryGetValue(type, out audioClip);
            return audioClip;
        }

        public void PlayAudioClip(int type)
        {
            PlayAudioClip((GameAudioClipType)type);
        }

        public void PlayAudioClip(GameAudioClipType type)
        {
            _effectAudioSource.PlayOneShot(GetAudioClip(type));
        }
    }

}
