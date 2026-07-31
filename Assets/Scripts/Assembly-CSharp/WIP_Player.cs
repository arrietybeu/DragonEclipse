using TMPro;
using UnityEngine;

public class WIP_Player : MonoBehaviour
{
	[SerializeField]
	private WIP_Tilemap _tileMap;

	[SerializeField]
	private Transform _cameraTransform;

	[SerializeField]
	private TextMeshProUGUI _placeLabel;

	[SerializeField]
	private GameObject _button;

	private Vector3 _target;

	private bool _isMoving;

	private void Start()
	{
		_target = base.transform.position;
	}

	private void Update()
	{
		if (!_isMoving)
		{
			TestInput();
		}
		else if (Vector3.Distance(base.transform.position, _target) <= 0.05f)
		{
			_isMoving = false;
			base.transform.position = _target;
		}
		else
		{
			Vector3 vector = _target - base.transform.position;
			base.transform.position += vector.normalized * (Time.deltaTime * 7f);
		}
		Vector3 vector2 = base.transform.position - _cameraTransform.position;
		Vector3 normalized = vector2.normalized;
		float num = Mathf.Clamp(vector2.magnitude, 0f, 5f);
		Vector3 position = _cameraTransform.position + normalized * (num * Time.deltaTime);
		position.z = -1f;
		_cameraTransform.position = position;
	}

	private void TestInput()
	{
		_tileMap.PlaceTiles.TryGetValue(new Vector2Int((int)_target.x, (int)_target.y), out var value);
		bool flag = value != null && value.activeInHierarchy;
		_placeLabel.text = (flag ? value.name : string.Empty);
		_button.SetActive(flag);
		if (Input.GetKey(KeyCode.W))
		{
			Vector3 target = _target + Vector3.up;
			if (CanMove(flag, target))
			{
				_target = target;
				_isMoving = true;
			}
		}
		if (Input.GetKey(KeyCode.S))
		{
			Vector3 target2 = _target + Vector3.down;
			if (CanMove(flag, target2))
			{
				_target = target2;
				_isMoving = true;
			}
		}
		if (Input.GetKey(KeyCode.A))
		{
			Vector3 target3 = _target + Vector3.left;
			if (CanMove(flag, target3))
			{
				_target = target3;
				_isMoving = true;
			}
		}
		if (Input.GetKey(KeyCode.D))
		{
			Vector3 target4 = _target + Vector3.right;
			if (CanMove(flag, target4))
			{
				_target = target4;
				_isMoving = true;
			}
		}
	}

	private bool CanMove(bool isOnPlace, Vector3 target)
	{
		Vector3Int pos = new Vector3Int((int)target.x, (int)target.y, 0);
		if (!_tileMap.Test(pos))
		{
			Vector2Int key = new Vector2Int((int)target.x, (int)target.y);
			_tileMap.PlaceTiles.TryGetValue(key, out var value);
			if (isOnPlace && value != null && value.activeInHierarchy)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public void HidePlace()
	{
		_tileMap.PlaceTiles.TryGetValue(new Vector2Int((int)_target.x, (int)_target.y), out var value);
		if (value != null)
		{
			value.SetActive(value: false);
		}
		TestInput();
	}
}
