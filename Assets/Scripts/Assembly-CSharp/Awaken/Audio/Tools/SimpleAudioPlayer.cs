using Awaken.Audio.Base;
using Awaken.Data.Audio;
using UnityEngine;

namespace Awaken.Audio.Tools
{
	public class SimpleAudioPlayer : MonoBehaviour
	{
		[SerializeField]
		private AudioEventData _audioData;

		[SerializeField]
		private bool playOneShot;

		[SerializeField]
		private bool playOnEnable;

		private AudioEventHandler _audioEventHandler;

		private void OnEnable()
		{
			if (playOnEnable)
			{
				PlayAudio();
			}
		}

		public void PlayAudio()
		{
			if (_audioEventHandler == null)
			{
				_audioEventHandler = new AudioEventHandler();
			}
			if (playOneShot)
			{
				_audioEventHandler.PlayOneShot(_audioData);
			}
			else
			{
				_audioEventHandler.Play(_audioData);
			}
		}

		private void OnDisable()
		{
			_audioEventHandler?.DisposeAll();
		}

		private void OnDestroy()
		{
			_audioEventHandler?.DisposeAll();
		}
	}
}
