using Lexplosion.UI.WPF.Mvvm.ViewModels.MainContent.InstanceProfile;
using Lexplosion.UI.WPF.Services.Descriptions;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Lexplosion.UI.WPF.Mvvm.Views.Pages.MainContent.InstanceProfile
{
    public partial class InstanceProfileOverviewView
    {
		private const string VirtualHost = "lexplosion-overview.appassets.example";
		private static readonly string HtmlFolder = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
			"Lexplosion", "Cache", "OverviewWebView2");
		private string _currentHtmlPath;
		private WebView2 _browser;
		private InstanceProfileOverviewModel _model;
		private bool _browserReady;
		private bool _initializing;
		private bool _renderPending;
		private int _generation;
		private int _renderVersion;
		// The browser can complete a superseded about:blank navigation with OperationCanceled.
		// Only the latest internal (NavigateToString) navigation may update the UI.
		private ulong _currentDocumentNavigationId;
		private bool _documentNavigationStarted;
		private bool _documentVisible;
		private bool _revealScheduled;
		private string _lastDescription;
		private InstanceSource _lastSource;

        private InstanceDescriptionMetadata CreateMetadata()
        {
            return new InstanceDescriptionMetadata
            {
                Author = _model.Author,
                Updated = _model.Updated,
                Categories = _model.Categories.Where(category => category != "—").ToArray()
            };
        }

		private Color GetBackgroundColor()
		{
			if (TryFindResource("PageSolidColorBrush") is SolidColorBrush b)
				return b.Color;
			if (Background is SolidColorBrush self)
				return self.Color;
			return Color.FromRgb(11, 16, 32);
		}

		private Color ThemeColor(string key, Color fallback)
		{
			return TryFindResource(key) is SolidColorBrush brush ? brush.Color : fallback;
		}

		private static string Hex(Color c) => $"#{c.R:X2}{c.G:X2}{c.B:X2}";

		// Read real Lexplosion theme brushes on the WPF UI thread. WebView2 HTML is
		// generated off-thread using plain strings (no WPF types in the builder).
		private InstanceDescriptionTheme CaptureDescriptionTheme()
		{
			Color background = GetBackgroundColor();
			bool light = background.R * 0.2126 + background.G * 0.7152 + background.B * 0.0722 > 155;
			return new InstanceDescriptionTheme
			{
				Background = Hex(background),
				Foreground = Hex(ThemeColor("PrimaryForegroundSolidColorBrush",
					light ? Colors.Black : Color.FromRgb(232, 235, 242))),
				Muted = Hex(ThemeColor("SecondaryForegroundSolidColorBrush",
					light ? Color.FromRgb(90, 99, 111) : Color.FromRgb(169, 183, 202))),
				Accent = Hex(ThemeColor("ActivitySolidColorBrush", Color.FromRgb(25, 135, 255))),
				Surface = Hex(ThemeColor("LayoutTabSolidColorBrush",
					light ? Color.FromRgb(247, 248, 250) : Color.FromRgb(16, 25, 42))),
				Border = Hex(ThemeColor("LayoutTabBorderSolidColorBrush",
					light ? Color.FromRgb(216, 221, 229) : Color.FromRgb(36, 49, 73)))
			};
		}

		private async Task EnsureBrowserAsync()
		{
			if (_browserReady)
			{
				RequestRender();
				return;
			}
			if (_initializing) return;
			_initializing = true;
			int generation = ++_generation;

			try
			{
				// Use the WebView2 default environment first: a previous version of this
				// page successfully used WebView2, so avoid an unnecessary custom profile.
				Color background = GetBackgroundColor();
				var browser = new WebView2
				{
					// Keep only the HWND hidden during initialization/navigation.
					// BrowserHost stays measured and attached to the visual tree.
					Visibility = Visibility.Hidden,
					HorizontalAlignment = HorizontalAlignment.Stretch,
					VerticalAlignment = VerticalAlignment.Stretch,
					DefaultBackgroundColor = System.Drawing.Color.FromArgb(
						255, background.R, background.G, background.B)
				};
				_browser = browser;
				BrowserHost.Children.Add(browser);
				OverviewDiagnostics.Info("Calling EnsureCoreWebView2Async(default environment)");
				await browser.EnsureCoreWebView2Async();
				if (generation != _generation || !IsLoaded || !ReferenceEquals(_browser, browser))
				{
					OverviewDiagnostics.Warn("Discarded completed initialization from a stale view");
					return;
				}
				OverviewDiagnostics.Info("WebView2 initialized, version=" +
					browser.CoreWebView2.Environment.BrowserVersionString);

				var settings = browser.CoreWebView2.Settings;
				settings.AreDefaultContextMenusEnabled = false;
				settings.AreDevToolsEnabled = Debugger.IsAttached;
				settings.AreHostObjectsAllowed = false;
				settings.AreDefaultScriptDialogsEnabled = false;
				settings.IsStatusBarEnabled = false;
				// Messages are accepted only from our virtual HTTPS origin,
				// and only for the Escape key. Third-party frames cannot control navigation.
				settings.IsWebMessageEnabled = true;
				// Only nonce-authorized first-party scripts run in the sanitized document.
				settings.IsScriptEnabled = true;

				browser.NavigationStarting += OnNavigationStarting;
				browser.NavigationCompleted += OnNavigationCompleted;
				// Show the page as soon as HTML/CSS is parsed. Do not wait for
				// remote images, trackers or YouTube iframes to finish loading.
				browser.CoreWebView2.DOMContentLoaded += OnDomContentLoaded;
				browser.CoreWebView2.WebMessageReceived += OnBrowserMessageReceived;
				browser.CoreWebView2.NewWindowRequested += OnNewWindowRequested;
				browser.CoreWebView2.ProcessFailed += OnProcessFailed;
				// Virtual HTTPS origin lets the embedded YouTube player receive a
				// document origin/Referer, unlike NavigateToString's opaque origin.
				Directory.CreateDirectory(HtmlFolder);
				browser.CoreWebView2.SetVirtualHostNameToFolderMapping(
					VirtualHost, HtmlFolder, CoreWebView2HostResourceAccessKind.DenyCors);
				_browserReady = true;
				SetStatus("Подготовка описания…");
				RequestRender();
			}
			catch (Exception exception)
			{
				OverviewDiagnostics.Error("WebView2 initialization", exception);
				if (generation == _generation)
				{
					ClearBrowser();
					ShowFailure("Ошибка WebView2: " + exception.Message +
							  "  •  Журнал: %LOCALAPPDATA%\\Lexplosion\\Logs\\instance-overview.log");
				}
			}
			finally
			{
				if (generation == _generation) _initializing = false;
			}
		}

		private void RequestRender()
		{
			if (!IsLoaded) return;
			if (_model == null)
			{
				ShowFailure("Описание не подключено: проверьте DataContext страницы Overview.");
				return;
			}
			if (!_model.IsDescriptionReady)
			{
				SetStatus("Загрузка полного описания…");
				return;
			}
			if (!_browserReady || _browser?.CoreWebView2 == null)
			{
				SetStatus("Ожидание WebView2…");
				return;
			}

			string description = _model.OverviewDescription ?? string.Empty;
			InstanceSource source = _model.DescriptionSource;
			if (_renderPending && string.Equals(_lastDescription, description, StringComparison.Ordinal) &&
				_lastSource == source) return;

			_renderPending = true;
			_lastDescription = description;
			_lastSource = source;
			int version = ++_renderVersion;
			SetStatus("Отображение описания…");
			OverviewDiagnostics.Info("Preparing HTML: source=" + source + ", textLength=" + description.Length);
			// Capture theme on the UI thread, not inside Task.Run.
			_ = RenderDescriptionAsync(description, source, version,
				CreateMetadata(), CaptureDescriptionTheme());
		}

		private async Task RenderDescriptionAsync(string description, InstanceSource source, int version,
			InstanceDescriptionMetadata metadata, InstanceDescriptionTheme theme)
		{
			try
			{
				string targetFile = "overview-" + Guid.NewGuid().ToString("N") + ".html";
				string path = Path.Combine(HtmlFolder, targetFile);
				string html = await Task.Run(() =>
				{
					string document = InstanceDescriptionHtmlBuilder.Build(description, source, theme.Background, metadata, theme);
					Directory.CreateDirectory(HtmlFolder);
					File.WriteAllText(path, document, new UTF8Encoding(false));
					return document;
				});
				if (!IsLoaded || version != _renderVersion || !_browserReady || _browser?.CoreWebView2 == null)
				{
					OverviewDiagnostics.Info("HTML dropped: stale render=" + version);
					TryDeleteFile(path);
					return;
				}
				OverviewDiagnostics.Info("VirtualHost navigate: htmlLength=" + html.Length +
					"; fullDataDescriptionLength=" + description.Length);
				string previousPath = _currentHtmlPath;
				_currentHtmlPath = path;
				// A real HTTPS origin is essential for YouTube iframe identity.
				_browser.CoreWebView2.Navigate("https://" + VirtualHost + "/" + targetFile);
				// The old file can be deleted after the new navigation is started.
				if (previousPath != null) TryDeleteFile(previousPath);
			}
			catch (Exception exception)
			{
				OverviewDiagnostics.Error("RenderDescriptionAsync", exception);
				if (version == _renderVersion)
					ShowFailure("Не удалось построить HTML: " + exception.Message);
			}
		}

		private void OnDomContentLoaded(object sender, CoreWebView2DOMContentLoadedEventArgs e)
		{
			OverviewDiagnostics.Info("DOMContentLoaded: id=" + e.NavigationId);
			RevealDocument(e.NavigationId, "DOMContentLoaded");
		}

		private void OnNavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
		{
			if (!ReferenceEquals(sender, _browser)) return;
			if (!_documentNavigationStarted ||
				_currentDocumentNavigationId == 0 ||
				e.NavigationId != _currentDocumentNavigationId) return;

			OverviewDiagnostics.Info("NavigationCompleted: id=" + e.NavigationId +
				", success=" + e.IsSuccess + ", error=" + e.WebErrorStatus);
			// The document may have been visible for seconds already: late network
			// resource failures should not hide valid content again.
			if (_documentVisible) return;
			if (!e.IsSuccess && e.WebErrorStatus == CoreWebView2WebErrorStatus.OperationCanceled)
				return;
			if (!e.IsSuccess)
			{
				ShowFailure("Ошибка навигации WebView2: " + e.WebErrorStatus);
				return;
			}
			// Safety fallback for runtimes on which DOMContentLoaded was missed.
			RevealDocument(e.NavigationId, "NavigationCompleted fallback");
		}

		private async void RevealDocument(ulong navigationId, string origin)
		{
			if (!IsLoaded || _browser == null || !_browserReady ||
				!_documentNavigationStarted || _documentVisible || _revealScheduled ||
				navigationId != _currentDocumentNavigationId) return;

			_revealScheduled = true;
			// WebView2 is an HWND. Revealing it during a WPF page/TabControl entrance
			// transform briefly draws the browser at stale native coordinates, looking
			// like it slides in from the right. Wait for the real host bounds to settle;
			// we never animate the native window into its place.
			try
			{
				Rect previous = Rect.Empty;
				int stableFrames = 0;
				for (int frame = 0; frame < 50 && stableFrames < 4; frame++)
				{
					await Task.Delay(16);
					if (!IsLoaded || !_browserReady || !_documentNavigationStarted ||
						navigationId != _currentDocumentNavigationId) return;
					var window = Window.GetWindow(this);
					if (window == null || BrowserHost.ActualWidth <= 0 || BrowserHost.ActualHeight <= 0)
						continue;
					Rect bounds;
					try
					{
						bounds = BrowserHost.TransformToAncestor(window).TransformBounds(
							new Rect(0, 0, BrowserHost.ActualWidth, BrowserHost.ActualHeight));
					}
					catch (InvalidOperationException) { continue; }
					if (!previous.IsEmpty &&
						Math.Abs(bounds.Left - previous.Left) < 0.5 &&
						Math.Abs(bounds.Top - previous.Top) < 0.5 &&
						Math.Abs(bounds.Width - previous.Width) < 0.5 &&
						Math.Abs(bounds.Height - previous.Height) < 0.5)
						stableFrames++;
					else stableFrames = 0;
					previous = bounds;
				}
				if (!IsLoaded || !_browserReady || _documentVisible ||
					!_documentNavigationStarted || navigationId != _currentDocumentNavigationId)
					return;

				await Dispatcher.InvokeAsync(() => BrowserHost.UpdateLayout(), DispatcherPriority.Loaded);
				if (!IsLoaded || !_browserReady ||
					navigationId != _currentDocumentNavigationId) return;
				_browser.Visibility = Visibility.Visible;
				LoadingOverlay.Visibility = Visibility.Collapsed;
				DescriptionLoader.Visibility = Visibility.Collapsed;
				_documentVisible = true;
				OverviewDiagnostics.Info("Overview displayed in stable position at " + origin +
					", navigation=" + navigationId);
			}
			finally
			{
				_revealScheduled = false;
			}
		}

		private void OnNavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
		{
			string raw = e.Uri ?? string.Empty;
			OverviewDiagnostics.Info("NavigationStarting: id=" + e.NavigationId +
				", scheme=" + (Uri.TryCreate(raw, UriKind.Absolute, out var parsed)
					? parsed.Scheme : "empty") + ", length=" + raw.Length);

			// NavigateToString may initiate an internal about:blank OR data: document.
			// Do NOT cancel it: doing so produces WebErrorStatus.OperationCanceled.
			if (raw.StartsWith("https://" + VirtualHost + "/", StringComparison.OrdinalIgnoreCase) ||
				string.IsNullOrEmpty(raw) ||
				raw.StartsWith("about:blank", StringComparison.OrdinalIgnoreCase) ||
				raw.StartsWith("data:text/html", StringComparison.OrdinalIgnoreCase))
			{
				if (raw.StartsWith("https://" + VirtualHost + "/", StringComparison.OrdinalIgnoreCase))
				{
					_currentDocumentNavigationId = e.NavigationId;
					_documentNavigationStarted = true;
				}
				return;
			}

			// Only our HTML document is rendered in WebView2. Any top-level external
			// navigation is canceled. HTTPS links open in the user's browser.
			e.Cancel = true;
			OverviewDiagnostics.Info("External top-level navigation intercepted; id=" +
				e.NavigationId);
			OpenExternal(raw);
		}

		private void OnBrowserMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
		{
			var trustedOrigin = "https://" + VirtualHost + "/";
			if (e.Source == null || !e.Source.StartsWith(trustedOrigin, StringComparison.OrdinalIgnoreCase))
				return;
			string message;
			try { message = e.TryGetWebMessageAsString(); }
			catch { return; }
			if (message != "lexplosion-overview-escape") return;

			// Chromium is a separate HWND: WPF's normal keyboard tunneling does not
			// see Escape after the user clicks on the description. Forward the gesture
			// to the parent Window's existing Escape handler, without owning routing.
			var window = Window.GetWindow(this);
			var source = window == null ? null : PresentationSource.FromVisual(window);
			if (source == null) return;
			var preview = new KeyEventArgs(Keyboard.PrimaryDevice, source, Environment.TickCount, Key.Escape)
			{
				RoutedEvent = Keyboard.PreviewKeyDownEvent
			};
			window.RaiseEvent(preview);
			if (!preview.Handled)
			{
				var keyDown = new KeyEventArgs(Keyboard.PrimaryDevice, source, Environment.TickCount, Key.Escape)
				{
					RoutedEvent = Keyboard.KeyDownEvent
				};
				window.RaiseEvent(keyDown);
			}
		}

		private void OnNewWindowRequested(object sender, CoreWebView2NewWindowRequestedEventArgs e)
		{
			e.Handled = true;
			OpenExternal(e.Uri);
		}

		private void OnProcessFailed(object sender, CoreWebView2ProcessFailedEventArgs e)
		{
			OverviewDiagnostics.Warn("WebView2 process failed: " + e.ProcessFailedKind);
			ShowFailure("Процесс WebView2 аварийно завершился: " + e.ProcessFailedKind);
		}

		private static void OpenExternal(string raw)
		{
			if (!Uri.TryCreate(raw, UriKind.Absolute, out var uri) ||
				uri.Scheme != Uri.UriSchemeHttps) return;
			try { Process.Start(new ProcessStartInfo(uri.AbsoluteUri) { UseShellExecute = true }); }
			catch (Exception e) { OverviewDiagnostics.Error("External link", e); }
		}

		private void ClearBrowser()
		{
			_browserReady = false;
			_currentDocumentNavigationId = 0;
			_documentNavigationStarted = false;
			_documentVisible = false;
			string fileToDelete = _currentHtmlPath;
			_currentHtmlPath = null;
			if (_browser == null)
			{
				TryDeleteFile(fileToDelete);
				return;
			}
			var browser = _browser;
			_browser = null;
			try
			{
				browser.NavigationStarting -= OnNavigationStarting;
				browser.NavigationCompleted -= OnNavigationCompleted;
				if (browser.CoreWebView2 != null)
				{
					browser.CoreWebView2.DOMContentLoaded -= OnDomContentLoaded;
					browser.CoreWebView2.WebMessageReceived -= OnBrowserMessageReceived;
					browser.CoreWebView2.NewWindowRequested -= OnNewWindowRequested;
					browser.CoreWebView2.ProcessFailed -= OnProcessFailed;
				}
				BrowserHost.Children.Remove(browser);
				browser.Dispose();
			}
			catch (Exception e)
			{
				OverviewDiagnostics.Error("WebView2 cleanup", e);
			}
			TryDeleteFile(fileToDelete);
		}

		private static void TryDeleteFile(string path)
		{
			if (string.IsNullOrEmpty(path)) return;
			try { if (File.Exists(path)) File.Delete(path); }
			catch (IOException) { /* browser may still have the file open */ }
			catch (UnauthorizedAccessException) { /* cache cleanup is best-effort */ }
		}
    }
}
