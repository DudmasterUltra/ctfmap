using ex0dusCLI;
using System;
using System.Net;
using System.Web;

namespace ctfmap {

    public class Runner {

        public static void Main(String[] args) {

            KillSwitch.Validate("ctfmap");

            CLI cli = new CLI(new CLI.Switch[] {

                new CLI.Switch("url") { HasValue = true, IsRequired = true,
                    Description = "The url to map" },
                new CLI.Switch("evil") { HasValue = false, IsRequired = false,
                    Description = "Intentionally visit pages listed under robots.txt" },
                new CLI.Switch("cookie") { HasValue = true, IsRequired = false,
                    Description = "Cookie header to use" },
                new CLI.Switch("flag") { HasValue = true, IsRequired = true,
                    Description = "Keywords to search for, separated by semicolons" }

            });
            CLIResult parse = cli.Parse(args);

            if (parse.IsEmpty) {

                cli.Splash("ctfmap", "ex0dus");
                return;

            }

            Console.WriteLine(Properties.Resources.launch);

            Uri url;
            if (parse.ParsedContains("url")) {

                CLI.ResultSwitch parsedUrl = parse.GetParsed("url");
                if (parsedUrl.HasParsedValue) {

                    String u = parsedUrl.Value;
                    if (!u.StartsWith("http://") && !u.StartsWith("https://")) {

                        url = new Uri("http://" + u);

                    } else {

                        url = new Uri(u);

                    }

                } else {

                    Console.WriteLine("FAILURE = -url is missing a string value");
                    return;

                }

            } else {

                Console.WriteLine("FAILURE = -url argument missing");
                return;

            }

            bool evil = parse.ParsedContains("evil");

            String cookie = String.Empty;
            if (parse.ParsedContains("cookie")) {

                CLI.ResultSwitch parsedCookie = parse.GetParsed("cookie");
                if (parsedCookie.HasParsedValue) {

                    cookie = parsedCookie.Value;

                } else {

                    Console.WriteLine("FAILURE = -cookie is missing a string value");
                    return;

                }

            }

            String[] flags;
            if (parse.ParsedContains("flag")) {

                CLI.ResultSwitch parsedFlag = parse.GetParsed("flag");
                if (parsedFlag.HasParsedValue) {

                    flags = parsedFlag.Value.Split(';');

                } else {

                    Console.WriteLine("FAILURE = -flag is missing string value(s)");
                    return;

                }

            } else {

                Console.WriteLine("FAILURE = -flag argument missing");
                return;

            }

            Console.WriteLine("Beginning ctfmap of '" + url + "'...");
            PageManager manager = new PageManager(url, flags);
            manager.evil = evil;
            if (!String.IsNullOrEmpty(cookie)) {

                manager.setCookie(cookie);

            }

            Console.WriteLine("Checking for GitHub...");
            HttpStatusCode gitFolder = manager.gitFolderStatusCode();
            if (HttpStatusCode.Forbidden.Equals(gitFolder)) {

                Console.Write("Forbidden GitHub folder found");
                if (HttpStatusCode.OK.Equals(manager.gitFolderHeadStatusCode())) {

                    Console.WriteLine(", HEAD not forbidden");

                } else {

                    Console.WriteLine(", HEAD unreachable");

                }

            } else if (HttpStatusCode.OK.Equals(gitFolder)) {

                Console.WriteLine("GitHub folder found");

            } else {

                HttpStatusCode gitIgnore = manager.gitIgnoreStatusCode();
                if (HttpStatusCode.Forbidden.Equals(gitIgnore)) {

                    Console.WriteLine("Forbidden GitHub ignore found");

                } else if (HttpStatusCode.OK.Equals(gitIgnore)) {

                    Console.WriteLine("GitHub ignore found");

                }

            }

            manager.onPageCrawl += delegate (Page m) {
                
                Console.WriteLine("Page crawled: " + m.url);

            };
            manager.onFlagFound += delegate (Page m, String flag) {

                Console.WriteLine("Keyword '" + flag + "' found in: " + m.url);

            };
            manager.onCookieFlagFound += delegate (Cookie c) {

                Console.WriteLine("Keyword '" + manager.flags + "' found in cookie '" + c.Name + "': " + HttpUtility.UrlDecode(c.Value));

            };
            manager.scan();
            Console.WriteLine("Scan complete: " + manager.pageCount + " pages found, " + manager.flagCount + " flag keywords found");
            Console.ReadKey();

        }

    }

}
