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
		//  4. Sum{} = (0)
		//  5. Sum{[a], [b], (0)} = Sum{[a], [b]}

		public SumNode(params Node[] children)
		{
			Children = new List<Node>(children);
		}

		public SumNode(IEnumerable<Node> children)
		{
			Children = new List<Node>(children);
		}

		public override SimplifyResult Simplify()
		{
			bool success = false; int count = 0;
			bool important = false;
			Number numSum = new Number(0);
			List<Node> newChildren = new List<Node>();

			foreach (Node n in Children)
			{
				var res = n.Replace(out Node node);
				if (res.success)
				{
					newChildren.Add(node);
					success = true;
					important = important || res.important;
				} else if (typeof(SumNode).IsInstanceOfType(n))
				{
					newChildren.AddRange(((SumNode)n).Children);
					success = true;
				} else if (typeof(NumberNode).IsInstanceOfType(n))
				{
					numSum += ((NumberNode)n).Value;
					count++; if (count >= 2) { success = true; important = true; }
				} else
					newChildren.Add(n);
			}

			if (success)
			{
				Children = newChildren;
				if (numSum.N != 0) Children.Add(new NumberNode(numSum));
			}

			return new SimplifyResult(success, important);
		}
		
		public override SimplifyResult Replace(out Node node) {
			if (Children.Count == 1)
			{
				node = Children[0];
				return new SimplifyResult(true);
			} else if (Children.Count == 0)
			{
				node = new NumberNode(Number.Zero);
				return new SimplifyResult(true);
			} else
			{
				node = null;
				return new SimplifyResult(false);
			}
		}

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

		public override SimplifyResult Replace(out Node node)
		{
			if (Children.Count == 0)
			{
				node = new NumberNode(Number.One);
				return new SimplifyResult(true);
			}

			node = Children[0];
			for (int i = 1; i < Children.Count; i++)
				if (Sign[i]) node = new MultiNode(Children[i], node);
				else node = new DivideNode(node, Children[i]);
			return new SimplifyResult(true);
		}

		public override SimplifyResult Simplify()
		{
			return new SimplifyResult(false);
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
		//  5. Mult{} = (1)											(*)
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

		public override SimplifyResult Simplify()
		{
			bool success = false; int count = 0;
			bool important = false;
			Number numProduct = new Number(1);
			List<Node> newChildren = new List<Node>();

			foreach (Node n in Children)
			{
				var res = n.Replace(out Node node);
				if (res.success)
				{
					newChildren.Add(node);
					success = true;
					important = important || res.important;
				} else if (typeof(MultiNode).IsInstanceOfType(n))
				{
					newChildren.AddRange(((MultiNode)n).Children);
					success = true;
				} else if (typeof(NumberNode).IsInstanceOfType(n))
				{
					numProduct *= ((NumberNode)n).Value;
					count++; if (count >= 2) { success = true; important = true; }
				} else
					newChildren.Add(n);
			}
			if (success)
			{
				if (numProduct.Numeric() != 1) newChildren.Add(new NumberNode(numProduct));
				Children = newChildren;
			}
			return new SimplifyResult(success, important);
		}

		public override SimplifyResult Replace(out Node node)
		{
			if (Children.Count == 1)
			{
				node = Children[0];
				return new SimplifyResult(true);
			} else if (Children.Count == 0)
			{
				node = new NumberNode(Number.One);
				return new SimplifyResult(true);
			}

			foreach (Node n in Children)
				if (typeof(NumberNode).IsInstanceOfType(n) && ((NumberNode)n).Value.N == 0)
				{
					node = n; // zero
					return new SimplifyResult(true, true);
				} else if (typeof(SumNode).IsInstanceOfType(n))
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

					node = new SumNode(l);
					return new SimplifyResult(true, true);
				}

			node = null;
			return new SimplifyResult(false);
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

		public override SimplifyResult Replace(out Node node)
		{
			if (typeof(NumberNode).IsInstanceOfType(RChild))
			{
				node = new MultiNode(LChild, new NumberNode(new Number(1) / ((NumberNode)RChild).Value));
				return new SimplifyResult(true, true);
			}

			// FIXME: because of DirectEqualsTo is no longer availible, we have to set up some new
			// NodeHelper.DirectEquals(), and NodeHelper.FindCommonFactor(a, b, out c).
			/*
			if (typeof(UnknownNode).IsInstanceOfType(RChild))
				if (typeof(MultiNode).IsInstanceOfType(LChild))
				{
					MultiNode l = (MultiNode)LChild;
					UnknownNode r = (UnknownNode)RChild;
					l.Children.Remove(l.Children.First((x) => 
							typeof(UnknownNode).IsInstanceOfType(x) && x.DirectEqualsTo(r)));

					node = l;
					return true;
				}
			*/

			node = null;
			return new SimplifyResult(false);
		}

		public override SimplifyResult Simplify()
		{
			var res = LChild.Replace(out Node n1);
			if (res.success)
			{
				LChild = n1;
				return new SimplifyResult(true, res.important);
			}
			res = RChild.Replace(out Node n2);
			if (res.success)
			{
				RChild = n2;
				return new SimplifyResult(true, res.important);
			}

			/*
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
				return true;
			}
			*/
			return new SimplifyResult(false);
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

		public override double NumericEval()
		{
			return Math.Abs(Child.NumericEval());
		}

		public override string Print()
		{
			return "| " + Child.Print() + " |";
		}

		public override SimplifyResult Replace(out Node node)
		{
			if (typeof(NumberNode).IsInstanceOfType(Child))
			{
				NumberNode n = (NumberNode)Child;
				if (n.Value < Number.Zero)
					node = new NumberNode(Number.Zero - n.Value);
				else
					node = n;
				return new SimplifyResult(true, true);
			} else { node = null; return new SimplifyResult(false); }
		}

		public override SimplifyResult Simplify()
		{
			var res = Child.Replace(out Node node);
			if (res.success)
			{
				Child = node;
				return new SimplifyResult(true, res.important);
			}
			return new SimplifyResult(false);
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

		public override double NumericEval()
		{
			return Math.Pow(LChild.NumericEval(), RChild.NumericEval());
		}

		public override string Print()
		{
			return "(" + LChild.Print() + ") ^ (" + RChild.Print() + ")";
		}

		public override SimplifyResult Replace(out Node node) {
			if (typeof(NumberNode).IsInstanceOfType(RChild) 
				|| ((NumberNode)RChild).Value.M == 1)
			{
				NumberNode n = (NumberNode)RChild;

				if (n.Value.N == 0)
				{
					node = new NumberNode(Number.One);
					return new SimplifyResult(true);
				} else
				{
					MultiNode result = new MultiNode();
					long x = Math.Abs(n.Value.N);
					for (long i = 0; i < x; i++)
						result.Children.Add(LChild);

					if (n.Value.N < 0)
						node = new DivideNode(new NumberNode(Number.One), result);
					else
						node = result;
					return new SimplifyResult(true, true);
				}
			} else
			{
				node = null;
				return new SimplifyResult(false);
			}
		}

		public override SimplifyResult Simplify()
		{
			var res = LChild.Replace(out Node n1);
			if (res.success)
			{
				LChild = n1;
				return new SimplifyResult(true, res.important);
			}

			res = RChild.Replace(out Node n2);
			if (res.success)
			{
				RChild = n2;
				return new SimplifyResult(true, res.important);
			}

			return new SimplifyResult(false);
		}
	}
}
