using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WIP_Tilemap : MonoBehaviour
{
	[SerializeField]
	private Tilemap _tilemap;

	[SerializeField]
	private GameObject[] _places;

	public Dictionary<Vector2Int, GameObject> PlaceTiles = new Dictionary<Vector2Int, GameObject>();

	private void Start()
	{
		GameObject[] places = _places;
		foreach (GameObject gameObject in places)
		{
			int num = (int)gameObject.transform.position.x;
			int num2 = num - 10;
			int num3 = num + 10;
			int num4 = (int)gameObject.transform.position.y;
			int num5 = num4 - 10;
			int num6 = num4 + 10;
			Vector2Int b = new Vector2Int(num, num4);
			for (int j = num2; j < num3; j++)
			{
				for (int k = num5; k < num6; k++)
				{
					Vector2Int vector2Int = new Vector2Int(j, k);
					if (Vector2Int.Distance(vector2Int, b) < 3f)
					{
						PlaceTiles.Add(vector2Int, gameObject);
					}
				}
			}
		}
	}

	public bool Test(Vector3Int pos)
	{
		return _tilemap.GetTile(pos) == null;
	}
}
