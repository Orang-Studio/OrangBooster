using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System.ComponentModel;
using Windows.UI;

namespace OrangBooster
{
    public enum CardTag { Safe, Unsafe, Privacy, Customizable }

    public class AppItem : INotifyPropertyChanged
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Category { get; set; } = "";
        public string Description { get; set; } = "";
        public string Choco { get; set; } = "";
        public string Winget { get; set; } = "";
        public string Link { get; set; } = "";
        public bool Foss { get; set; }
        private bool _selected;
        public bool Selected
        {
            get => _selected;
            set { if (_selected == value) return; _selected = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Selected))); }
        }
        public Visibility FossVisibility => Foss ? Visibility.Visible : Visibility.Collapsed;
        public Visibility ProprietaryVisibility => Foss ? Visibility.Collapsed : Visibility.Visible;
        public event PropertyChangedEventHandler? PropertyChanged;
    }

    public class BoosterCard : INotifyPropertyChanged
    {
        public string Title { get; set; } = "";
        private string? _key;
        public string Key
        {
            get => string.IsNullOrEmpty(_key) ? Slugify(Title) : _key!;
            set => _key = value;
        }
        public static string Slugify(string s)
        {
            var sb = new System.Text.StringBuilder();
            bool prevDash = false;
            foreach (char c in (s ?? "").ToLowerInvariant())
            {
                if (char.IsLetterOrDigit(c)) { sb.Append(c); prevDash = false; }
                else if (sb.Length > 0 && !prevDash) { sb.Append('-'); prevDash = true; }
            }
            return sb.ToString().Trim('-');
        }
        public string Description { get; set; } = "";
        public string Functions { get; set; } = "";
        public CardTag Tag { get; set; } = CardTag.Safe;
        public bool Recommended { get; set; }
        public string[] WinUtilTweaks { get; set; } = System.Array.Empty<string>();
        public string[] Win11DebloatArgs { get; set; } = System.Array.Empty<string>();
        public string[] EmbeddedActions { get; set; } = System.Array.Empty<string>();
        private bool _enabled;
        public bool Enabled
        {
            get => _enabled;
            set { if (_enabled == value) return; _enabled = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Enabled))); }
        }
        private bool _running;
        public bool Running
        {
            get => _running;
            set
            {
                if (_running == value) return;
                _running = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Running)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RunningVisibility)));
            }
        }
        public Visibility RunningVisibility => _running ? Visibility.Visible : Visibility.Collapsed;
        public string TagGlyph => Tag switch
        {
            CardTag.Privacy => "",
            CardTag.Unsafe => "",
            CardTag.Customizable => "",
            _ => "",
        };
        public string TagLabel => Tag switch
        {
            CardTag.Privacy => "Privacy",
            CardTag.Unsafe => "Unsafe",
            CardTag.Customizable => "Customizable",
            _ => "Safe",
        };
        public Brush TagBrush => new SolidColorBrush(Tag switch
        {
            CardTag.Privacy => Color.FromArgb(255, 0xAB, 0x47, 0xBC),
            CardTag.Unsafe => Color.FromArgb(255, 0xFF, 0x88, 0x00),
            CardTag.Customizable => Color.FromArgb(255, 0x5B, 0x9B, 0xD5),
            _ => Color.FromArgb(255, 0x43, 0xA0, 0x47),
        });
        public event PropertyChangedEventHandler? PropertyChanged;
}   }