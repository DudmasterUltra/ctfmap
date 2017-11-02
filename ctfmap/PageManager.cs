using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace ctfmap {

    public class PageManager {

        public String[] flags { get; }
        public HttpClient client { get; }
        public bool evil { get; set; }
        public Uri url { get; }
        public int pageCount {

            get {

                return maps.Count;

            }

        }
        public int flagCount { get; private set; }

        private List<Page> maps;
        private CookieContainer cookieContainer;
        private bool foundCookieFlag;

        public delegate void LinkCrawled(Page map);
        public delegate void FlagFound(Page map, String flag);
        public delegate void CookieFlagFound(Cookie cookie);
        public event FlagFound onFlagFound;
        public event LinkCrawled onPageCrawl;
        public event CookieFlagFound onCookieFlagFound;

        public PageManager(Uri url, String[] flag) {

            this.url = url;
            this.flags = flag;
            HttpClientHandler handler = new HttpClientHandler();
            cookieContainer = new CookieContainer();
            handler.CookieContainer = cookieContainer;
            client = new HttpClient(handler);
            client.DefaultRequestHeaders.Add("User-Agent", "ctfmap");
            maps = new List<Page>();
            foundCookieFlag = false;

        }

        public void setCookie(String cookie) {

            cookieContainer.SetCookies(this.url, cookie);

        }

        public void triggerOnFlagFound(Page map, String flag) {

            flagCount++;
            onFlagFound?.Invoke(map, flag);

        }

        public void triggerOnLinkCrawled(Page map) {

            onPageCrawl?.Invoke(map);

        }

        private void addPage(Page page) {
            
            foreach (Page p in maps) {

                if (p.url.Equals(page.url)) {

                    return;

                }

            }
            maps.Add(page);

        }

        private void addPages(List<Page> pages) {

            foreach (Page p in pages) {

                addPage(p);

            }

        }

        private bool hasCrawledAll() {

            int crawled = 0;
            foreach (Page page in maps) {

                if (page.hasCrawled) {

                    crawled++;

                }

            }
            return crawled == maps.Count;

        }

        public Cookie checkCookiesForFlag() {

            foreach (Cookie cookie in cookieContainer.GetCookies(url)) {

                foreach (String f in flags) {

                    if (cookie.Name.Contains(f) || cookie.Value.Contains(f)) {

                        foundCookieFlag = true;
                        return cookie;

                    }

                }

            }
            return null;

        }

        public HttpStatusCode gitFolderHeadStatusCode() {

            return client.GetAsync(url.Scheme + "://" + url.Host + "/.git/HEAD").Result.StatusCode;

        }

        public HttpStatusCode gitFolderStatusCode() {

            return client.GetAsync(url.Scheme + "://" + url.Host + "/.git").Result.StatusCode;

        }

        public HttpStatusCode gitIgnoreStatusCode() {

            return client.GetAsync(url.Scheme + "://" + url.Host + "/.gitignore").Result.StatusCode;

        }

        public void scan() {

            // Scan for robots.txt

            HttpResponseMessage robotsResponse = client.GetAsync(url.Scheme + "://" + url.Host + "/robots.txt").Result;
            if (robotsResponse.IsSuccessStatusCode) {

                RobotsTXT robotsTxt = new RobotsTXT(robotsResponse.Content.ReadAsStringAsync().Result);
                foreach (Uri allowed in robotsTxt.allowed) {

                    if (allowed.IsAbsoluteUri) {

                        addPage(new Page(allowed, this));

                    } else {

                        addPage(new Page(new Uri(url, allowed), this));

                    }

                }
                if (evil) {

                    foreach (Uri disallowed in robotsTxt.disallowed) {

                        if (disallowed.IsAbsoluteUri) {

                            addPage(new Page(disallowed, this));

                        } else {

                            addPage(new Page(new Uri(url, disallowed), this));

                        }

                    }

                }

            }

            // Now scan all found pages and keep going as they are found, checking cookies for flags too

            addPage(new Page(url, this));
            for (int i = 0; i < maps.Count; i++) {

                Page page = maps[i];
                if (!page.hasCrawled) {

                    addPages(page.crawl());
                    if (!foundCookieFlag) {

                        Cookie found = checkCookiesForFlag();
                        if (found != null) {

                            flagCount++;
                            onCookieFlagFound?.Invoke(found);

                        }

                    }

                }

            }

        }

    }

}
