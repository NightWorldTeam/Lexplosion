using HtmlAgilityPack;
using Lexplosion.Logic.Management.Instances;
using Markdig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Text;

namespace Lexplosion.UI.WPF.Services.Descriptions
{
    // Browser-neutral metadata. The WPF View supplies these values; the ViewModel and Core
    // remain independent from WPF/WebView2. All values are HTML-encoded before use.
    internal sealed class InstanceDescriptionMetadata
    {
        public string Author { get; set; }
        public string Updated { get; set; }
        public string[] Categories { get; set; } = Array.Empty<string>();
    }

    // Plain color values supplied by the platform-specific View. No WPF dependency here.
    internal sealed class InstanceDescriptionTheme
    {
        public string Background { get; set; } = "#0B1020";
        public string Foreground { get; set; } = "#E8EBF2";
        public string Muted { get; set; } = "#A9B7CA";
        public string Accent { get; set; } = "#1987FF";
        public string Surface { get; set; } = "#10192A";
        public string Border { get; set; } = "#243149";
    }

    // No reference to WPF or WebView2: this transformer can later move to an Avalonia host.
    // Treat all external descriptions as untrusted HTML, including HTML embedded in Markdown.
    internal static class InstanceDescriptionHtmlBuilder
    {
        private static readonly MarkdownPipeline MarkdownPipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .Build();

        private static readonly HashSet<string> AllowedTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "p", "div", "span", "br", "hr", "h1", "h2", "h3", "h4", "h5", "h6",
            "strong", "b", "em", "i", "u", "s", "del", "blockquote", "pre", "code",
            "ul", "ol", "li", "table", "thead", "tbody", "tr", "th", "td",
            "img", "a", "details", "summary", "figure", "figcaption", "iframe", "center", "font"
        };

        private static readonly HashSet<string> RemoveWithContent = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "script", "style", "link", "meta", "object", "embed", "svg", "math", "form",
            "input", "button", "select", "textarea", "template", "base", "frame", "frameset"
        };

        public static string Build(string description, InstanceSource source, string backgroundHex = "#0b1020", InstanceDescriptionMetadata metadata = null, InstanceDescriptionTheme theme = null)
        {
            string input = description ?? string.Empty;
            bool html = source == InstanceSource.Curseforge && LooksLikeHtml(input);
            // Modrinth occasionally returns escaped Markdown image tokens. Normalize them
            // before Markdig so they become media rather than clickable raw ![](url) text.
            string fragment = html ? input : Markdown.ToHtml(RepairMarkdownImages(input), MarkdownPipeline);
            // Some providers mix raw HTML and Markdown, or turn Markdown image
            // syntax into text nodes with an auto-linked URL. Restore those
            // tokens after Markdig as well, then sanitize the result as usual.
            fragment = RepairRenderedMarkdownImages(fragment, source);
            string safeFragment = Sanitize(fragment, source);
            // Providers often insert many empty paragraphs and line breaks between
            // embedded media and buttons. Remove spacer-only markup, not content.
            safeFragment = CompactEmptySpacing(safeFragment);
            // Closed spoilers retain only their header in the live DOM; heavyweight
            // children stay inert in <template> until the user expands one.
            safeFragment = DeferSpoilers(safeFragment);

            if (string.IsNullOrWhiteSpace(safeFragment))
                safeFragment = "<p class='empty'>Описание отсутствует.</p>";

            string requestedBackground = theme?.Background;
            theme = theme ?? new InstanceDescriptionTheme();
            string bg = SafeHex(requestedBackground, backgroundHex, "#0B1020");
            string fg = SafeHex(theme.Foreground, "#E8EBF2");
            string muted = SafeHex(theme.Muted, "#A9B7CA");
            string accent = SafeHex(theme.Accent, "#1987FF");
            string surface = SafeHex(theme.Surface, "#10192A");
            string border = SafeHex(theme.Border, "#243149");
            bool light = IsLight(bg);

            var result = new StringBuilder(11000 + safeFragment.Length);
            string nonce = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            result.Append(@"<!doctype html><html lang='ru'><head><meta charset='utf-8'>");
            result.Append(@"<meta name='viewport' content='width=device-width,initial-scale=1'>");
            // The containing HTML has a real, mapped HTTPS origin; YouTube requires
            // an identifiable embedding page. Never use referrer=no-referrer here.
            result.Append(@"<meta name='referrer' content='strict-origin-when-cross-origin'>");
            result.Append("<meta http-equiv='Content-Security-Policy' content=\"default-src 'none'; script-src 'nonce-");
            result.Append(nonce);
            result.Append("'; style-src 'unsafe-inline'; img-src https: data:; frame-src https://www.youtube.com https://www.youtube-nocookie.com; font-src 'none'; connect-src 'none'; base-uri 'none'; form-action 'none'; object-src 'none'\">");
            result.Append("<style>:root{color-scheme:")
                .Append(light ? "light" : "dark")
                .Append(";--bg:").Append(bg)
                .Append(";--fg:").Append(fg)
                .Append(";--muted:").Append(muted)
                .Append(";--accent:").Append(accent)
                .Append(";--surface:").Append(surface)
                .Append(";--border:").Append(border)
                .Append(";--hover:color-mix(in srgb,var(--surface) 88%,var(--accent))")
                .Append(";--chip:color-mix(in srgb,var(--surface) 92%,var(--fg))")
                .Append(";--scroll:color-mix(in srgb,var(--bg) 68%,var(--fg))}");
            result.Append(@"
              *,*:before,*:after{box-sizing:border-box}
              html{width:100%;min-height:100%;background:var(--bg);overflow-x:hidden;overflow-y:auto;
                   scrollbar-gutter:stable;scrollbar-color:var(--scroll) var(--bg);scrollbar-width:thin}
              body{margin:0;min-height:100vh;width:100%;background:var(--bg);color:var(--fg);
                   padding:22px clamp(16px,1.35vw,28px) 46px 24px;
                   font:14px/1.65 'Segoe UI',Arial,system-ui,sans-serif;overflow-wrap:anywhere;overflow-x:clip}
              ::-webkit-scrollbar{width:9px;height:9px}
              ::-webkit-scrollbar-track{background:var(--bg)}
              ::-webkit-scrollbar-thumb{background:var(--scroll);border:2px solid var(--bg);border-radius:9px}
              /* Fix the page geometry before remote images/iframes are fetched.
                 No entrance transitions: a native WebView2 window must not slide. */
              main.overview-layout{width:100%;max-width:none;margin:0;display:grid;
                   grid-template-columns:minmax(0,1fr) 282px;
                   justify-content:stretch;align-items:start;gap:20px;
                   transition:none!important;animation:none!important;transform:none!important}
              /* The reading COLUMN is centered, not its paragraphs. Keep a stable
                 readable measure even if the provider's HTML centers its contents. */
              article.description-content{min-width:0;width:100%;max-width:980px;margin:0 auto;
                   text-align:left;transition:none!important;animation:none!important;
                   transform:none!important}
              /* CurseForge/Modrinth descriptions often wrap all their prose in a
                 <center> or a center-aligned <div>. Do not inherit that
                 alignment into full-width paragraphs. Images keep their independent
                 margin:auto centering and media wrappers are handled separately. */
              article.description-content :is(center,div[align='center'],div[style*='text-align:center'],div[style*='text-align: center']){
                   text-align:left!important}
              article.description-content :is(p,li,h1,h2,h3,h4,blockquote,figcaption):not(:has(img,iframe)){
                   text-align:left!important}
              aside.metadata-sidebar{position:sticky;top:16px;align-self:start;min-width:0;
                   width:282px;transform:none!important;transition:none!important;animation:none!important;
                   padding:18px;background:var(--surface);border:1px solid var(--border);border-radius:12px}
              .metadata-sidebar h3{font-size:16px;margin:0 0 16px;font-weight:650}
              .metadata-field{border-top:1px solid var(--border);padding:14px 0 2px}
              .metadata-field:first-of-type{border-top:0;padding-top:0}
              .metadata-key{display:block;color:var(--muted);font-size:12px;margin-bottom:4px}
              .metadata-value{color:var(--fg);font-weight:600;overflow-wrap:anywhere}
              .metadata-chips{display:flex;flex-wrap:wrap;gap:6px;margin-top:9px}
              .metadata-chips span{padding:3px 7px;border:1px solid var(--border);border-radius:5px;
                   background:var(--chip);font-size:11px;font-weight:500;color:var(--fg)}
              h1,h2,h3,h4{color:var(--fg);line-height:1.3;letter-spacing:-.01em;margin:1.35em 0 .55em;font-weight:650}
              h1{font-size:26px}h2{font-size:22px}h3{font-size:19px}h4{font-size:17px}
              p{margin:.6em 0 1em}a{color:var(--accent);text-decoration:none}a:hover{text-decoration:underline;filter:brightness(1.08)}
              a:focus-visible{outline:2px solid var(--accent);outline-offset:3px}
              img{display:block;max-width:100%;width:auto;height:auto;object-fit:contain;
                   border-radius:9px;margin:12px auto}
              /* Hero images/screenshots may expand into the reading column.
                 Smaller buttons and badges keep their intended dimensions. */
              article img.content-media{width:100%;height:auto;max-width:100%;}
              a>img{border-radius:9px}
              a:has(img){display:inline-block;max-width:100%;vertical-align:middle}
              article a:has(>img.content-media){display:block;width:100%;}
              /* Linked banners were left-aligned because their anchors were inline-block. */
              article p:has(a img),article center:has(a img){text-align:center}
              article p:has(>a img)>a{max-width:100%}
              img.social-icon{width:40px!important;height:40px!important;max-width:40px!important;
                   max-height:40px!important;object-fit:contain;display:inline-block;
                   margin:0!important;border-radius:5px}
              /* Do not clamp images just because their *link* points to Discord.
                 Such links can contain a wide button/banner, not a square icon. */
              img[width='48'],img[height='48'],img[width='64'],img[height='64']{
                   max-width:48px;max-height:48px}
              article tr.banner-links-row td{text-align:center;vertical-align:middle;padding:10px}
              article tr.banner-links-row img.link-banner{display:inline-block;
                   height:clamp(38px,3.4vw,64px)!important;width:auto!important;
                   max-width:100%!important;max-height:none!important;
                   object-fit:contain;margin:0 auto!important;border-radius:7px}
              p.social-row,div.social-row{display:flex;align-items:center;justify-content:center;
                   flex-wrap:wrap;gap:14px;min-height:44px}
              .social-row a:has(img){display:inline-flex;align-items:center;justify-content:center}
              a[href*='discord'] img[src$='.svg'],a[href*='patreon'] img[src$='.svg'],
              img[src*='patreon'][src$='.svg'],
              a[href*='patreon'] img[src*='icon'],a[href*='discord'] img[src*='icon'],
              img[width='32'],img[height='32']{max-width:48px;max-height:48px}
              figure{margin:16px 0}blockquote{border-left:3px solid var(--accent);padding:.35em 1em;margin:14px 0;color:var(--muted)}
              hr{border:0;border-top:1px solid var(--border);margin:22px 0}
              ul,ol{padding-left:24px}li{margin:6px 0}
              pre{background:var(--surface);border:1px solid var(--border);padding:16px;overflow:auto;border-radius:8px}
              code{font-family:Cascadia Code,Consolas,monospace}p code,li code{background:var(--chip);padding:2px 5px;border-radius:4px}
              .video-box{width:100%;max-width:900px;margin:18px auto 20px;
                   content-visibility:auto;contain-intrinsic-size:auto 540px}
              .video-box iframe{display:block;width:100%;aspect-ratio:16/9;height:auto;
                   border:0;border-radius:10px;background:var(--surface)}
              .video-fallback{display:block;text-align:right;font-size:12px;margin-top:7px}
              details{border:1px solid var(--border);background:var(--surface);border-radius:9px;margin:14px 0;overflow:hidden}
              summary{cursor:pointer;font-weight:600;padding:12px 16px;user-select:none;
                   color:var(--fg);list-style:none;display:flex;align-items:center;gap:11px}
              summary::-webkit-details-marker{display:none}
              summary:before{content:'';width:9px;height:9px;flex:none;border-right:2px solid var(--accent);
                   border-bottom:2px solid var(--accent);transform:rotate(-45deg);transition:transform .16s ease}
              details[open]>summary:before{transform:rotate(45deg)}
              summary:hover{background:var(--hover);color:var(--fg)}
              summary:focus-visible{outline:2px solid var(--accent);outline-offset:-2px}
              details[open]>summary{border-bottom:1px solid var(--border);background:var(--hover)}
              details>*:not(summary):not(template){margin:14px 16px}
              table{border-collapse:collapse;display:block;overflow-x:auto;max-width:100%}
              th,td{border:1px solid var(--border);padding:7px 10px}
              .empty{color:var(--muted)}
              @media (max-width:1119px){main.overview-layout{display:block;max-width:none}
                    article.description-content{max-width:820px}
                    aside.metadata-sidebar{display:none}}
              @media (max-width:700px){body{padding:16px 10px 40px 18px;font-size:14px}
                    h1{font-size:23px}h2{font-size:20px}.video-box{max-width:100%}}
            ");
            result.Append("</style></head><body><main class='overview-layout'><article class='description-content'>");
            result.Append(safeFragment);
            result.Append("</article>");
            AppendMetadata(result, metadata);
            result.Append("</main>");
            // This script is first-party and nonce-authorized. Scripts provided in
            // downloaded descriptions have already been removed by the sanitizer.
            result.Append("<script nonce='").Append(nonce).Append("'>");
            result.Append(@"(function(){
                const rootMargin='500px';
                const load=function(iframe){if(iframe.dataset.src){iframe.src=iframe.dataset.src;iframe.removeAttribute('data-src');}};
                const io=('IntersectionObserver' in window)
                  ?new IntersectionObserver(function(entries){entries.forEach(function(e){if(e.isIntersecting){load(e.target);io.unobserve(e.target);}});},{rootMargin:rootMargin})
                  :null;
                function watch(container){container.querySelectorAll('iframe[data-src]').forEach(function(frame){if(io){io.observe(frame);}else{load(frame);}});}
                watch(document);
                // Last-resort repair: a provider can embed Markdown inside an HTML
                // paragraph, leaving ![](url) visibly printed after Markdig.
                // This stays entirely inside our sanitized first-party document.
                function recoverLiteralImages(){
                  const re=/^!\s*\[([^\]]*)\]\s*\(\s*(https:\/\/[^\s)<>]+)\s*\)$/i;
                  document.querySelectorAll('article p').forEach(function(p){
                    if(p.querySelector('img,iframe,video'))return;
                    const m=p.textContent.trim().match(re);
                    if(!m)return;
                    let parsed;
                    try{parsed=new URL(m[2]);}catch(_){return;}
                    if(parsed.protocol!=='https:')return;
                    const img=document.createElement('img');
                    img.src=parsed.href;img.alt=m[1];img.loading='lazy';
                    p.replaceChildren(img);
                  });
                }
                recoverLiteralImages();
                // Forward Escape through the WebView2 HWND to the existing WPF shortcut.
                // This first-party document is the only page allowed to send this message.
                document.addEventListener('keydown',function(e){
                  if(e.key==='Escape'&&!e.defaultPrevented&&window.chrome&&window.chrome.webview){
                    window.chrome.webview.postMessage('lexplosion-overview-escape');
                  }
                },true);
                // Reduce oversize social glyphs (Discord/Patreon/etc) without
                // shrinking wide sponsor banners or regular screenshots.
                function markSocialImage(img){
                  const apply=function(){
                    const width=img.naturalWidth, height=img.naturalHeight;
                    if(!width||!height)return;
                    const ratio=width/height;
                    const anchor=img.closest('a');
                    const ref=(anchor?anchor.href:'')+' '+img.currentSrc+' '+(img.alt||'');
                    const group=img.closest('p,div');
                    const siblings=group?group.querySelectorAll('img').length:0;
                    // A row of small linked square images is usually a social bar,
                    // even when its asset names are opaque CDN hashes.
                    const rowOfIcons=!!(anchor && group && group.tagName==='P' &&
                       siblings>=3 && siblings<=7 && group.textContent.trim().length<120);
                    if(!/discord|patreon|ko-fi|github|twitter|twitch|social|gamepad|controller/i.test(ref)
                        && !rowOfIcons)return;
                    if(ratio<0.55||ratio>1.75)return;
                    if(width>512 && !rowOfIcons)return;
                    img.classList.add('social-icon');
                    if(group && siblings>1)group.classList.add('social-row');
                  };
                  if(img.complete)apply();else img.addEventListener('load',apply,{once:true});
                }
                function watchSocial(container){container.querySelectorAll('img').forEach(markSocialImage);}
                // Distinguish full-width artwork from little icons. Do not use an
                // anchor's hostname as a proxy for the asset's actual dimensions.
                function sizeMedia(container){
                  container.querySelectorAll('img').forEach(function(img){
                    const apply=function(){
                      const w=img.naturalWidth, h=img.naturalHeight;
                      if(!w||!h)return;
                      const ratio=w/h;
                      const row=img.closest('tr');
                      if(row && row.querySelectorAll('img').length>=2 && ratio>=1.75){
                        const banners=Array.from(row.querySelectorAll('img'));
                        if(banners.every(function(b){return b.naturalWidth &&
                          b.naturalWidth/b.naturalHeight>=1.75;})){
                          row.classList.add('banner-links-row');
                          banners.forEach(function(b){b.classList.remove('social-icon');b.classList.add('link-banner');});
                          return;
                        }
                      }
                      if(w>=560 && h>=120 && ratio<=6.2 && !img.classList.contains('social-icon'))
                        img.classList.add('content-media');
                    };
                    if(img.complete)apply();else img.addEventListener('load',apply,{once:true});
                  });
                }
                watchSocial(document);
                sizeMedia(document);
                document.addEventListener('toggle',function(event){
                  const detail=event.target;if(!(detail instanceof HTMLDetailsElement)||!detail.open)return;
                  const template=detail.querySelector(':scope > template');
                  if(template){detail.appendChild(template.content);template.remove();}
                  watch(detail);
                  watchSocial(detail);
                  sizeMedia(detail);
                },true);
            })();");
            result.Append("</script></body></html>");
            return result.ToString();
        }

        private static string SafeHex(params string[] candidates)
        {
            foreach (string candidate in candidates)
            {
                if (candidate != null && Regex.IsMatch(candidate, @"^#[0-9a-fA-F]{6}$"))
                    return candidate;
            }
            return "#0B1020";
        }

        private static bool IsLight(string hex)
        {
            int r = Convert.ToInt32(hex.Substring(1, 2), 16);
            int g = Convert.ToInt32(hex.Substring(3, 2), 16);
            int b = Convert.ToInt32(hex.Substring(5, 2), 16);
            return (0.2126 * r + 0.7152 * g + 0.0722 * b) > 155;
        }

        private static void AppendMetadata(StringBuilder result, InstanceDescriptionMetadata meta)
        {
            // Text-only metadata, never raw HTML from the mod hosting providers.
            string author = WebUtility.HtmlEncode(meta?.Author ?? "—");
            string updated = WebUtility.HtmlEncode(meta?.Updated ?? "—");
            result.Append("<aside class='metadata-sidebar' aria-label='Информация о сборке'>");
            result.Append("<h3>Информация о сборке</h3>");
            result.Append("<div class='metadata-field'><span class='metadata-key'>Автор</span><div class='metadata-value'>")
                  .Append(author).Append("</div></div>");
            result.Append("<div class='metadata-field'><span class='metadata-key'>Обновлено</span><div class='metadata-value'>")
                  .Append(updated).Append("</div></div>");
            result.Append("<div class='metadata-field'><span class='metadata-key'>Категории</span><div class='metadata-chips'>");
            var tags = meta?.Categories ?? Array.Empty<string>();
            if (tags.Length == 0) result.Append("<span>—</span>");
            foreach (string tag in tags.Take(16))
                result.Append("<span>").Append(WebUtility.HtmlEncode(tag ?? string.Empty)).Append("</span>");
            result.Append("</div></div></aside>");
        }

        private static string RepairMarkdownImages(string input)
        {
            if (string.IsNullOrWhiteSpace(input) || input.IndexOf('!') < 0)
                return input;

            // Modrinth descriptions can contain escaped Markdown punctuation and
            // line breaks between the image marker and its URL. Normalize only
            // HTTPS images; final markup still passes through the sanitizer.
            string normalized = input.Replace(@"\![", "![")
                .Replace(@"\[", "[").Replace(@"\]", "]").Replace(@"\(", "(").Replace(@"\)", ")")
                .Replace(@"\:", ":").Replace(@"\/", "/");
            return Regex.Replace(normalized,
                @"!\[(?<alt>[^\]\r\n]*)\][ \t\r\n]*\([ \t\r\n]*(?<url>https?://[^\s)\r\n]+)[ \t\r\n]*\)",
                match =>
                {
                    string url = ExternalUrl(match.Groups["url"].Value, InstanceSource.Modrinth);
                    if (url == null) return match.Value;
                    return "<img src=\"" + WebUtility.HtmlEncode(url) + "\" alt=\"" +
                        WebUtility.HtmlEncode(match.Groups["alt"].Value) + "\" />";
                }, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
        }

        // Convert image tokens that survived Markdown parsing. A frequent example is
        // <p>![](<a href="https://...">https://...</a>)</p>, or mixed HTML/Markdown
        // returned by a provider. Work on parsed nodes so arbitrary user text is not
        // interpreted as markup, and pass the generated nodes through Sanitize later.
        private static string RepairRenderedMarkdownImages(string fragment, InstanceSource source)
        {
            if (string.IsNullOrWhiteSpace(fragment) ||
                fragment.IndexOf("![", StringComparison.Ordinal) < 0)
                return fragment;

            var doc = new HtmlDocument { OptionFixNestedTags = true };
            doc.LoadHtml(fragment);
            var paragraphs = doc.DocumentNode.Descendants("p").ToList();
            foreach (var paragraph in paragraphs)
            {
                if (paragraph.ParentNode == null || paragraph.Descendants("img").Any())
                    continue;

                string raw = WebUtility.HtmlDecode(paragraph.InnerText ?? string.Empty).Trim();
                var match = MatchBareImage(raw);
                HtmlNode next = null;
                if (!match.Success && Regex.IsMatch(raw, @"^!\s*\[[^\]]*\]$"))
                {
                    next = paragraph.NextSibling;
                    while (next != null && next.NodeType == HtmlNodeType.Text &&
                           string.IsNullOrWhiteSpace(next.InnerText))
                        next = next.NextSibling;
                    if (next != null && next.Name == "p")
                        match = MatchBareImage(raw + "(" +
                            (WebUtility.HtmlDecode(next.InnerText ?? string.Empty).Trim().TrimStart('(')));
                    else
                        next = null;
                }
                if (!match.Success) continue;

                string url = ExternalUrl(match.Groups["url"].Value, source);
                if (url == null) continue;
                var image = HtmlNode.CreateNode("<img />");
                image.SetAttributeValue("src", url);
                image.SetAttributeValue("alt", match.Groups["alt"].Value);
                paragraph.RemoveAllChildren();
                paragraph.AppendChild(image);
                if (next != null && next.ParentNode != null) next.Remove();
            }
            return doc.DocumentNode.InnerHtml;
        }

        private static Match MatchBareImage(string text)
        {
            return Regex.Match(text ?? string.Empty,
                @"^!\s*\[(?<alt>[^\]\r\n]*)\]\s*\(\s*(?<url>https?://[^\s<>\)]+)\s*\)\s*$",
                RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(200));
        }

        // Strip only provider-generated *empty* spacing. Keep authored text,
        // pictures, videos, tables and expanded/collapsed spoiler structure intact.
        // The most common cause of the ~300 px gap after YouTube is a sequence of
        // <p>&nbsp;</p>, <p><br></p> and repeated <br> tags from CurseForge HTML.
        private static string CompactEmptySpacing(string html)
        {
            if (string.IsNullOrWhiteSpace(html)) return html;
            var doc = new HtmlDocument { OptionFixNestedTags = true };
            doc.LoadHtml(html);

            foreach (var node in doc.DocumentNode.Descendants().ToList())
            {
                if (node.ParentNode == null) continue;
                if (node.Name != "p" && node.Name != "div" && node.Name != "center") continue;
                // Do not change structural elements containing any actual media,
                // interactive details, hyperlinks or other meaningful blocks.
                if (node.Descendants().Any(child =>
                    child.Name == "img" || child.Name == "iframe" ||
                    child.Name == "details" || child.Name == "table" ||
                    child.Name == "hr" || child.Name == "a" ||
                    child.Name == "ul" || child.Name == "ol")) continue;
                string text = WebUtility.HtmlDecode(node.InnerText ?? string.Empty)
                    .Replace('\u00a0', ' ').Trim();
                if (text.Length == 0) node.Remove();
            }

            // Repeated <br> elements can be direct descendants of <center>/<div>
            // rather than inside paragraphs. Cap those runs to a single break.
            foreach (var container in doc.DocumentNode.Descendants().ToList())
            {
                if (container.ParentNode == null && container != doc.DocumentNode) continue;
                bool lastWasBreak = false;
                foreach (var node in container.ChildNodes.ToList())
                {
                    if (node.Name == "br")
                    {
                        if (lastWasBreak) node.Remove();
                        lastWasBreak = true;
                    }
                    else if (node.NodeType != HtmlNodeType.Text ||
                             !string.IsNullOrWhiteSpace(WebUtility.HtmlDecode(node.InnerText)
                                 .Replace('\u00a0', ' '))) lastWasBreak = false;
                }
            }
            return doc.DocumentNode.InnerHtml;
        }

        private static string DeferSpoilers(string html)
        {
            var doc = new HtmlDocument { OptionFixNestedTags = true };
            doc.LoadHtml(html);
            foreach (var details in doc.DocumentNode.Descendants("details").ToList())
            {
                var summary = details.ChildNodes.FirstOrDefault(n => n.Name == "summary");
                if (summary == null)
                {
                    summary = HtmlNode.CreateNode("<summary>Show spoiler</summary>");
                    details.PrependChild(summary);
                }
                // Template is inert: media and large lists under a closed spoiler
                // are not attached to the live layout until first expansion.
                var template = HtmlNode.CreateNode("<template></template>");
                foreach (var child in details.ChildNodes.ToList())
                    if (!ReferenceEquals(child, summary)) template.AppendChild(child);
                details.AppendChild(template);
            }
            return doc.DocumentNode.InnerHtml;
        }

        private static bool LooksLikeHtml(string value)
        {
            return value.IndexOf("<p", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   value.IndexOf("<div", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   value.IndexOf("<img", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   value.IndexOf("<iframe", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string Sanitize(string fragment, InstanceSource source)
        {
            var document = new HtmlDocument { OptionFixNestedTags = true };
            document.LoadHtml(fragment);
            FilterChildren(document.DocumentNode, source);
            return document.DocumentNode.InnerHtml;
        }

        private static void FilterChildren(HtmlNode parent, InstanceSource source)
        {
            foreach (var node in parent.ChildNodes.ToList())
            {
                if (node.NodeType == HtmlNodeType.Comment)
                {
                    node.Remove();
                    continue;
                }
                if (node.NodeType != HtmlNodeType.Element)
                    continue;

                if (RemoveWithContent.Contains(node.Name))
                {
                    node.Remove();
                    continue;
                }

                // CurseForge marks its expandable sections with CSS classes.
                // Preserve their meaning by converting to semantic <details> before
                // removing untrusted classes/styles/attributes.
                string className = node.GetAttributeValue("class", string.Empty);
                bool isSpoiler = node.Name.Equals("div", StringComparison.OrdinalIgnoreCase) &&
                    Regex.IsMatch(className, @"(^|\s)(spoiler|spoiler-wrapper)(\s|$)", RegexOptions.IgnoreCase);
                if (isSpoiler)
                {
                    // For wrapper > spoiler nesting only the outer wrapper becomes
                    // a disclosure; nested content is unwrapped by the inner pass.
                    bool nested = node.ParentNode != null &&
                        Regex.IsMatch(node.ParentNode.GetAttributeValue("class", string.Empty),
                            @"(^|\s)spoiler-wrapper(\s|$)", RegexOptions.IgnoreCase);
                    if (!nested)
                    {
                        FilterChildren(node, source);
                        var disclosure = HtmlNode.CreateNode("<details class='cf-spoiler'><summary>Show spoiler</summary></details>");
                        foreach (var child in node.ChildNodes.ToList()) disclosure.AppendChild(child);
                        parent.ReplaceChild(disclosure, node);
                        continue;
                    }
                }
                FilterChildren(node, source);
                if (!AllowedTags.Contains(node.Name))
                {
                    // Keep text and children of harmless wrappers, e.g. <section> or <center>.
                    foreach (var child in node.ChildNodes.ToList())
                        parent.InsertBefore(child, node);
                    node.Remove();
                    continue;
                }

                string href = node.GetAttributeValue("href", null);
                string src = node.GetAttributeValue("src", null);
                string alt = node.GetAttributeValue("alt", null);
                string imageWidth = node.GetAttributeValue("width", null);
                string imageHeight = node.GetAttributeValue("height", null);
                string inlineStyle = node.GetAttributeValue("style", null);
                string fontColor = node.GetAttributeValue("color", null);
                string align = node.GetAttributeValue("align", null);
                string safeStyle = SafeInlineStyle(inlineStyle, node.Name);
                if (node.Name.Equals("font", StringComparison.OrdinalIgnoreCase))
                    safeStyle = SafeInlineStyle("color:" + fontColor, "font") ?? safeStyle;
                if (safeStyle == null && (align == "center" || align == "right" || align == "left"))
                    safeStyle = SafeInlineStyle("text-align:" + align, node.Name);
                foreach (var attribute in node.Attributes.ToList())
                    node.Attributes.Remove(attribute);

                if (safeStyle != null)
                    node.SetAttributeValue("style", safeStyle);

                if (node.Name.Equals("a", StringComparison.OrdinalIgnoreCase))
                {
                    // CurseForge sometimes wraps a playable iframe in an anchor.
                    // Nested clickable elements break video playback/interaction.
                    if (node.Descendants("iframe").Any())
                    {
                        foreach (var child in node.ChildNodes.ToList())
                            parent.InsertBefore(child, node);
                        node.Remove();
                        continue;
                    }
                    string safeHref = ExternalUrl(href, source);
                    // Some provider descriptions turn ![](url) into link text.
                    // If its sole visible text is an image token, restore the image.
                    string innerText = WebUtility.HtmlDecode(node.InnerText ?? string.Empty).Trim();
                    var imageToken = Regex.Match(innerText,
                        @"^!\[([^\]]*)\]\((https?://[^\s)]+)\)$",
                        RegexOptions.IgnoreCase);
                    if (imageToken.Success)
                    {
                        string imageUrl = ExternalUrl(imageToken.Groups[2].Value, source);
                        if (imageUrl != null)
                        {
                            node.RemoveAllChildren();
                            var image = HtmlNode.CreateNode("<img />");
                            image.SetAttributeValue("src", imageUrl);
                            image.SetAttributeValue("loading", "lazy");
                            image.SetAttributeValue("alt", imageToken.Groups[1].Value);
                            node.AppendChild(image);
                        }
                    }
                    if (safeHref != null)
                    {
                        node.SetAttributeValue("href", safeHref);
                        node.SetAttributeValue("target", "_blank");
                        node.SetAttributeValue("rel", "noopener noreferrer nofollow");
                    }
                }
                else if (node.Name.Equals("img", StringComparison.OrdinalIgnoreCase))
                {
                    string safeSrc = ExternalUrl(src, source);
                    if (safeSrc == null)
                    {
                        node.Remove();
                        continue;
                    }
                    node.SetAttributeValue("src", safeSrc);
                    node.SetAttributeValue("alt", alt ?? string.Empty);
                    node.SetAttributeValue("loading", "lazy");
                    node.SetAttributeValue("referrerpolicy", "no-referrer");
                    // Keep the image's intended dimensions if they're safe integers;
                    // CSS still caps their size to the reading column.
                    if (int.TryParse(imageWidth, out int width) && width > 0 && width <= 4096)
                        node.SetAttributeValue("width", width.ToString());
                    if (int.TryParse(imageHeight, out int height) && height > 0 && height <= 4096)
                        node.SetAttributeValue("height", height.ToString());
                }
                else if (node.Name.Equals("iframe", StringComparison.OrdinalIgnoreCase))
                {
                    string safeSrc = YouTubeEmbedUrl(src);
                    if (safeSrc == null)
                    {
                        node.Remove();
                        continue;
                    }
                    node.SetAttributeValue("data-src", safeSrc);
                    node.SetAttributeValue("loading", "lazy");
                    node.SetAttributeValue("allowfullscreen", "allowfullscreen");
                    node.SetAttributeValue("allow", "accelerometer; autoplay; encrypted-media; gyroscope; picture-in-picture");
                    node.SetAttributeValue("referrerpolicy", "strict-origin-when-cross-origin");
                    // YouTube needs scripts and presentation. Sandboxing the player
                    // further can break sign-in/configuration in Chromium WebView2.

                    string id = new Uri(safeSrc).AbsolutePath.Substring("/embed/".Length);
                    var box = HtmlNode.CreateNode("<div class='video-box'></div>");
                    parent.InsertBefore(box, node);
                    node.Remove();
                    box.AppendChild(node);
                    string watchUrl = "https://www.youtube.com/watch?v=" + id;
                    var link = HtmlNode.CreateNode("<a class='video-fallback'>Смотреть на YouTube ↗</a>");
                    link.SetAttributeValue("href", watchUrl);
                    link.SetAttributeValue("target", "_blank");
                    link.SetAttributeValue("rel", "noopener noreferrer nofollow");
                    box.AppendChild(link);
                }
            }
        }

        // Preserve harmless formatting from CurseForge descriptions while blocking
        // positioning, visibility tricks, background URLs and CSS injection.
        private static string SafeInlineStyle(string style, string tag)
        {
            if (string.IsNullOrWhiteSpace(style)) return null;
            if (!(tag.Equals("p", StringComparison.OrdinalIgnoreCase) ||
                  tag.Equals("div", StringComparison.OrdinalIgnoreCase) ||
                  tag.Equals("span", StringComparison.OrdinalIgnoreCase) ||
                  tag.Equals("strong", StringComparison.OrdinalIgnoreCase) ||
                  tag.Equals("h1", StringComparison.OrdinalIgnoreCase) ||
                  tag.Equals("h2", StringComparison.OrdinalIgnoreCase) ||
                  tag.Equals("h3", StringComparison.OrdinalIgnoreCase) ||
                  tag.Equals("h4", StringComparison.OrdinalIgnoreCase) ||
                  tag.Equals("li", StringComparison.OrdinalIgnoreCase) ||
                  tag.Equals("font", StringComparison.OrdinalIgnoreCase))) return null;

            var accepted = new List<string>();
            foreach (var declaration in style.Split(';'))
            {
                var pair = declaration.Split(new[] { ':' }, 2);
                if (pair.Length != 2) continue;
                var name = pair[0].Trim().ToLowerInvariant();
                var value = pair[1].Trim().ToLowerInvariant();
                if (name == "color" && Regex.IsMatch(value,
                    @"^#[0-9a-f]{3}([0-9a-f]{3})?$") )
                    accepted.Add("color:" + value);
                else if (name == "color" && Regex.IsMatch(value,
                    @"^rgb\(\s*\d{1,3}\s*,\s*\d{1,3}\s*,\s*\d{1,3}\s*\)$"))
                    accepted.Add("color:" + value);
                else if (name == "text-align" && (value == "center" || value == "right" || value == "left"))
                    accepted.Add("text-align:" + value);
            }
            return accepted.Count == 0 ? null : string.Join(";", accepted);
        }

        private static string ExternalUrl(string raw, InstanceSource source)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;
            string value = WebUtility.HtmlDecode(raw.Trim())
                .Replace("https\\:", "https:").Replace("http\\:", "http:")
                .Replace("\\/", "/");
            if (value.StartsWith("//", StringComparison.Ordinal)) value = "https:" + value;
            if (value.StartsWith("/", StringComparison.Ordinal))
            {
                string root = source == InstanceSource.Modrinth ? "https://modrinth.com" : "https://www.curseforge.com";
                value = root + value;
            }
            if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
                return null;
            // CurseForge wraps external links in /linkout?remoteUrl=..., sometimes
            // with a doubly encoded URL. Return the real target if it is HTTPS.
            if ((uri.Host.Equals("www.curseforge.com", StringComparison.OrdinalIgnoreCase) ||
                 uri.Host.Equals("curseforge.com", StringComparison.OrdinalIgnoreCase)) &&
                uri.AbsolutePath.TrimEnd('/').EndsWith("/linkout", StringComparison.OrdinalIgnoreCase))
            {
                var match = Regex.Match(uri.Query, @"(?:[?&])remoteUrl=([^&]+)", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    string remote = match.Groups[1].Value;
                    for (int i = 0; i < 3; i++)
                    {
                        string decoded = WebUtility.UrlDecode(remote);
                        if (decoded == remote) break;
                        remote = decoded;
                    }
                    if (Uri.TryCreate(remote, UriKind.Absolute, out var target) &&
                        target.Scheme == Uri.UriSchemeHttps)
                        return target.AbsoluteUri;
                }
            }
            return uri.AbsoluteUri;
        }

        private static string YouTubeEmbedUrl(string raw)
        {
            string absolute = ExternalUrl(raw, InstanceSource.Curseforge);
            if (!Uri.TryCreate(absolute, UriKind.Absolute, out var uri)) return null;
            string host = uri.Host.ToLowerInvariant();
            if (host != "youtube.com" && host != "www.youtube.com" &&
                host != "youtube-nocookie.com" && host != "www.youtube-nocookie.com")
                return null;
            if (!uri.AbsolutePath.StartsWith("/embed/", StringComparison.OrdinalIgnoreCase))
                return null;
            string id = uri.AbsolutePath.Substring("/embed/".Length).Trim('/');
            if (id.Length == 0 || id.Length > 32 || id.Any(c => !(char.IsLetterOrDigit(c) || c == '_' || c == '-')))
                return null;
            return "https://www.youtube-nocookie.com/embed/" + id;
        }
    }
}
