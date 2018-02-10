using System;
using System.Collections.Generic;

namespace Symbolic
{
	public abstract class Node
	{
		public abstract bool CanSimplify();

		public abstract void Simplify();

		public abstract bool CanReplace();

		public abstract Node Replace();

		public abstract double NumericEval();

		public abstract string Print();

		// Assume the type of n is always same with the object
		public abstract bool DirectEqualsTo(Node n);

		public abstract IEnumerable<Node> GetChildren();
	}

	public class Tree
	{
		public Node Root;

		public Tree(Node top) { Root = top; }

		public void Simplify()
		{
			Node n;
			Console.WriteLine("\n" + Root.Print());
			while (true)
			{
				if (Root.CanReplace())
				{
					Root = Root.Replace();
					Console.WriteLine(Root.Print() + " -REP");
					continue;
				}

				n = BFS(Root);
				if (n != null)
					n.Simplify();
				else
					break;

				Console.WriteLine(Root.Print() + " -SIMP");
			}
		}

		public string Print() { return Root.Print(); }

		private Node BFS(Node n)
		{
			Queue<Node> q = new Queue<Node>();
			q.Enqueue(n);

			while (q.Count > 0)
			{
				Node x = q.Dequeue();
				if (x.CanSimplify()) return x;

				foreach (Node child in x.GetChildren())
					q.Enqueue(child);
			}

			return null;
		}
	}

	public abstract class UnaryNode : Node
	{
		public Node Child;

		public override IEnumerable<Node> GetChildren() { return new Node[] { Child }; }
	}

	public abstract class BinaryNode : Node
	{
		public Node LChild, RChild;

		public override IEnumerable<Node> GetChildren() { return new Node[] { LChild, RChild }; }

		public override bool DirectEqualsTo(Node n)
		{
			BinaryNode m = (BinaryNode)n;
			return m.LChild.DirectEqualsTo(LChild) && m.RChild.DirectEqualsTo(RChild);
		}
	}

	public abstract class MultiChildNode : Node
	{
		public List<Node> Children;

		public override bool DirectEqualsTo(Node n)
		{
			MultiChildNode m = (MultiChildNode)n;
			foreach (Node x in Children)
			{
				int countA = Children.FindAll((d) => d.DirectEqualsTo(x)).Count;
				int countB = m.Children.FindAll((d) => d.DirectEqualsTo(x)).Count;
				if (countA != countB) return false;
			}

			return true;
		}

		public bool ContainsNode(Node n)
		{
			foreach (Node c in Children)
				if (c.GetType().IsInstanceOfType(n))
					if (c.DirectEqualsTo(n))
						return true;

			return false;
		}

		public override IEnumerable<Node> GetChildren() { return Children.ToArray(); }
	}

}
