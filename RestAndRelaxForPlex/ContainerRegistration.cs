using System.Collections.Generic;
using Autofac;
using Autofac.Core;
using JimBobBennett.JimLib.Network;
using JimBobBennett.RestAndRelaxForPlex.Caches;
using JimBobBennett.RestAndRelaxForPlex.Connection;

namespace JimBobBennett.RestAndRelaxForPlex
{
    public static class ContainerRegistration
    {
        public static void OnInitialize(ContainerBuilder builder, string tmdbApiKey)
        {
            builder.RegisterType<TvdbConnection>().As<ITvdbConnection>().SingleInstance();
            builder.RegisterType<TmdbConnection>().As<ITmdbConnection>().WithParameters(new List<Parameter>
            {
                new ResolvedParameter((p, c) => p.Position == 0, (p, c) => c.Resolve<IRestConnection>()),
                new PositionalParameter(1, tmdbApiKey)
            }).SingleInstance();

            builder.RegisterType<TvdbCache>().As<ITvdbCache>().SingleInstance();
            builder.RegisterType<TmdbCache>().As<ITmdbCache>().SingleInstance();

            builder.RegisterType<MyPlexConnection>().As<IMyPlexConnection>().SingleInstance();
            builder.RegisterType<NowPlaying>().As<INowPlaying>().SingleInstance();
            builder.RegisterType<ConnectionManager>().As<IConnectionManager>().SingleInstance();
        }
    }
}
