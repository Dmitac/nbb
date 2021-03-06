﻿using System;

namespace NBB.Core.Effects
{
    public static class Fun
    {
        public static T Identity<T>(T x) => x;

        public static Func<IEffect<T1>, IEffect<T2>> Bind<T1, T2>(Func<T1, IEffect<T2>> kFunc) => x => x.Bind(kFunc);

        public static Func<T1, T3> Compose<T1, T2, T3>(Func<T2, T3> g, Func<T1, T2> f) => x => g(f(x));

        public static Func<T1, IEffect<T3>> ComposeK<T1, T2, T3>(Func<T2, IEffect<T3>> g, Func<T1, IEffect<T2>> f) =>
            x => f(x).Bind(g);
    }
}
