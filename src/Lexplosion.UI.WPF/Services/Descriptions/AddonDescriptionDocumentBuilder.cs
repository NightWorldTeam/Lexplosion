using Lexplosion;
using Lexplosion.Logic.Management.Instances;
using System.Text.RegularExpressions;
using System.Net;

namespace Lexplosion.UI.WPF.Services.Descriptions
{
    // Addon-specific presentation on top of the shared safe Markdown/HTML renderer.
    // Changes here must not affect the instance overview page.
    internal static class AddonDescriptionDocumentBuilder
    {
        internal static string Build(string description, ProjectSource projectSource, InstanceDescriptionTheme theme, string noDescription)
        {
            InstanceSource source = projectSource == ProjectSource.Curseforge
                    ? InstanceSource.Curseforge : InstanceSource.Modrinth;

                // The shared renderer is used by instance profiles too. Customize only
                // this document; don't change instance pages or their WebView behavior.
            string document = InstanceDescriptionHtmlBuilder.Build(
                    description, source, theme.Background, null, theme);
            // The shared renderer's default placeholder is Russian; replace only
            // this add-on page's fallback without changing instance descriptions.
            if (string.IsNullOrWhiteSpace(description) && !string.IsNullOrWhiteSpace(noDescription))
                document = document.Replace("<p class='empty'>Описание отсутствует.</p>",
                    "<p class='empty'>" + WebUtility.HtmlEncode(noDescription) + "</p>");
                // The builder unconditionally adds an "Информация о сборке" HTML aside.
                // The addon page already has a native WPF facts panel, so remove ONLY
                // that generated aside. The HTML remains sanitized by the builder.
                document = Regex.Replace(document,
                    @"<aside class='metadata-sidebar'[^>]*>.*?</aside>",
                    string.Empty, RegexOptions.Singleline);
                const string layoutCss = @"
                    main.overview-layout{display:block!important;width:100%!important;
                        max-width:none!important;margin:0!important;gap:0!important;}
                    article.description-content{width:100%!important;max-width:1140px!important;
                        margin:0!important;padding:0!important;text-align:left!important;}
                    /* WPF sets the WebView's left inset to exactly the TabPanel inset.
                       Keep HTML's own left padding at zero so it cannot drift on resize. */
                    body{padding:28px 34px 68px 0!important;
                        font-size:clamp(14px,0.33vw + 11px,18px)!important;line-height:1.70!important;}
                    article.description-content > :first-child{margin-top:0!important;}
                    article.description-content > h1:first-child{font-size:clamp(25px,0.35vw + 22px,29px)!important;}
                    article.description-content p{margin-top:.55em;margin-bottom:.95em;}
                    article.description-content li{margin:5px 0;}
                    article.description-content h2{margin-top:1.7em;}
                    article.description-content img{height:auto;max-width:100%;}
                    @media (max-width:1180px){
                        article.description-content{max-width:100%!important;margin:0!important;}
                    }
                    @media (max-width:760px){
                        body{padding:22px 24px 48px 0!important;}
                        article.description-content > h1:first-child{font-size:23px!important;}
                    }";
                document = document.Replace("</style></head>", layoutCss + "</style></head>");
            return document;
        }
    }
}
