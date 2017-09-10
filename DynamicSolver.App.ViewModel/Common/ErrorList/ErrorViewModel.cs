using System;
using System.Reactive.Linq;
using System.Text;
using ReactiveUI;

namespace DynamicSolver.App.ViewModel.Common.ErrorList
{
    public class ErrorViewModel : ReactiveObject
    {
        private string _source;
        private string _message;
        private ErrorLevel _level;

        public ErrorLevel Level
        {
            get { return _level; }
            set { this.RaiseAndSetIfChanged(ref _level, value); }
        }

        public string Source
        {
            get { return _source; }
            set { this.RaiseAndSetIfChanged(ref _source, value); }
        }

        public string Message
        {
            get { return _message; }
            set { this.RaiseAndSetIfChanged(ref _message, value); }
        }

        private readonly ObservableAsPropertyHelper<string> _fullMessage;
        public string FullMessage => _fullMessage.Value;

        public ErrorViewModel()
        {
            _fullMessage = this.WhenAnyValue(m => m.Level, m => m.Source, m => m.Message)
                .Select(GetFullMessage)
                .ToProperty(this, m => m.FullMessage);
        }

        private string GetFullMessage(Tuple<ErrorLevel, string, string> state)
        {
            var sb = new StringBuilder();
            sb.Append(state.Item1);
            if (!string.IsNullOrEmpty(state.Item2))
            {
                sb.Append(" at ").Append(state.Item2);
            }

            sb.Append(": ").Append(state.Item3);

            return sb.ToString();
        }
    }
}