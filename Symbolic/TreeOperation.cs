using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Symbolic 
{
	public class SumNode: MultiChildNode
	{
		// Simplify Rule:
		//  1. Sum{[a], Sum{[b], [c]}} = Sum{[a], [b], [c]}
		//  2. Sum{[a], (2), (3)} = Sum{[a], (5)}
		//  3. Sum{[a]} = [a]
		//  4. Sum{[a], [b], (0)} = Sum{[a], [b]}

		public SumNode(params Node[] children)
		{
			Children = new List<Node>(children);
		}

		public SumNode(IEnumerable<Node> children)
		{
			Children = new List<Node>(children);
		}

		public override bool CanSimplify()
		{
			int numCount = 0;
			foreach (Node n in Children)
				if (n.CanReplace())
					return true;
				else if (typeof(SumNode).IsInstanceOfType(n))
					return true;
				else if (typeof(NumberNode).IsInstanceOfType(n))
					if (((NumberNode)n).Value.N == 0) return true;
					else numCount++;

			return numCount >= 2;
		}

		public override void Simplify()
		{
			Number numSum = new Number(0);
			List<Node> newChildren = new List<Node>();

			foreach (Node n in Children)
				if (n.CanReplace())
					newChildren.Add(n.Replace());
				else if (typeof(SumNode).IsInstanceOfType(n))
					newChildren.AddRange(((SumNode)n).Children);
				else if (typeof(NumberNode).IsInstanceOfType(n))
					numSum += ((NumberNode)n).Value;
				else
					newChildren.Add(n);

			Children = newChildren;
			if (numSum.N != 0) Children.Add(new NumberNode(numSum));
		}

		public override bool CanReplace() { return Children.Count == 1; }

		public override Node Replace() { return Children[0]; }

		public override double NumericEval()
		{
			double x = 0;
			foreach (Node n in Children)
				x += n.NumericEval();
			return x;
		}

		public override string Print() {
			return "(" + string.Join(") + (", (from Node n in Children select n.Print())) + ")";
		}
	}

	public class ProductNode : MultiChildNode
	{
		public List<bool> Sign;

		public ProductNode(IEnumerable<Node> children, IEnumerable<bool> sign)
		{
			Children = new List<Node>(children);
			Sign = new List<bool>(sign);
		}

		public override bool CanReplace()
		{
			return true;
		}

		public override bool CanSimplify()
		{
			return false;
		}

		public override double NumericEval()
		{
			double result = 1;
			for (int i = 0; i < Children.Count; i++)
				if (Sign[i]) result *= Children[i].NumericEval();
				else result /= Children[i].NumericEval();

			return result;
		}

		public override string Print()
		{
			if (Children.Count == 0) return "";
			string result = "(" + Children[0].Print() + ")";
			for (int i = 1; i < Children.Count; i++)
				if (Sign[i]) result += " * (" + Children[i].Print() + ")";
				else result += " / (" + Children[i].Print() + ")";

			return result;
		}

		public override Node Replace()
		{
			if (Children.Count == 0) return new MultiNode();
			Node node = Children[0];
			for (int i = 1; i < Children.Count; i++)
				if (Sign[i]) node = new MultiNode(Children[i], node);
				else node = new DivideNode(node, Children[i]);

			return node;
		}

		public override void Simplify()
		{
			throw new NotImplementedException();
		}
	}

	public class MultiNode : MultiChildNode
	{
		// Simplify Rule:
		//  1. Mult{[a], Mult{[b], [c]}} = Mult{[a], [b], [c])
		//  2. Mult{[a], [b], Sum{[c], [d]}} = 
		//      Sum{Mult{[a], [b], [c]}, Mult{[a], [b], [d]}}		(*)
		//  3. Mult{[a], (2), (3)} = Mult{[a], (6)}
		//  3a.Mult{[a], (1)} = Mult{[a]}
		//  3b.Mult{[a], (0)} = (0)									(*)
		//  4. Mult{[a]} = [a]										(*)
		//
		//  (*) use CanReplace

		public MultiNode(params Node[] children)
		{
			Children = new List<Node>();
			Children.AddRange(children);
		}

		public MultiNode(IEnumerable<Node> children)
		{
			Children = new List<Node>();
			Children.AddRange(children);
		}

		public override bool CanSimplify()
		{
			int numCount = 0;
			foreach (Node n in Children)
				if (n.CanReplace())
					return true;
				else if (typeof(MultiNode).IsInstanceOfType(n))
					return true;
				else if (typeof(NumberNode).IsInstanceOfType(n))
					if (((NumberNode)n).Value.Numeric() == 1)
						return true;
					else
						numCount++;

			return numCount >= 2;
		}

		public override void Simplify()
		{
			Number numProduct = new Number(1);
			List<Node> newChildren = new List<Node>();
			foreach (Node n in Children)
				if (n.CanReplace())
					newChildren.Add(n.Replace());
				else if (typeof(MultiNode).IsInstanceOfType(n))
					newChildren.AddRange(((MultiNode)n).Children);
				else if (typeof(NumberNode).IsInstanceOfType(n))
					numProduct *= ((NumberNode)n).Value;
				else
					newChildren.Add(n);

			if (numProduct.Numeric() != 1) newChildren.Add(new NumberNode(numProduct));
			Children = newChildren;
		}

		public override bool CanReplace()
		{
			if (Children.Count == 1) return true;

			foreach (Node n in Children)
				if (typeof(NumberNode).IsInstanceOfType(n) && ((NumberNode)n).Value.N == 0)
					return true;
				else if (typeof(SumNode).IsInstanceOfType(n))
					return true;

			return false;
		}

		public override Node Replace()
		{
			if (Children.Count == 1) return Children[0];

			foreach (Node n in Children)
				if (typeof(NumberNode).IsInstanceOfType(n) && ((NumberNode)n).Value.N == 0)
					return n;
				else if (typeof(SumNode).IsInstanceOfType(n))
				{
					List<Node> l = new List<Node>();
					foreach (Node child in ((SumNode)n).Children)
					{
						List<Node> ll = new List<Node>(Children);
						for (int i = 0; i < ll.Count; i++)
							if (ll[i] == n)
							{
								ll.RemoveAt(i);
								break;
							}
						ll.Add(child);
						l.Add(new MultiNode(ll));
					}
					return new SumNode(l);
				}

			return null;
		}

		public override double NumericEval()
		{
			double product = 1;
			foreach (Node n in Children)
				product *= n.NumericEval();

			return product;
		}

		public override string Print()
		{
			return "(" + string.Join(") * (", (from Node n in Children select n.Print())) + ")";
		}
	}

	public class DivideNode : BinaryNode
	{
		// Simplify Rules: 
		// 1. Div{[a], (5)} = Mult{(1/5), [a]}
		// 2. Div{Mult{[a], [b], [c]}, [a]} = Mult{[b], [c]}
		// 3. Div{Mult{[a], [b], [c]}, Mult{[a], [b]}} = Div{Mult{[c]}, Mult{}} = Div{[c], (1)} = [c]
		//
		// WARNING: ASSUME UNKNOWNS NOT ZERO!

		public DivideNode(Node l, Node r)
		{
			LChild = l; RChild = r;
		}

		public override bool CanReplace()
		{
			if (typeof(NumberNode).IsInstanceOfType(RChild))
				return true;
			else if (typeof(UnknownNode).IsInstanceOfType(RChild))
				if (typeof(MultiNode).IsInstanceOfType(LChild))
					if (((MultiNode)LChild).ContainsNode(RChild))
						return true;

			return false;
		}

		public override bool CanSimplify()
		{
			if (LChild.CanReplace() || RChild.CanReplace()) return true;

			if (typeof(MultiNode).IsInstanceOfType(RChild))
				if (typeof(MultiNode).IsInstanceOfType(LChild))
				{
					MultiNode l, r;
					l = (MultiNode)LChild; r = (MultiNode)RChild;
					foreach (Node n in r.Children)
						if (l.Children.Any((x) => x.DirectEqualsTo(n)))
							return true;
				}

			return false;
		}

		public override Node Replace()
		{
			if (typeof(NumberNode).IsInstanceOfType(RChild))
				return new MultiNode(LChild, new NumberNode(new Number(1) / ((NumberNode)RChild).Value));

			if (typeof(UnknownNode).IsInstanceOfType(RChild))
				if (typeof(MultiNode).IsInstanceOfType(LChild))
				{
					MultiNode l = (MultiNode)LChild;
					UnknownNode r = (UnknownNode)RChild;
					l.Children.Remove(l.Children.First((x) => 
							typeof(UnknownNode).IsInstanceOfType(x) && x.DirectEqualsTo(r)));
					return l;
				}

			return null;
		}

		public override void Simplify()
		{
			if (LChild.CanReplace()) LChild = LChild.Replace();
			else if (RChild.CanReplace()) RChild = RChild.Replace();
			else
			{
				MultiNode l, r;
				l = (MultiNode)LChild; r = (MultiNode)RChild;
				List<Node> lc = new List<Node>(l.Children), rc = new List<Node>(r.Children);
				foreach (Node c in r.Children)
				{
					Node n = lc.FirstOrDefault((x) => c.GetType().IsInstanceOfType(x) && x.DirectEqualsTo(c));
					if (n != null) lc.Remove(n);
				}

				l.Children = lc; r.Children = rc;
			}
		}

		public override double NumericEval()
		{
			return LChild.NumericEval() / RChild.NumericEval();
		}

		public override string Print()
		{
			return "(" + LChild.Print() + ") / (" + RChild.Print() + ")";
		}
	}

	public class AbsoluteValueNode: UnaryNode
	{
		// Simplify Rules:
		// 1. Abs{(-5)} = (5)

		public AbsoluteValueNode(Node child)
		{
			Child = child;
		}

		public override bool CanReplace()
		{
			return typeof(NumberNode).IsInstanceOfType(Child);
		}

		public override bool CanSimplify()
		{
			return Child.CanReplace();
		}

		public override bool DirectEqualsTo(Node n)
		{
			return ((AbsoluteValueNode)n).Child.DirectEqualsTo(Child);
		}

		public override double NumericEval()
		{
			return Math.Abs(Child.NumericEval());
		}

		public override string Print()
		{
			return "| " + Child.Print() + " |";
		}

		public override Node Replace()
		{
			NumberNode n = (NumberNode)Child;
			if (n.Value < Number.Zero)
				return new NumberNode(Number.Zero - n.Value);
			else
				return n;
		}

		public override void Simplify()
		{
			if (Child.CanReplace()) Child = Child.Replace();
		}
	}

	public class PowerNode : BinaryNode
	{
		// Simplify Rule:
		// 1. Pow{[x], (2)} = Mult{[x], [x]}

		public PowerNode(Node l, Node r)
		{
			LChild = l; RChild = r;
		}

		public override bool CanReplace()
		{
			if (typeof(NumberNode).IsInstanceOfType(RChild))
				return ((NumberNode)RChild).Value.M == 1;
			return false;
		}

		public override bool CanSimplify()
		{
			return LChild.CanReplace() || RChild.CanReplace();
		}

		public override double NumericEval()
		{
			return Math.Pow(LChild.NumericEval(), RChild.NumericEval());
		}

		public override string Print()
		{
			return "(" + LChild.Print() + ") ^ (" + RChild.Print() + ")";
		}

		public override Node Replace() {
			NumberNode n = (NumberNode)RChild;

			if (n.Value.N == 0)
				return new NumberNode(Number.One);
			else
			{
				MultiNode result = new MultiNode();
				long x = Math.Abs(n.Value.N);
				for (long i = 0; i < x; i++)
					result.Children.Add(LChild);

				if (n.Value.N < 0)
					return new DivideNode(new NumberNode(Number.One), result);
				else
					return result;
			}
		}

		public override void Simplify()
		{
			if (LChild.CanReplace()) LChild = LChild.Replace();
			if (RChild.CanReplace()) RChild = RChild.Replace();
		}
	}
}
