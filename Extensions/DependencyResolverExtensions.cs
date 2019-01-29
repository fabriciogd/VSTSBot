using Autofac;
using Autofac.Features.Metadata;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VSTSBot.Extensions
{
    public static class DependencyResolverExtensions
    {
        public static IDialog<object> Find(this ILifetimeScope resolver, string activityText)
        {
            resolver.ThrowIfNull(nameof(resolver));
            activityText.ThrowIfNullOrWhiteSpace(nameof(activityText));

            var dialogs = resolver.GetServices<Meta<IDialog<object>>>();

            var dialog = dialogs?
                .FirstOrDefault(m => activityText.Trim().StartsWith(((Enum)m.Metadata["Dialog"]).GetDescription(), StringComparison.OrdinalIgnoreCase));

            return dialog?.Value;
        }

        public static IEnumerable<T> GetServices<T>(this ILifetimeScope resolver)
        {
            resolver.ThrowIfNull(nameof(resolver));

            return resolver.ResolveOptional<IEnumerable<T>>() as IEnumerable<T>;
        }
    }
}