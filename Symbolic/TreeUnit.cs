using System;
using System.Collections.Generic;

namespace Symbolic
{
	public class NumberNode : Node
	{
		public Number Value;

		public NumberNode(Number x) { Value = x; }

		public override SimplifyResult Simplify() { return new SimplifyResult(false); }

		public override SimplifyResult Replace(out Node node)
		{
			node = null;
			return new SimplifyResult(false);
		}

		public override double NumericEval() { return Value.Numeric(); }

		public override string Print() { return Value.ToString(); }

		public override IEnumerable<Node> GetChildren() { return new Node[] { }; }
	}

	public class UnknownNode : Node
	{
		public String Name;

		public UnknownNode(String n) { Name = n; }

		public override SimplifyResult Simplify() { return new SimplifyResult(false); }

		public override SimplifyResult Replace(out Node node)
		{
			node = null;
			return new SimplifyResult(false);
		}

		public override double NumericEval() { throw new InvalidOperationException(); }

		public override string Print() { return Name; }

		public override IEnumerable<Node> GetChildren() { return new Node[] { }; }
	}
}
