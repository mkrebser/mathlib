/// <summary>
/// Linear congruential generator. Probably not as good as System.Random, but has less overhead.
/// </summary>
public struct SimpleRandom
{
	// values just from https://en.wikipedia.org/wiki/Linear_congruential_generator
	const long a = 214013;
	const long b = 2531011;
	const long m = 4294967296;
	long x;

	const double intmaxinverse = 1.0 / int.MaxValue;

	/// <summary>
	/// Constructing this object with a specified seed. The default constructor uses seed = 0
	/// </summary>
	/// <param name="seed"></param>
	public SimpleRandom(int seed = 0)
	{
		x = seed;
	}

	/// <summary>
	/// next int min inclusive + 1 to max exclusive (Positive only)
	/// </summary>
	/// <param name="min"></param>
	/// <param name="max"></param>
	/// <returns></returns>
	public int Next(int min, int max)
	{
		unchecked
		{
			var range = max - min;
			if (max < 0 || min < 0 || range < 1)
				throw new System.Exception("Bad input");
			var next = Next();
			return min + next % range;
		}
	}

	/// <summary>
	/// Returns -1 or 1
	/// </summary>
	/// <returns></returns>
	public int NextSign()
	{
		return NextN() < 0 ? -1 : 1;
	}

	/// <summary>
	/// next int 0 inclusive to int.max exclusive
	/// </summary>
	/// <param name="min"></param>
	/// <param name="max"></param>
	/// <returns></returns>
	public int Next()
	{
		unchecked
		{
			x = (a * x + b) % m;
			var result = (int)x;

			if (result < 0) // to avoid even odd even odd pattern.. randomly perform again 1 or 2 times
			{
				x = (a * x + b) % m;
				return System.Math.Abs((int)x);
			}
			else
			{
				x = (a * x + b) % m;
				x = (a * x + b) % m;
				return System.Math.Abs((int)x);
			}
		}
	}

	/// <summary>
	/// next int min inclusive + 1 to max exclusive
	/// </summary>
	/// <param name="min"></param>
	/// <param name="max"></param>
	/// <returns></returns>
	public int NextN(int min, int max)
	{
		unchecked
		{
			var range = max - min;
			if (range < 1)
				throw new System.Exception("Bad range");
			var next = NextN();
			return min + System.Math.Abs(next % (range));
		}
	}

	/// <summary>
	/// next int int.min + 1 inclusive to int.max exclusive 
	/// </summary>
	/// <param name="min"></param>
	/// <param name="max"></param>
	/// <returns></returns>
	public int NextN()
	{
		unchecked
		{
			x = (a * x + b) % m;
			var result = (int)x;
			
			if (result < 0) // to avoid even odd even odd pattern.. randomly perform again 1 or 2 times
			{
				x = (a * x + b) % m;
				return (int)x;
			}
			else
			{
				x = (a * x + b) % m;
				x = (a * x + b) % m;
				return (int)x;
			}
		}
	}

	/// <summary>
	/// Next double 0...1
	/// </summary>
	/// <returns></returns>
	public double NextDouble()
	{
		var next = Next();
		return (double)next * intmaxinverse;
	}

	/// <summary>
	/// Next double -1...1
	/// </summary>
	/// <returns></returns>
	public double NextDoubleN()
	{
		var next = NextN();
		return (double)next * intmaxinverse;
	}

	/// <summary>
	/// Next float 0...1
	/// </summary>
	/// <returns></returns>
	public float NextSingle()
	{
		return (float)NextDouble();
	}

	/// <summary>
	/// next float -1...1
	/// </summary>
	/// <returns></returns>
	public float NextSingleN()
	{
		return (float)NextDoubleN();
	}
}