﻿using UnityEngine;
using DS = Config.DataSingleton;

namespace WallSystem
{
	public class RotateBlock : MonoBehaviour {
		// Update is called once per frame
		private void Update () {
			// ...also rotate around the World's Y axis
			// transform.Rotate(0, 0.1f * Time.deltaTime * DS.GetData().CharacterData.GoalRotationSpeed, 0, Space.World);
		}
	}
}
