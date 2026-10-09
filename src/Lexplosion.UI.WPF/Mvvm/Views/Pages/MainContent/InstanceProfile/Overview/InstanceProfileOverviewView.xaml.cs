using Lexplosion.UI.WPF.Mvvm.ViewModels.MainContent.InstanceProfile;
using Lexplosion.UI.WPF.Services.Descriptions;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Lexplosion.UI.WPF.Mvvm.Views.Pages.MainContent.InstanceProfile
{
	/// <summary>
	/// Instance descriptions are browser content; NewsHub and the existing MarkdownWPF
	/// pipeline are deliberately not changed. The ViewModel contains no WebView2 code.
	/// </summary>
	public partial class InstanceProfileOverviewView : UserControl
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

		public InstanceProfileOverviewView()
		{
			InitializeComponent();
			Loaded += OnLoaded;
			Unloaded += OnUnloaded;
			DataContextChanged += OnDataContextChanged;
			OverviewDiagnostics.Info("NavigationFullDataFix: view constructed");
		}

		private void SetStatus(string status)
		{
			BrowserStatus.Text = status;
			DescriptionLoader.Visibility = Visibility.Visible;
			// HwndHost paints above WPF controls even at a lower ZIndex.
			// Hiding only the browser (not its parent Grid) lets WebView2 finish
			// initializing/navigating without its blank native frame showing.
			if (_browser != null) _browser.Visibility = Visibility.Hidden;
			LoadingOverlay.Visibility = Visibility.Visible;
			_documentVisible = false;
			_revealScheduled = false;
			_documentNavigationStarted = false;
			_currentDocumentNavigationId = 0;
			OverviewDiagnostics.Info("Status: " + status);
		}

		private void ShowFailure(string message)
		{
			SetStatus(message);
			DescriptionLoader.Visibility = Visibility.Collapsed;
		}

		private void OnOverviewSizeChanged(object sender, SizeChangedEventArgs e)
		{
			UpdateInformationLayout();
			UpdateScrollbarInset();
		}

		// WPF's maximized non-client border can clip a Chromium scrollbar.
		// Move only the WebView surface inwards, never the entire content pane.
		private void UpdateScrollbarInset()
		{
			var window = Window.GetWindow(this);
			double inset = window != null && window.WindowState == WindowState.Maximized ? 10 : 0;
			BrowserHost.Margin = new Thickness(0, 0, inset, 0);
		}

		// Matches the HTML media query for its permanently visible sidebar.
		// In maximized windows, account for the WebView scrollbar safety inset.
		private bool HasInlineInformation()
		{
			var window = Window.GetWindow(this);
			double inset = window != null && window.WindowState == WindowState.Maximized ? 10 : 0;
			return OverviewRoot.ActualWidth > 0 && OverviewRoot.ActualWidth - inset >= 1120;
		}

		private void UpdateInformationLayout()
		{
			if (HasInlineInformation())
			{
				CloseInformationIfOpen();
				return;
			}
			if (InformationDrawer.Visibility != Visibility.Visible) return;
			// Keep the compact drawer inside the page and outside WebView2 airspace.
			double available = Math.Max(0, OverviewRoot.ActualWidth);
			double width = Math.Min(400, Math.Max(265, available * 0.36));
			if (available > 0) width = Math.Min(width, Math.Max(0, available - 240));
			DrawerColumn.Width = new GridLength(width, GridUnitType.Pixel);
		}

		public bool IsInformationDrawerOpen => InformationDrawer.Visibility == Visibility.Visible;

		public void ToggleInformation(FrameworkElement headerButton)
		{
			if (HasInlineInformation())
			{
				CloseInformationIfOpen();
				return;
			}
			if (InformationDrawer.Visibility == Visibility.Visible)
			{
				CloseInformationDrawer();
				return;
			}
			UpdateMetadata();
			InformationDrawer.Visibility = Visibility.Visible;
			UpdateInformationLayout();
			OverviewDiagnostics.Info("Information drawer opened");
		}

		public void CloseInformationIfOpen()
		{
			if (InformationDrawer.Visibility == Visibility.Visible)
				CloseInformationDrawer();
		}

		private void OnCloseInformationClick(object sender, RoutedEventArgs e)
		{
			CloseInformationDrawer();
		}

		private void CloseInformationDrawer()
		{
			InformationDrawer.Visibility = Visibility.Collapsed;
			DrawerColumn.Width = new GridLength(0, GridUnitType.Pixel);
			OverviewDiagnostics.Info("Information drawer closed");
		}

		private static object ReadProperty(object value, params string[] names)
		{
			if (value == null) return null;
			foreach (string name in names)
			{
				PropertyInfo p = value.GetType().GetProperty(name,
					BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
				if (p == null || p.GetIndexParameters().Length != 0) continue;
				try { return p.GetValue(value, null); } catch { /* metadata is optional */ }
			}
			return null;
		}

		private static string Label(object value)
		{
			if (value == null) return "—";
			if (value is DateTime dt) return dt.ToString("dd MMM yyyy", CultureInfo.CurrentCulture);
			if (value is DateTimeOffset offset) return offset.ToString("dd MMM yyyy", CultureInfo.CurrentCulture);
			if (value is string text)
			{
				if (DateTimeOffset.TryParse(text, CultureInfo.InvariantCulture,
					DateTimeStyles.AssumeUniversal, out var parsed))
					return parsed.ToString("dd MMM yyyy", CultureInfo.CurrentCulture);
				return string.IsNullOrWhiteSpace(text) ? "—" : text;
			}
			return value.ToString();
		}

		private static string Categories(object value)
		{
			if (value is string single) return Label(single);
			if (!(value is IEnumerable enumerable)) return "—";
			var tags = enumerable.Cast<object>().Take(16)
				.Select(x => Label(ReadProperty(x, "Name", "Title", "Slug") ?? x))
				.Where(x => !string.IsNullOrWhiteSpace(x) && x != "—");
			string rendered = string.Join(" • ", tags);
			return string.IsNullOrWhiteSpace(rendered) ? "—" : rendered;
		}

		private void UpdateMetadata()
		{
			if (_model == null) return;
			object full = _model.AdditionalData;
			object page = _model.InstanceData;
			object baseData = _model.BaseInstanceData;
			object instance = _model.InstanceModel;
			string author = Label(ReadProperty(full, "Author", "AuthorName", "Creator", "Owner", "Developer") ??
				ReadProperty(page, "Author", "AuthorName", "Creator", "Owner", "Developer") ??
				ReadProperty(baseData, "Author", "AuthorName", "Creator", "Owner", "Developer") ??
				ReadProperty(instance, "Author", "AuthorName", "Creator", "Owner", "Developer"));
			string updated = Label(ReadProperty(full, "LastUpdate", "DateModified", "Updated") ??
				ReadProperty(page, "LastUpdate", "DateModified", "Updated"));
			string categories = Categories(ReadProperty(full, "Categories", "Tags") ??
				ReadProperty(page, "Categories", "Tags") ??
				ReadProperty(baseData, "Categories", "Tags") ??
				ReadProperty(instance, "Categories", "Tags"));
			DrawerAuthorText.Text = author;
			DrawerUpdatedText.Text = updated;
			DrawerCategories.ItemsSource = categories == "—"
				? new[] { "—" }
				: categories.Split(new[] { " • " }, StringSplitOptions.RemoveEmptyEntries);
		}

		private InstanceDescriptionMetadata CreateMetadata()
		{
			UpdateMetadata();
			var tags = DrawerCategories.ItemsSource as IEnumerable;
			return new InstanceDescriptionMetadata
			{
				Author = DrawerAuthorText.Text,
				Updated = DrawerUpdatedText.Text,
				Categories = tags?.Cast<object>().Select(x => x?.ToString() ?? string.Empty)
					.Where(x => x != "—").ToArray() ?? Array.Empty<string>()
			};
		}

		private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			OverviewDiagnostics.Info("DataContextChanged; type=" + (e.NewValue?.GetType().FullName ?? "null"));
			AttachModel();
			RequestRender();
		}

		private void AttachModel()
		{
			if (_model != null)
				_model.PropertyChanged -= OnModelPropertyChanged;

			var vm = DataContext as InstanceProfileOverviewViewModel;
			_model = vm?.Model ?? DataContext as InstanceProfileOverviewModel;
			_lastDescription = null;
			_renderPending = false;
			++_renderVersion;
			// Do not show the previous instance's page while the new one loads.
			SetStatus("Загрузка полного описания…");

			if (_model != null)
			{
				_model.PropertyChanged += OnModelPropertyChanged;
				UpdateMetadata();
				OverviewDiagnostics.Info("Model attached: source=" + _model.DescriptionSource +
					", ready=" + _model.IsDescriptionReady +
					", length=" + (_model.OverviewDescription?.Length ?? 0));
			}
			else
			{
				OverviewDiagnostics.Warn("Overview Model missing. Check DataContext binding.");
			}
		}

		private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName != nameof(InstanceProfileOverviewModel.OverviewDescription) &&
				e.PropertyName != nameof(InstanceProfileOverviewModel.IsDescriptionReady))
				return;
			if (!Dispatcher.CheckAccess())
			{
				Dispatcher.BeginInvoke(new Action(() => { UpdateMetadata(); RequestRender(); }));
				return;
			}
			UpdateMetadata();
			RequestRender();
		}

		private async void OnLoaded(object sender, RoutedEventArgs e)
		{
			OverviewDiagnostics.Info("View Loaded; size=" + ActualWidth + "x" + ActualHeight);
			AttachModel();
			UpdateScrollbarInset();
			SetStatus("Инициализация WebView2…");
			await EnsureBrowserAsync();
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

		private void OnUnloaded(object sender, RoutedEventArgs e)
		{
			OverviewDiagnostics.Info("View Unloaded");
			++_generation;
			++_renderVersion;
			_initializing = false;
			_renderPending = false;
			CloseInformationDrawer();
			LoadingOverlay.Visibility = Visibility.Collapsed;
			if (_model != null)
			{
				_model.PropertyChanged -= OnModelPropertyChanged;
				_model = null;
			}
			ClearBrowser();
		}
	}
}
