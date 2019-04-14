// -----------------------------------------------------------------------
//  <copyright file="SubverbHelper.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Startup.Cli
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Collections;
    using CommandLine;

    static class SubverbHelper
    {
        public static HierarchyList<VerbAttribute> Get()
        {
            var allVerbs = AppDomain.CurrentDomain
                                    .GetAssemblies()
                                    .SelectMany(a => a.GetTypes())
                                    .Where(a => a.GetCustomAttribute<VerbAttribute>() != null)
                                    .ToArray();

            var sub = new List<VerbAttribute>();

            foreach (var verb in allVerbs)
            {
                var subVerbs = verb.GetCustomAttributes<SubVerbAttribute>();

                foreach (var subVerb in subVerbs)
                {
                    var ss = allVerbs.FirstOrDefault(a => a == subVerb.Type);

                    if (ss != null)
                        sub.Add(ss.GetCustomAttribute<VerbAttribute>());
                }
            }

            var rootVerbs = allVerbs.Select(a => a.GetCustomAttribute<VerbAttribute>()).Except(sub);

            var result = new HierarchyList<VerbAttribute>(new VerbAttribute(name: "AppRootVerb"));

            foreach (var verbAttribute in rootVerbs)
                result.Root.AddChildren(verbAttribute);

            return result;
        }
    }
}