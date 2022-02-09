// --------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Xoc Software">
//     Copyright © 2021 Xoc Software
// </copyright>
// <summary>Implements Donald Knuth's algorithm for solving the mastermind game.</summary>
// --------------------------------------------------------------------------------------------------

namespace Mastermind
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Globalization;
	using System.Net.Http.Headers;
	using System.Runtime.CompilerServices;
	using System.Text;

	/// <summary>A program.</summary>
	public class Program
	{
		/// <summary>all.</summary>
		private static readonly List<int> All = new();

		/// <summary>The guess start.</summary>
		private static int guessStart;

		/// <summary>The root.</summary>
		private static Node root;

		/// <summary>The digits in code.</summary>
		private static int digitsInCode;

		/// <summary>Main entry-point for this application.</summary>
		public static void Main()
		{
			// Step 1
			CreateSet(4, 6, true);

			root = new Node(guessStart);

			foreach (int solution in All)
			{
				Solve(solution, new List<int>(All));
			}

			RecurseNode(root, (0, 0), 0);
		}

		/// <summary>
		/// Creates the set of possible codes. Sets guessStart and digitsInCode.
		/// </summary>
		/// <param name="digitsInCodeSet">The digits in code.</param>
		/// <param name="digitsPossible">The digits possible.</param>
		/// <param name="repeats">True to repeats.</param>
		private static void CreateSet(int digitsInCodeSet, int digitsPossible, bool repeats)
		{
			guessStart = repeats ? 1122 : 1234;
			digitsInCode = digitsInCodeSet;
			AddDigit(0, 0, digitsInCodeSet, digitsPossible, repeats);
			All.Sort();
		}

		/// <summary>Add digits to the existing code.</summary>
		/// <param name="code">The code built up so far.</param>
		/// <param name="position">The position of the digit being added.</param>
		/// <param name="digitsInCode">The digits in code.</param>
		/// <param name="digitsPossible">The digits possible.</param>
		/// <param name="repeats">Repeated digits possible.</param>
		private static void AddDigit(int code, int position, int digitsInCode, int digitsPossible, bool repeats)
		{
			if (position == digitsInCode)
			{
				All.Add(code);
			}
			else
			{
				for (int digit = 1; digit <= digitsPossible; digit++)
				{
					bool skip = false;
					if (!repeats)
					{
						char digitChar = (char)(48 + digit);
						string codeString = code.ToString();
						for (int i = 0; i < codeString.Length; i++)
						{
							if (digitChar == codeString[i])
							{
								skip = true;
							}
						}
					}

					if (!skip)
					{
						AddDigit((code * 10) + digit, position + 1, digitsInCode, digitsPossible, repeats);
					}
				}
			}
		}

		/// <summary>
		/// Evaluates a guess against a code. There are probably better ways of writing this.
		/// </summary>
		/// <param name="code">The code of the solution.</param>
		/// <param name="guess">The guess.</param>
		/// <returns>A Tuple with the B/W results.</returns>
		private static (int Black, int White) Evaluate(int code, int guess)
		{
			int black = 0;
			int white = 0;

			StringBuilder currentGuess = new(guess.ToString());
			StringBuilder currentSolution = new(code.ToString());
			for (int digit = 0; digit < digitsInCode; digit++)
			{
				if (currentGuess[digit] == currentSolution[digit])
				{
					black++;
					currentGuess[digit] = ' ';
					currentSolution[digit] = ' ';
				}
			}

			for (int digit = 0; digit < digitsInCode; digit++)
			{
				if (currentGuess[digit] != ' ')
				{
					for (int compare = 0; compare < digitsInCode; compare++)
					{
						if (currentGuess[digit] == currentSolution[compare])
						{
							white++;
							currentGuess[digit] = ' ';
							currentSolution[compare] = ' ';
							break;
						}
					}
				}
			}

			Debug.Assert(black + white <= digitsInCode, "Should never be true");
			return (black, white);
		}

		/// <summary>Evaluate row and gives the results.</summary>
		/// <param name="row">The guess number being worked on.</param>
		/// <param name="code">The code of the code.</param>
		/// <param name="guess">The guess.</param>
		/// <returns>A Tuple with the B/W result.</returns>
		private static (int Black, int White) EvaluateRow(int row, int code, int guess)
		{
			(int black, int white) = Evaluate(code, guess);
			Console.WriteLine("{0}: {1} : B{2} W{3}", row, guess, black, white);
			return (black, white);
		}

		/// <summary>Finds the max of the given arguments.</summary>
		/// <param name="s">The set of codes that are still possible.</param>
		/// <param name="guess">The guess.</param>
		/// <returns>The calculated maximum.</returns>
		private static int GetMax(List<int> s, int guess)
		{
			int[,] count = new int[digitsInCode + 1, digitsInCode + 1];
			foreach (int item in s)
			{
				(int black, int white) = Evaluate(item, guess);
				count[black, white]++;
			}

			int result = 0;
			for (int black = 0; black <= digitsInCode; black++)
			{
				for (int white = 0; white <= digitsInCode; white++)
				{
					if (count[black, white] > result)
					{
						result = count[black, white];
					}
				}
			}

			return result;
		}

		/// <summary>Recurse node to write code table.</summary>
		/// <param name="node">The node currently being worked on.</param>
		/// <param name="result">The result that lead to this node.</param>
		/// <param name="indent">How far to indent this.</param>
		private static void RecurseNode(Node node, (int Black, int White) result, int indent)
		{
			if (indent == 0)
			{
				Console.WriteLine("{0}", node.Guess);
			}
			else
			{
				Console.WriteLine("{0}B{1}W{2} {3}", new string('\t', indent), result.Black, result.White, node.Guess);
			}

			foreach (KeyValuePair<(int Black, int White), Node> keyValuePair in node.Nodes)
			{
				RecurseNode(keyValuePair.Value, keyValuePair.Key, indent + 1);
			}
		}

		/// <summary>Solves the code.</summary>
		/// <param name="code">The code to solve for.</param>
		/// <param name="s">The set of codes that are possible to solve for.</param>
		private static void Solve(int code, List<int> s)
		{
			int row = 1;
			List<int> unguessed = new(All);

			// Step 2
			int guess = guessStart;
			Console.WriteLine("Solution: {0}", code);
			Node node = root;
			while (true)
			{
				_ = unguessed.Remove(guess);

				// Step 3
				(int blackGuess, int whiteGuess) = EvaluateRow(row, code, guess);

				// Step 4
				if (blackGuess == digitsInCode)
				{
					break;
				}

				// Step 5
				List<int> sNew = new(s);
				_ = sNew.Remove(guess);
				foreach (int item in s)
				{
					(int black, int white) = Evaluate(guess, item);
					if (black != blackGuess || white != whiteGuess)
					{
						_ = sNew.Remove(item);
					}
				}

				s = sNew;

				// Step 6
				int min = int.MaxValue;
				foreach (int item in unguessed)
				{
					int max = GetMax(s, item);
					if (max <= min)
					{
						if (max == min)
						{
							if (!s.Contains(guess) && s.Contains(item))
							{
								guess = item;
								min = max;
							}
						}
						else
						{
							guess = item;
							min = max;
						}
					}
				}

				node = node.AddGuess((blackGuess, whiteGuess), guess);
				row++;
			}

			Console.WriteLine();
		}

		/// <summary>A node.</summary>
		private class Node
		{
			/// <summary>Initializes a new instance of the <see cref="Node" /> class.</summary>
			/// <param name="guess">The guess.</param>
			public Node(int guess)
			{
				this.Guess = guess;
			}

			/// <summary>Gets the guess.</summary>
			/// <value>The guess.</value>
			public int Guess { get; private set; }

			/// <summary>Gets the nodes.</summary>
			/// <value>The nodes.</value>
			public SortedDictionary<(int Black, int White), Node> Nodes { get; } = new(new ItemComparer());

			/// <summary>
			/// Adds the guess node to the list. If it is already there, just return the node.
			/// </summary>
			/// <param name="result">The result of the previous guess.</param>
			/// <param name="guess">The guess to be made.</param>
			/// <returns>A Node.</returns>
			public Node AddGuess((int Black, int White) result, int guess)
			{
				Node node;
				if (this.Nodes.ContainsKey(result))
				{
					node = this.Nodes[result];
				}
				else
				{
					node = new Node(guess);
					this.Nodes.Add(result, node);
				}

				return node;
			}

			/// <summary>Compares the two B/W results and sorts them.</summary>
			/// <seealso cref="T:System.Collections.Generic.IComparer{(System.Int32 Black, System.Int32 White)}"/>
			private class ItemComparer : IComparer<(int Black, int White)>
			{
				/// <summary>
				/// Compares two objects and returns a value indicating whether one is less than,
				/// equal to, or greater than the other.
				/// </summary>
				/// <param name="x">The first object to compare.</param>
				/// <param name="y">The second object to compare.</param>
				/// <returns>
				/// A signed integer that indicates the relative values of <paramref name="x" />
				/// and <paramref name="y" />.
				/// </returns>
				/// <seealso cref="M:System.Collections.Generic.IComparer{(System.Int32 Black, System.Int32 White)}.Compare((intBlack,intWhite),(intBlack,intWhite))"/>
				public int Compare((int Black, int White) x, (int Black, int White) y)
				{
					int result = x.Black.CompareTo(y.Black);
					if (result == 0)
					{
						result = x.White.CompareTo(y.White);
					}

					return -result;
				}
			}
		}
	}
}