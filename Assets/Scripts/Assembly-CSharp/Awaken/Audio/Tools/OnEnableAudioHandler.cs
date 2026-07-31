using Awaken.Audio.Base;
using UnityEngine;

namespace Awaken.Audio.Tools
{
	public class OnEnableAudioHandler : MonoBehaviour
	{
		[SerializeField]
		private AudioEventObject _onEnabledAudio;

		private void OnEnable()
		{
			_onEnabledAudio.Play();
		}
	}
}
