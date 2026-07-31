using UnityEngine;
using UnityEngine.InputSystem;

public class TestLine : MonoBehaviour
{
	[SerializeField]
	private RectTransform _origin;

	[SerializeField]
	private RectTransform _rect;

	private Canvas _canvas;

	[SerializeField]
	private Camera _worldCamera;

	public RectTransform TempR;

	public Transform beginTransform;

	public Transform endTransform;

	private void Start()
	{
		_canvas = GetComponentInParent<Canvas>();
	}

	private void Update()
	{
		if ((bool)beginTransform)
		{
			_origin.position = beginTransform.position;
		}
		float num = 1f / _canvas.transform.localScale.x;
		Vector2 vector = _origin.position;
		if ((bool)endTransform)
		{
			TempR.localPosition = MousePosition2(endTransform.position);
		}
		else
		{
			TempR.localPosition = Mouse.current.position.ReadValue();
		}
		Vector2 vector2 = TempR.position;
		Vector2 vector3 = vector - vector2;
		Vector2 normalized = vector3.normalized;
		float z = Mathf.Atan2(normalized.y, normalized.x) * 57.29578f;
		float x = vector3.magnitude * num;
		_rect.sizeDelta = new Vector2(x, _rect.sizeDelta.y);
		_origin.rotation = Quaternion.Euler(0f, 0f, z);
	}

	private Vector3 MousePosition2(Vector3 position)
	{
		Camera main = Camera.main;
		position.z = main.nearClipPlane;
		position = main.WorldToScreenPoint(position);
		position.z = main.nearClipPlane + main.transform.position.z;
		return position;
	}
}
