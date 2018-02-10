using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Symbolic
{
	/// <summary>
	/// Rational number.
	/// </summary>
	public class Number
	{
		public long N { get; private set; }
		public long M { get; private set; } // always positive!

		public static readonly Number Zero = new Number(0);
		public static readonly Number One = new Number(1);

		public Number(long nn, long mm)
		{
			int sign = ((nn < 0) != (mm < 0)) ? -1 : 1;
			N = Math.Abs(nn) * sign; M = Math.Abs(mm);

			Simplify();
		}

		public Number(double num, int digits = 6)
		{
			int sign = (num < 0 ? -1 : 1);
			long division = 1; double nn = Math.Abs(num);

			num = Math.Abs(num);
			double eps = 1 / Math.Pow(10, digits); int i = 0;
			while (num - Math.Floor(Math.Round(num, digits)) > eps)
			{
				if (i >= digits) break;
				num -= Math.Floor(Math.Round(num, digits));
				num *= 10;
				division *= 10;
				i++;

				//Console.WriteLine("Num = " + num + "\nDiv = " + division);
			}

			N = (int)Math.Round(nn * division) * sign;
			M = division;

			Simplify();
		}

		public double Numeric() { return (double)N / M;  }

		public static Number operator -(Number x)
		{
			return new Number(-x.N, x.M);
		}

		public static Number operator +(Number a, Number b)
		{
			//  a1     b1     a1 * b2 + b1 * a2
			// ---- + ---- = -------------------
			//  a2     b2          a2 * b2
			long nn, mm;
			nn = a.N * b.M + b.N * a.M;
			mm = a.M * b.M;
			return new Number(nn, mm);
		}

		public static Number operator -(Number a, Number b)
		{
			return a + (- b);
		}

		public static Number operator *(Number a, Number b)
		{
			//  a1     b1     a1 * b1
			// ---- * ---- = ---------
			//  a2     b2     a2 * b2
			long nn, mm;
			nn = a.N * b.N;
			mm = a.M * b.M;
			return new Number(nn, mm);
		}

		public static Number operator /(Number a, Number b)
		{
			//  a1     b1     a1 * b2
			// ---- / ---- = ---------
			//  a2     b2     a2 * b1
			long nn, mm;
			nn = a.N * b.M;
			mm = a.M * b.N;
			return new Number(nn, mm);
		}

		public static bool operator >(Number a, Number b)
		{
			return a.ToDouble() > b.ToDouble();
		}

		public static bool operator <(Number a, Number b)
		{
			return a.ToDouble() < b.ToDouble();
		}

		public static bool operator ==(Number a, Number b)
		{
			return a.N == b.N && a.M == b.M;
		}

		public static bool operator !=(Number a, Number b)
		{
			return a.N != b.N || a.M != b.M;
		}

		public double ToDouble() { return (double)N / M; }

		public override string ToString()
		{
			if (M == 1) return N.ToString();
			return N.ToString() + "/" + M.ToString();
		}

		private void Simplify()
		{
			// find GCD
			long gcd = Math.Abs(N), b = M;
			while (b != 0)
			{
				long r = b;
				b = gcd % b;
				gcd = r;
			}

			N /= gcd;
			M /= gcd;
		}

		public override bool Equals(object obj)
		{
			var number = obj as Number;
			return number != null &&
				   N == number.N &&
				   M == number.M;
		}
	}
}
