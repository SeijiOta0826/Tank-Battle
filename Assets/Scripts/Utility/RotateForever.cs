using UnityEngine;
using UnityEngine.Serialization;

namespace FusionExamples.Tanknarok
{
	/// <summary>
	/// アタッチしたオブジェクトを指定した軸・速度で常に回転させ続けるスクリプトです。
	/// </summary>
	public class RotateForever : MonoBehaviour
	{
		private enum Axis { X, Y, Z };

		[SerializeField] private Axis axis = Axis.Y;
		[SerializeField] bool reverse = false;
		[SerializeField] public float _rotationsPerSecond = 1f;

		void Update()
		{
			Rotate();
		}

		/// <summary>
		/// 回転処理を行います。フレーム間の経過時間（Time.deltaTime）を用いて滑らかに回転させます。
		/// </summary>
		void Rotate()
		{
			float direction = reverse == true ? -1 : 1;
			// 1秒あたりの回転数に360度を掛け、経過時間と方向を掛けて回転量を計算
			float rotation = _rotationsPerSecond * 360f * Time.deltaTime * direction;

			switch (axis)
			{
				case Axis.X:
					transform.Rotate(rotation, 0, 0);
					break;
				case Axis.Y:
					transform.Rotate(0, rotation, 0);
					break;
				case Axis.Z:
					transform.Rotate(0, 0, rotation);
					break;
			}
		}
	}
}