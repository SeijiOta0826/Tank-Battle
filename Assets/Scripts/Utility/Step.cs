namespace FusionExamples.Utility
{
	/// <summary>
	/// 端数の値をステップで進める際に、正確に1.0で終了するようにするためのヘルパークラスです。
	/// コルーチンやforループ内でのアニメーション、フェード処理などに使用されます。
	/// 
	/// Convenient helper to use in for-loops when stepping fractional values but want to be sure to always end exactly at 1.0f
	/// If the counter is initialized to a negative value, the first value returned will be 0
	/// for(float f=-1;Step.Forward(ref f, some_step); )
	/// {
	///  // This block will execute once for all values of f=[0;1] and is guaranteed to be called for both values 0 and 1, regardless of some_step. 
	/// }
	/// If used in a coroutine, you can loop each frame over a second like so:
	///
	/// for(float f=0;Step.Forward(ref f); )
	/// {
	///   yield return new WaitForEndOfFrame();
	/// }
	/// </summary>
	public static class Step
	{
		/// <summary>
		/// 値を0から1に向かって増加させます。1.0に達した場合はfalseを返し、ループの終了条件として使えます。
		/// </summary>
		public static bool Forward(ref float f, float step)
		{
			if (f < 0)
			{
				f = 0;
				return true;
			}

			if (f == 1)
				return false;

			f = f + step;
			if (f >= 1)
				f = 1;
			return true;
		}

		/// <summary>
		/// 値を1から0に向かって減少させます。0.0に達した場合はfalseを返します。
		/// </summary>
		public static bool Backwards(ref float f, float step)
		{
			if (f > 1)
			{
				f = 1;
				return true;
			}

			if (f == 0)
				return false;

			f = f - step;
			if (f <= 0)
				f = 0;
			return true;
		}
	}
}