using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpineTest : MonoBehaviour
{
	[SerializeField]
	private Animator _animator;

	[SerializeField]
	private Button _templateButton;

	[SerializeField]
	private Toggle _templateToggle;

	private void Start()
	{
		_templateButton.gameObject.SetActive(value: true);
		foreach (AnimatorControllerParameter parameter in _animator.parameters.Where((AnimatorControllerParameter p) => p.type == AnimatorControllerParameterType.Trigger))
		{
			Button button = Object.Instantiate(_templateButton, _templateButton.transform.parent);
			button.GetComponentInChildren<TextMeshProUGUI>().text = parameter.name;
			button.onClick.AddListener(delegate
			{
				ResetTriggers();
				_animator.SetTrigger(parameter.name);
			});
		}
		_templateButton.gameObject.SetActive(value: false);
		_templateToggle.gameObject.SetActive(value: true);
		foreach (AnimatorControllerParameter parameter2 in _animator.parameters.Where((AnimatorControllerParameter p) => p.type == AnimatorControllerParameterType.Bool))
		{
			Toggle toggle = Object.Instantiate(_templateToggle, _templateToggle.transform.parent);
			toggle.GetComponentInChildren<TextMeshProUGUI>().text = parameter2.name;
			toggle.onValueChanged.AddListener(delegate(bool v)
			{
				_animator.SetBool(parameter2.name, v);
			});
		}
		_templateToggle.gameObject.SetActive(value: false);
	}

	private void ResetTriggers()
	{
		foreach (AnimatorControllerParameter item in _animator.parameters.Where((AnimatorControllerParameter p) => p.type == AnimatorControllerParameterType.Trigger))
		{
			if (item.type == AnimatorControllerParameterType.Trigger)
			{
				_animator.ResetTrigger(item.name);
			}
		}
	}
}
