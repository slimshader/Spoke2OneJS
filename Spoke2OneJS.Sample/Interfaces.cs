using System;
using Spoke;
using Spoke2OneJS;

namespace Sample
{
    // Fields typed as Spoke interfaces rather than concrete State<T>.
    public partial class Scoreboard
    {
        [OneJsBind]
        private ISignal<int> _score = State.Create(3);

        [OneJsBind(OneJsBindMode.TwoWay)]
        private IState<string> _title = State.Create("Round 1");

        public static void Run()
        {
            var board = new Scoreboard();
            board.OnScoreChanged += () => Console.WriteLine($"Score changed: {board.Score}");
            board.OnTitleChanged += () => Console.WriteLine($"Title changed: {board.Title}");
            ((IState<int>)board._score).Set(7);
            board.SetTitle("Round 2");
        }
    }
}
