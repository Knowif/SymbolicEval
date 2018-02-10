using System;
using System.Collections.Generic;

namespace Symbolic
{
	public class NumberNode : Node
	{
		public Number Value;

		public NumberNode(Number x) { Value = x; }

		public override bool CanSimplify() { return false; }

		public override void Simplify() { }

		public override bool CanReplace() { return false; }

		public override Node Replace() { return null; }

		public override double NumericEval() { return Value.Numeric(); }

		public override bool DirectEqualsTo(Node n) { return ((NumberNode)n).Value == Value; }

		public override string Print() { return Value.ToString(); }

		public override IEnumerable<Node> GetChildren() { return new Node[] { }; }
	}

	public class UnknownNode : Node
	{
		public String Name;

		public UnknownNode(String n) { Name = n; }

		public override bool CanSimplify() { return false; }

		public override void Simplify() { }

		public override bool CanReplace() { return false; }

		public override Node Replace() { return null; }

		public override double NumericEval() { throw new InvalidOperationException(); }

		public override bool DirectEqualsTo(Node n) { return ((UnknownNode)n).Name == Name; }

		public override string Print() { return Name; }

		public override IEnumerable<Node> GetChildren() { return new Node[] { }; }
	}
}
