//Copyright (c) CodeSharp.  All rights reserved.

using System;
using System.Collections.Generic;
using Castle.Core;
using Castle.Core.Configuration;
using Castle.MicroKernel;
using Castle.MicroKernel.Context;
using Castle.MicroKernel.Facilities;
using Castle.MicroKernel.Registration;
using Castle.Windsor;

namespace CodeSharp.EventSourcing.Castles
{
    /// <summary>
    /// 基于Castle容器实现的IoC依赖解析器
    /// </summary>
    public class WindsorContainerResolver : IDependencyResolver
    {
        private IWindsorContainer _container;

        public WindsorContainerResolver(IWindsorContainer container)
        {
            _container = container;
        }

        /// <summary>
        /// 返回Castle容器
        /// </summary>
        public virtual IWindsorContainer Container
        {
            get
            {
                return _container;
            }
        }

        #region IDependencyResolver Members

        void IDependencyResolver.RegisterType(Type type)
        {
            _container.RegisterType(type);
        }
        void IDependencyResolver.RegisterTypes(Func<Type, bool> typePredicate, params System.Reflection.Assembly[] assemblies)
        {
            _container.RegisterTypes(typePredicate, assemblies);
        }
        void IDependencyResolver.Register<T>(T instance, LifeStyle life)
        {
            _container.Register(Component.For<T>().Instance(instance).Life(life));
        }
        bool IDependencyResolver.IsTypeRegistered(Type type)
        {
            return _container.Kernel.HasComponent(type);
        }
        T IDependencyResolver.Resolve<T>()
        {
            return (T)((IDependencyResolver)this).Resolve(typeof(T));
        }
        T IDependencyResolver.Resolve<T>(Type type)
        {
            return ((IDependencyResolver)this).Resolve<T>(type);
        }
        object IDependencyResolver.Resolve(Type type)
        {
            return _container.Kernel.HasComponent(type) ? _container.Resolve(type) : null;
        }

        #endregion
    }
}