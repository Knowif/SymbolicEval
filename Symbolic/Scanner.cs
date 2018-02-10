using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Symbolic
{
	public enum TokenType
	{
		TokenNumber,
		TokenPlus, TokenMinus,
		TokenMult, TokenDivide,
		TokenLParen, TokenRParen,
		TokenPower, TokenTube,
		TokenIdentifer,
		TokenEnd,
		TokenError
	}

	public class Scanner
	{
		string input;
		int pos = 0;

		TokenType token;
		double number;
		string symbol;

		public TokenType Token { get { return token; } }
		public double Number { get { return number; } }
		public string Symbol { get { return symbol; } }

		public Scanner(string input)
		{
			this.input = input;
			Accept();
		}

		public void Accept()
		{
			EatWhite();
			switch (CurrentChar())
			{
				case '+':
					token = TokenType.TokenPlus;
					pos++;
					break;
				case '-':
					token = TokenType.TokenMinus;
					pos++;
					break;
				case '*':
					token = TokenType.TokenMult;
					pos++;
					break;
				case '/':
					token = TokenType.TokenDivide;
					pos++;
					break;
				case '(':
					token = TokenType.TokenLParen;
					pos++;
					break;
				case ')':
					token = TokenType.TokenRParen;
					pos++;
					break;
				case '^':
					token = TokenType.TokenPower;
					pos++;
					break;
				case '|':
					token = TokenType.TokenTube;
					pos++;
					break;
				case '0': case '1': case '2': case '3': case '4':
				case '5': case '6': case '7': case '8': case '9':
				case '.':
					token = TokenType.TokenNumber;
					number = 0; int depth = 0;
					while (char.IsDigit(CurrentChar()) || CurrentChar() == '.')
					{
						if (CurrentChar() == '.')
							depth = 1;
						else if (depth == 0)
							number = number * 10 + Char2Int(CurrentChar());
						else
						{
							number += Char2Int(CurrentChar()) * (1 / Math.Pow(10, depth));
							depth++;
						}

						pos++;
					}
					break;
				case '\0':
					token = TokenType.TokenEnd;
					break;
				default:
					if (char.IsLetter(CurrentChar()))
					{
						token = TokenType.TokenIdentifer;
						symbol = "";
						while (char.IsLetter(CurrentChar()))
						{
							symbol += CurrentChar();
							pos++;
						}
					} else token = TokenType.TokenError;
					break;
			}
		}

		private void EatWhite()
		{
			while (char.IsWhiteSpace(CurrentChar()))
				pos++;
		}

		private char CurrentChar()
		{
			if (pos >= input.Length) return '\0';
			else return input[pos];
		}

		private int Char2Int(char ch)
		{
			return Convert.ToInt32(ch) - Convert.ToInt32('0');
		}
	}
}
