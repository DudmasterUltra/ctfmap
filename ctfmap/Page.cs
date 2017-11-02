using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace ctfmap {

    public class Page {
        
        public Uri url { get; }
        public bool hasFlag { get; private set; }
        public bool hasCrawled { get; private set; }

        private PageManager manager;

        public Page(Uri url, PageManager manager) {

            this.url = url;
            this.manager = manager;
            hasCrawled = false;

        }

        public List<Page> crawl() {

            hasCrawled = true;
            List<Page> found = new List<Page>();
            HttpResponseMessage response = manager.client.GetAsync(url).Result;
            if (response.IsSuccessStatusCode) {

                manager.triggerOnLinkCrawled(this);
                HtmlDocument document = new HtmlDocument();
                String documentString = response.Content.ReadAsStringAsync().Result;
                foreach (String flag in manager.flags) {

                    if (documentString.Contains(flag)) {

                        hasFlag = true;
                        manager.triggerOnFlagFound(this, flag);

                    }

                }
                document.LoadHtml(documentString);
                HtmlNodeCollection hrefs = document.DocumentNode.SelectNodes(".//a[@href]");
                HtmlNodeCollection scripts = document.DocumentNode.SelectNodes(".//script[@src]");
                HtmlNodeCollection iframes = document.DocumentNode.SelectNodes(".//iframe[@src]");
                if (hrefs != null) {

                    foreach (HtmlNode href in hrefs) {

                        HtmlAttribute att = href.Attributes["href"];
                        found.Add(new Page(new Uri(manager.url, att.Value), manager));

                    }

                }
                if (scripts != null) {

                    foreach (HtmlNode script in scripts) {

                        HtmlAttribute att = script.Attributes["src"];
                        found.Add(new Page(new Uri(manager.url, att.Value), manager));

                    }

                }
                if (iframes != null) {

                    foreach (HtmlNode frame in iframes) {

                        HtmlAttribute att = frame.Attributes["src"];
                        found.Add(new Page(new Uri(manager.url, att.Value), manager));

                    }

                }

            }

            return found;

        }

        public override String ToString() {

            return url.ToString();

        }

        public override int GetHashCode() {

            int hash = 0;
            foreach (char c in url.ToString().ToCharArray()) {

                hash += c;

            }
            return hash;

        }

        public override bool Equals(Object obj) {

            if (obj == null) {

                return false;

            } else if (!obj.GetType().Equals(GetType())) {

                return false;

            } else {

                return ((Page) obj).url.Equals(url);

            }

        }

    }

}
