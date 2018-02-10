using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Symbolic
{
	public class Parser
	{
		public enum Status { OK, Error }

		Status status = Status.OK;
		Scanner scanner;

		public Parser(Scanner _scan)
		{
			scanner = _scan;
		}

		public Tree Parse()
		{
			Tree t = new Tree(Expression());
			if (scanner.Token != TokenType.TokenEnd)
				Console.WriteLine("Warning: Not completely parsed");

			return t;
		}


		Node Expression()
		{
			// Expr := Term {('+' | '-') Term}

			Node result = Term();
			TokenType token = scanner.Token;
			if (token == TokenType.TokenPlus || token == TokenType.TokenMinus)
			{
				SumNode node = new SumNode(result);
				do
				{
					scanner.Accept();
					Node child = Term();
					if (token == TokenType.TokenMinus)
						child = new MultiNode(new NumberNode(new Number(-1)), child);
					node.Children.Add(child);
					token = scanner.Token;
				} while (token == TokenType.TokenPlus || token == TokenType.TokenMinus);

				result = node;
			}

			return result;
		}

		Node Term()
		{
			// Term := Factor {('*' | '/') Factor}

			Node result = Factor();
			TokenType token = scanner.Token;
			if (token == TokenType.TokenMult || token == TokenType.TokenDivide)
			{
				ProductNode node = new ProductNode(new Node[] { result }, new bool[] { true });
				do
				{
					scanner.Accept();
					Node child = Factor();
					node.Children.Add(child);
					node.Sign.Add(token == TokenType.TokenMult);
					token = scanner.Token;
				} while (token == TokenType.TokenMult || token == TokenType.TokenDivide);

				result = node;
			}

			return result;
		}

		Node Factor()
		{
			// Factor := SimpFactor '^' Factor
			//        or SimpFactor

			Node result = SimpleFactor();
			TokenType token = scanner.Token;
			if (token == TokenType.TokenPower)
			{
				scanner.Accept();
				return new PowerNode(result, Factor());
			}

			return result;
		}

		Node SimpleFactor()
		{
			// SimpFactor := '(' Expr ')'
			//            or '|' Expr '|'
			//            or Number
			//            or Identifier
			//            or '-' Factor

			Node result;
			TokenType token = scanner.Token;

			if (token == TokenType.TokenLParen)
			{
				scanner.Accept();
				result = Expression();
				if (scanner.Token == TokenType.TokenRParen)
					scanner.Accept();
				else
					status = Status.Error;
			} else if (token == TokenType.TokenTube)
			{
				scanner.Accept();
				result = new AbsoluteValueNode(Expression());
				if (scanner.Token == TokenType.TokenTube)
					scanner.Accept();
				else
					status = Status.Error;
			} else if (token == TokenType.TokenNumber)
			{
				result = new NumberNode(new Number(scanner.Number));
				scanner.Accept();
			} else if (token == TokenType.TokenIdentifer)
			{
				result = new UnknownNode(scanner.Symbol);
				scanner.Accept();
			} else if (token == TokenType.TokenMinus)
			{
				scanner.Accept();
				result = new MultiNode(new NumberNode(new Number(-1)), Factor());
			} else
			{
				status = Status.Error;
				result = null;
			}

			return result;
		}
	}
}
