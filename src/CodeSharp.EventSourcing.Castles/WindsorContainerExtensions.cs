using System;
using System.Linq;
using System.Reflection;
using Castle.MicroKernel.Registration;
using Castle.Windsor;

namespace CodeSharp.EventSourcing.Castles
{
    /// <summary>
    /// WindsorContainer扩展
    /// </summary>
    public static class WindsorContainerExtensions
    {
        /// <summary>
        /// 注册一个指定的类型及其接口
        /// </summary>
        public static IWindsorContainer RegisterType(this IWindsorContainer container, Type type)
        {
            //生命周期
            var life = ParseLife(type);
            //实现注册
            var typeKey = type.FullName;
            if (!container.Kernel.HasComponent(typeKey))
            {
                container.Register(Component.For(type).Named(typeKey).Life(life));
            }
            //接口注册
            foreach (var interfaceType in type.GetInterfaces())
            {
                var key = interfaceType.FullName + "#" + type.FullName;
                if (!container.Kernel.HasComponent(key))
                {
                    container.Register(Component.For(interfaceType).ImplementedBy(type).Named(key).Life(life));
                }
            }

            return container;
        }
        /// <summary>
        /// 注册符合条件的多个类型及其接口
        /// </summary>
        public static IWindsorContainer RegisterTypes(this IWindsorContainer container, Func<Type, bool> typePredicate, params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetExportedTypes().Where(x => typePredicate(x)))
                {
                    container.RegisterType(type);
                }
            }
            return container;
        }
        /// <summary>
        /// 设置生命周期
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="registration"></param>
        /// <param name="life"></param>
        /// <returns></returns>
        public static ComponentRegistration<T> Life<T>(this ComponentRegistration<T> registration, LifeStyle life) where T : class
        {
            if (life == LifeStyle.Singleton)
            {
                return registration.LifeStyle.Singleton;
            }
            return registration.LifeStyle.Transient;
        }

        private static LifeStyle ParseLife(Type type)
        {
            var componentAttributes = type.GetCustomAttributes(typeof(ComponentAttribute), false);
            return componentAttributes.Count() <= 0 ? LifeStyle.Transient : (componentAttributes[0] as ComponentAttribute).LifeStyle;
        }
    }
}