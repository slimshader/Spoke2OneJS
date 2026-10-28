using System;
using System.Collections.Immutable;
using Spoke;
using Spoke2OneJS;

namespace Sample
{
    public partial class TicTacToe
    {
        [OneJsBind("History")]
        private State<ImmutableArray<string[]>> _history = State.Create(ImmutableArray.Create<string[]>().Add(new string[9]));

        [OneJsBind(OneJsBindMode.TwoWay)] // name derived from field: Step, with a setter
        private State<int> _step = State.Create(0);

        [OneJsBind]
        private Trigger _startButtonClicked = Trigger.Create();

        [OneJsBind("CellClicked")]
        private Trigger<int> _cellClicked = Trigger.Create<int>();

        public TicTacToe()
        {
            _startButtonClicked.Subscribe(() => Console.WriteLine("Start clicked"));
            _cellClicked.Subscribe((int i) => Console.WriteLine($"Cell {i} clicked"));
        }

        public void Play() { _step.Set(_step.Now + 1); _history.Set(_history.Now.Add(new string[9])); }

        public partial class Nested<T>
        {
            [OneJsBind("Value")]
            static State<T> s_value = State.Create<T>(default);
        }
    }

    static class Program
    {
        static void Main()
        {
            var game = new TicTacToe();
            game.OnHistoryChanged += () => Console.WriteLine($"History changed: {game.History.Length}");
            game.OnStepChanged += () => Console.WriteLine($"Step changed: {game.Step}");
            game.Play();
            game.Step = 10;
            Console.WriteLine($"Step after set: {game.Step}");
            game.StartButtonClicked();
            game.CellClicked(4);
            Console.WriteLine(TicTacToe.Nested<string>.Value ?? "<null>");
        }
    }
}
