using System;
using System.Collections.Generic;
using System.IO;

namespace ctfmap {

    public class RobotsTXT {

        public List<String> userAgents { get; }
        public bool containsSitemap { get; }
        public List<Uri> allowed { get; }
        public List<Uri> disallowed { get; }

        public RobotsTXT(String robotsTxt) {

            allowed = new List<Uri>();
            disallowed = new List<Uri>();
            userAgents = new List<String>();
            StringReader reader = new StringReader(robotsTxt);
            String line;
            while ((line = reader.ReadLine()) != null) {

                if (line.StartsWith("#")) {

                    continue;

                } else if (line.ToLower().StartsWith("user-agent:")) {

                    userAgents.Add(line.Substring(12));

                } else if (line.ToLower().StartsWith("allow:")) {

                    allowed.Add(new Uri(line.Substring(7).Trim(), UriKind.RelativeOrAbsolute));

                } else if (line.ToLower().StartsWith("disallow:")) {

                    disallowed.Add(new Uri(line.Substring(9).Trim(), UriKind.RelativeOrAbsolute));

                } else if (line.ToLower().StartsWith("sitemap:")) {

                    containsSitemap = true;

                }

            }

        }

    }

}
