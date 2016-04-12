using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Razor.TagHelpers;
using Microsoft.AspNet.Mvc.ViewFeatures;
using Microsoft.Extensions.WebEncoders;

namespace FPS.TagHelpers
{
    [HtmlTargetElement("ul", Attributes = AttributeName)]
    [HtmlTargetElement("ul", Attributes = ActiveCssName)]
    public class ActiveListTagHelper : TagHelper
    {
        private const string AttributeName = "active-list";
        private const string ActiveCssName = "active-css";

        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        /// <summary>
        /// The name of css to be activated.
        /// </summary>
        [HtmlAttributeName(ActiveCssName)]
        public string ActiveCss { get; set; }


        /// <summary>
        /// Specifies if the current &lt;ul&gt; tag will be aware of the current request path.
        /// </summary>
        [HtmlAttributeName(AttributeName)]
        public bool IsActive { get; set; }

        protected string CurrentPath => ViewContext.HttpContext.Request.Path.ToString().ToLower();


        private readonly IHtmlEncoder _encoder;

        public ActiveListTagHelper(IHtmlEncoder encoder)
        {
            _encoder = encoder;
        }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (IsActive)
            {
                var htmlContent = await GetHtmlAsync(output);
                if (!string.IsNullOrEmpty(htmlContent))
                {
                    var html = new HtmlDocument();

                    html.LoadHtml(htmlContent);
                    EnumerateMenu(html.DocumentNode);

                    var result = await GetHtmlAsync(html);
                    output.Content.SetHtmlContent(result);
                }
            }

            // Cleanup: remove attributes form the tag
            output.Attributes.RemoveAll(AttributeName);
            output.Attributes.RemoveAll(ActiveCssName);

            await base.ProcessAsync(context, output);
        }

        private void EnumerateMenu(HtmlNode menu)
        {
            foreach (var li in menu.Descendants("li"))
            {
                var submenu = li.Descendants("ul").FirstOrDefault();
                if (submenu != null)
                {
                    EnumerateMenu(submenu);
                }
                else
                {
                    var anchor = li.Descendants("a").FirstOrDefault();
                    var path = $"{anchor?.Attributes["href"].Value?.ToLower()}";
                    if (!string.IsNullOrEmpty(path))
                    {
                        var active = path == "/" ? CurrentPath.Equals(path) : CurrentPath.Contains(path);
                        if (active)
                        {
                            ActivateList(li);
                            return;
                        }
                    }
                }
            }
        }

        private void ActivateList(HtmlNode node)
        {
            var item = node;
            do
            {
                var css = item.Attributes["class"] ?? item.Attributes.Append("class");
                var activeCss = GetActiveCss();
                if (css.Value.Contains("collapse"))
                    activeCss += " in";
                if (!css.Value.Contains(activeCss))
                    css.Value = $"{css.Value} {activeCss}".Trim();
                item = item.ParentNode;
            } while (item != null && item.ParentNode.Name != "#document");
        }

        private async Task<string> GetHtmlAsync(TagHelperOutput output)
        {
            var childContent = await output.GetChildContentAsync();
            if (childContent.IsEmpty)
                return null;

            using (var ms = new MemoryStream())
            {
                var attributes = output.Attributes.ToDictionary(key => key.Name, elem => elem.Value);
                var tag = new TagBuilder(output.TagName);
                tag.MergeAttributes(attributes);
                tag.InnerHtml.AppendHtml(childContent.GetContent());

                var writer = new StreamWriter(ms);
                tag.WriteTo(writer, _encoder);
                await writer.FlushAsync();
                ms.Seek(0, SeekOrigin.Begin);

                return await new StreamReader(ms).ReadToEndAsync();
            }
        }

        private async Task<string> GetHtmlAsync(HtmlDocument html)
        {
            using (var ms = new MemoryStream())
            {
                var writer = new StreamWriter(ms);
                html.Save(writer);
                await writer.FlushAsync();
                ms.Seek(0, SeekOrigin.Begin);

                return await new StreamReader(ms).ReadToEndAsync();
            }
        }

        private string GetActiveCss()
        {
            return ActiveCss ?? "active";
        }
    }
}
