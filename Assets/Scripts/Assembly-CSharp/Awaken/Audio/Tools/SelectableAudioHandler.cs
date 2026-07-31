using Awaken.Audio.Base;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Awaken.Audio.Tools
{
	public class SelectableAudioHandler : MonoBehaviour, IPointerClickHandler, IEventSystemHandler, IPointerEnterHandler
	{
		[SerializeField]
		private AudioEventObject _onButtonClicked;

		[SerializeField]
		private AudioEventObject _onButtonHovered;

		private Selectable _selectable;

		private void Start()
		{
			_selectable = GetComponent<Selectable>();
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			if (_selectable.enabled && _selectable.IsInteractable() && _onButtonClicked != null)
			{
				_onButtonClicked.Play();
			}
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			if (_selectable.enabled && _selectable.IsInteractable() && _onButtonHovered != null)
			{
				_onButtonHovered.Play();
			}
		}
	}
}
