using System.Collections.Generic;
using UnityEngine;

namespace FusionExamples.Tanknarok
{
	/// <summary>
	/// マテリアル、パーティクル、UI画像の色をまとめて変更するためのユーティリティクラス。
	/// 子オブジェクトに含まれるColorChangerを一括操作する機能も提供します。
	/// </summary>
	public class ColorChanger : MonoBehaviour
	{
		private ParticleSystem _partSystem;
		private MeshRenderer _meshRenderer;
		private UnityEngine.UI.Image _imageComponent;

		private ParticleSystem.MainModule _mainModule;

		/// <summary>
		/// アタッチされているコンポーネント（ParticleSystem, MeshRenderer, Image）を取得・初期化します。
		/// </summary>
		public void Initialize()
		{
			_partSystem = GetComponent<ParticleSystem>();
			_meshRenderer = GetComponent<MeshRenderer>();
			_imageComponent = GetComponent<UnityEngine.UI.Image>();
		}

		/// <summary>
		/// 取得したコンポーネントのカラー（マテリアルカラーやパーティクルの開始色など）を指定した色に変更します。
		/// </summary>
		public void ChangeColor(Color newColor)
		{
			if (_meshRenderer != null)
			{
				_meshRenderer.material.color = newColor;
			}

			if (_partSystem != null)
			{
				try
				{
					_mainModule = _partSystem.main;
					_mainModule.startColor = newColor;
				}
				catch (System.NullReferenceException)
				{
					Debug.LogError("NullReference in ColorChanger. (Do not create your own module instances)");
				}
			}

			if (_imageComponent != null)
			{
				_imageComponent.color = newColor;
			}
		}
		
		/// <summary>
		/// 指定したTransformとその子孫オブジェクトにアタッチされているColorChangerを再帰的に探し、リストに追加します。
		/// </summary>
		public static void FindColorChangers(Transform currentTransform, ref List<ColorChanger> colorChangers)
		{
			ColorChanger colorChanger = currentTransform.GetComponent<ColorChanger>();
			if (colorChanger != null)
			{
				colorChangers.Add(colorChanger);
				colorChanger.Initialize();
			}

			foreach (Transform go in currentTransform)
			{
				FindColorChangers(go, ref colorChangers);
			}
		}

		/// <summary>
		/// 取得したColorChangerのリストすべてに対して、色を一括で変更します。
		/// </summary>
		public static void ChangeColor(Color color, List<ColorChanger> colorChangers)
		{
			foreach (ColorChanger colorChanger in colorChangers)
			{
				colorChanger.ChangeColor(color);
			}
		}

		/// <summary>
		/// 指定したルートオブジェクトの配下にあるすべてのColorChangerの色を変更します。
		/// </summary>
		public static void ChangeColor(Transform fromRoot, Color color)
		{
			List<ColorChanger> changers = new List<ColorChanger>();
			FindColorChangers( fromRoot, ref changers);
			ChangeColor(color,changers);
		}
	}
}