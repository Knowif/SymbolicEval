using System;
using System.Collections.Generic;

namespace Symbolic
{
	public struct SimplifyResult
	{
		public readonly bool success;
		public readonly bool important;

		public SimplifyResult(bool success, bool important = false)
		{
			this.success = success;
			this.important = important;
		}
	}

	public abstract class Node
	{
		public abstract SimplifyResult Simplify();

		public abstract SimplifyResult Replace(out Node result);

		public abstract double NumericEval();

		public abstract string Print();

		public abstract IEnumerable<Node> GetChildren();
	}

	public class Tree
	{
		public Node Root;
		public int StepLimit = 1000;

		public Tree(Node top) { Root = top; }

		public List<String> Simplify()
		{
			List<String> steps = new List<string>();
			Queue<Node> queue = new Queue<Node>();
			Console.WriteLine(Root.Print());

			while (true)
			{
				var res = Root.Replace(out Node newRoot);
				if (res.success)
					Root = newRoot;
				if (res.important)
					steps.Add(Root.Print());

				if (!BFSSimplify(Root, queue, steps)) break;
				
				if (steps.Count >= StepLimit)
				{
					steps.Add("ERROR_STEP_LIMIT_REACHED");
					break;
				}
			}

			steps.Add(Root.Print());
			return steps;
		}

		public string Print() { return Root.Print(); }

		private bool BFSSimplify(Node n, Queue<Node> q, List<String> steps)
		{
			q.Clear();
			q.Enqueue(n);

			while (q.Count > 0)
			{
				Node x = q.Dequeue();
				var res = x.Simplify();
				if (res.success) return true;
				if (res.important) steps.Add(Root.Print());

				foreach (Node child in x.GetChildren())
					q.Enqueue(child);
			}

			return false;
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
	}

	public abstract class MultiChildNode : Node
	{
		public List<Node> Children;

		public override IEnumerable<Node> GetChildren() { return Children.ToArray(); }
	}

}
