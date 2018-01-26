using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MinesweeperSolverDemo.Lib.Objects;

namespace MinesweeperSolverDemo.Lib.Solver
{
    public class ModelCheckingSolver : SingleGameSolver
    {
        List<Panel> fringe;

        public override void Deliberate()
        {
            fringe = this.Board.Panels.Where(p =>
                (!p.IsRevealed && !p.IsFlagged && Board.GetNeighbors(p.X, p.Y).Any(q => q.IsRevealed))).ToList();

            for (int i = 0; i < fringe.Count; ++i)
            {
                var field = fringe[i];

                // Try to reveal a field, which cannot contain a mine
                field.beliefState = searchState.tryMine;
                if (!isThereAModel(i))
                {
                    Board.RevealPanel(field.X, field.Y);
                    return;
                }

                // Try to flag a field, which has to contain a mine
                field.beliefState = searchState.tryNotMine;
                if (!isThereAModel(i))
                {
                    Board.FlagPanel(field.X, field.Y);
                    return;
                }

                // Cannot decide anything => continue
                field.beliefState = searchState.notDecided;
            }

            // Pseudo-random move - field with lowest number of remaining mines around it
            var bestFree = fringe
                .SelectMany(f => Board.GetNeighbors(f.X, f.Y))
                .Where(IsFreeField)
                .OrderBy(p => GetRemainingMines(p) + Board.GetNeighbors(p.X, p.Y).Sum(GetRemainingMines))
                .FirstOrDefault();

            if (bestFree != null)
            {
                Board.RevealPanel(bestFree.X, bestFree.Y);
                return;
            }

            // Completely random move (fallback)
            base.Deliberate();
        }

        // Free field = Not flagged, not revealed and not in the decision process
        bool IsFreeField(Panel p) => !p.IsFlagged && !p.IsRevealed && p.beliefState == searchState.notDecided;

        // Flagged field = Flagged or mine under
        bool IsFlagged(Panel p) => p.IsFlagged || p.beliefState == searchState.tryMine;

        // Number on the field - currently flagged mines around it. 0 for unrevealed fields.
        private int GetRemainingMines(Panel p)
        {
            if (!p.IsRevealed) return 0;
            var surroundingFlags = Board.GetNeighbors(p.X, p.Y).Count(IsFlagged);
            return p.AdjacentMines - surroundingFlags;
        }

        private bool isThereAModel(int startPoint)
        {
            //return IsThereAModelSat();
            return isThereAModelRecur(startPoint + 1, startPoint);
        }


        private bool IsThereAModelSat()
        {
            List<string> clauses = new List<string>();

            var beforeFringe = Board.Panels
                .Where(p => p.IsRevealed && Board.GetNeighbors(p.X, p.Y).Any(IsFreeField));

            foreach (var field in beforeFringe)
            {
                // Each field is represented as concatenates [X,Y] coordinates
                var freeNeighboursVariables = Board.GetNeighbors(field.X, field.Y)
                    .Where(IsFreeField)
                    .Select(f => (f.X * Board.Width) + (f.Y))
                    .ToList();

                // Counts of remaining fields and mines
                int n = freeNeighboursVariables.Count;
                int k = GetRemainingMines(field);

                // No mines left => no clauses for this field
                if (k == 0) continue;

                // Wrong mine counts => not satisfiable
                if (k < 0 || k > n) return false;

                // #mines == #fields => one clause for each variable
                if (k == n)
                {
                    foreach (var variable in freeNeighboursVariables)
                    {
                        var current = GetCnfClause(new[] {variable}, negated: false);
                        clauses.Add(current);
                    }
                }

                // # mines < # fields => add following combinations of mines:
                if (k < n)
                {
                    // positive clauses (at least one of n-k+1 is a mine)
                    var positiveCombinations = NChooseK(freeNeighboursVariables, n - k + 1);
                    foreach (var combination in positiveCombinations)
                    {
                        var current = GetCnfClause(combination, negated: false);
                        clauses.Add(current);
                    }

                    // negative clauses (at least one of k+1 is not a mine)
                    var negativeCombinations = NChooseK(freeNeighboursVariables, k + 1);
                    foreach (var combination in negativeCombinations)
                    {
                        var current = GetCnfClause(combination, negated: true);
                        clauses.Add(current);
                    }
                }
            }

            // Create & Solve CNF
            return SolveClauses(GetCnfFormula(clauses));
        }

        private bool SolveClauses(string cnf)
        {
            // Get cnf & flush into file
            var writer = new StreamWriter("problem.txt", append: false);
            writer.Write(cnf);
            writer.Close();

            // Solve
            StreamReader outputReader = null;
            var processInfo = new ProcessStartInfo("java.exe", "-jar org.sat4j.core.jar problem.txt")
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true
            };

            using (var solver = Process.Start(processInfo))
                try
                {
                    // Solver run
                    solver.WaitForExit();
                    outputReader = solver.StandardOutput;

                    // Output processing
                    string line;
                    while ((line = outputReader.ReadLine()) != null)
                        if (line.Contains("s SATISFIABLE"))
                            return true;

                    // Any other output is represented as not satisfiable
                    return false;
                }
                catch
                {
                    Console.WriteLine(@"Solver error !");
                    return false;
                }
        }

        private string GetCnfFormula(List<string> clauses)
        {
            var sb = new StringBuilder();

            // header line => p cnf (variable_count) (clause_count)
            sb.AppendLine($"p cnf {(Board.Width + 1) * (Board.Height + 1)} {clauses.Count}");

            // clauses
            foreach (var clause in clauses)
                sb.AppendLine(clause);

            return sb.ToString();
        }

        private string GetCnfClause<T>(IEnumerable<T> variables, bool negated = false)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var variable in variables)
                sb.Append($"{(negated ? "-" : "")}{variable.ToString()} ");

            sb.Append("0");

            return sb.ToString();
        }

        private IEnumerable<IEnumerable<T>> NChooseK<T>(IEnumerable<T> list, int k) where T : IComparable
        {
            if (k == 1) return list.Select(t => new T[] {t});
            return NChooseK(list, k - 1)
                .SelectMany(t => list.Where(o => o.CompareTo(t.Last()) > 0),
                    (t1, t2) => t1.Concat(new T[] {t2}));
        }

        private bool isThereAModelRecur(int i, int startPoint)
        {
            if (i == startPoint)
                return true;

            if (i >= fringe.Count)
                return isThereAModelRecur(0, startPoint);
            bool isModelFound;
            fringe[i].beliefState = searchState.tryMine;
            if (isCorrect(fringe[i]))
            {
                isModelFound = isThereAModelRecur(i + 1, startPoint);
                if (isModelFound)
                {
                    fringe[i].beliefState = searchState.notDecided;
                    return true;
                }
            }

            fringe[i].beliefState = searchState.tryNotMine;
            if (isCorrect(fringe[i]))
            {
                isModelFound = isThereAModelRecur(i + 1, startPoint);
                if (isModelFound)
                {
                    fringe[i].beliefState = searchState.notDecided;
                    return true;
                }
            }

            fringe[i].beliefState = searchState.notDecided;
            return false;
        }

        private bool isCorrect(Panel p)
        {
            var neighbours = Board.GetNeighbors(p.X, p.Y);
            foreach (var item in neighbours)
            {
                if (!item.IsRevealed || item.IsFlagged)
                    continue;
                var minPossibleMinesAround = Board.GetNeighbors(item.X, item.Y)
                    .Where(t => t.IsFlagged || t.beliefState == searchState.tryMine).Count();
                var maxPossibleMinesAround = Board.GetNeighbors(item.X, item.Y).Where(t =>
                    t.IsFlagged || (!t.IsRevealed && t.beliefState != searchState.tryNotMine)).Count();
                if ((minPossibleMinesAround > item.AdjacentMines) || (maxPossibleMinesAround < item.AdjacentMines))
                    return false;
            }

            return true;
        }

        public ModelCheckingSolver(Random rand, Visualizer vis, GameBoard board)
            : base(rand, vis, board) { }

        public ModelCheckingSolver(Random rand, Visualizer vis)
            : base(rand, vis) { }
    }
}