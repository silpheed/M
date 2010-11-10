using System;
using System.Collections.Generic;
using System.Text;

namespace m
{
	public class SearchQuery : ISearchQuery
	{
		private readonly IList<string> wantedAtoms = new List<string>();
		private readonly IList<string> unwantedAtoms = new List<string>();
		
		public SearchQuery(string rawInput)
		{
			Parse(rawInput);
		}

		private void Parse(string input)
		{
			if (String.IsNullOrEmpty(input))
				return;
			
			string raw = input.Trim();

			if (String.IsNullOrEmpty(raw))
				return;
			
			StringBuilder currentAtom = new StringBuilder();
			bool inQuote = false;
			bool wanted = true;

			foreach (char c in raw)
			{
				if ((c == '-') && (!inQuote) && (wanted) && (currentAtom.Length == 0)) {
					wanted = false;
					continue;
				}
				if (c == '"') {
					if ((inQuote) && (currentAtom.Length > 0))
						AddToRelevantAtomListAndReset(ref currentAtom, ref wanted);
					inQuote = !inQuote;
					continue;
				}
				if ((inQuote) || (c != ' '))
					currentAtom.Append(c);
				else if (currentAtom.Length > 0)
					AddToRelevantAtomListAndReset(ref currentAtom, ref wanted);
				else if (!wanted)
					wanted = true;
			}
			if (currentAtom.Length > 0)
				AddToRelevantAtomListAndReset(ref currentAtom, ref wanted);
		}

		private void AddToRelevantAtomListAndReset(ref StringBuilder atom, ref bool wanted)
		{
			if (wanted)
				wantedAtoms.Add(atom.ToString());
			else
				unwantedAtoms.Add(atom.ToString());
			//reset both the atom and wanted flag
			atom.Length = 0;
			wanted = true;
		}

		public IList<string> WantedAtoms
		{
			get { return wantedAtoms; }
		}

		public IList<string> UnwantedAtoms
		{
			get { return unwantedAtoms; }
		}
		
	}
}