using System.Reflection;
using Autofac;
using Autofac.Core;
using Autofac.Extras.DynamicProxy;
using Nobisoft.Core.Lifetimes;

// ReSharper disable All

namespace Nobisoft.Core.DI;

public static class DependencyInjection
{
    /// <summary>
    /// Register the dependencies with the container
    /// </summary>
    /// <param name="builder">Container builder</param>
    /// <typeparam name="T">Module Type</typeparam>
    /// <returns>Container builder</returns>
    public static ContainerBuilder AutoRegisterModules<T>(this ContainerBuilder builder) where T : IModule
    {
        // Scan dll file in the location of type T
        var source1 = Directory.EnumerateFiles(Path.GetDirectoryName(typeof(T).Assembly.Location) ?? throw new SystemException($"No dll found {nameof(AutoRegisterModules)}", null), "*.dll", SearchOption.TopDirectoryOnly)
            .Select(Assembly.LoadFrom)
            .SelectMany(x => x.GetExportedTypes());

        // Find all assemblies of type IModule
        var assemblies = source1.Where(type => typeof(IModule).IsAssignableFrom(type) && type.IsClass && !type.IsAbstract && type.IsPublic)
            .Select(type => type.Assembly)
            .Distinct()
            .ToArray();
        // Register all types in those assemblies https://autofac.readthedocs.io/en/latest/register/scanning.html
        builder.RegisterAssemblyModules(assemblies);

        // Find all assemblies which contain concrete class of ITransientDependency, IScopedDependency and ISingletonDependency
        var source2 = source1.Where(type => (typeof(ITransientDependency).IsAssignableFrom(type) || typeof(IScopedDependency).IsAssignableFrom(type) || typeof(ISingletonDependency).IsAssignableFrom(type)) && type.IsPublic);
        var array2 = source2.Select(type => type.Assembly).Distinct().ToArray();

        // Find the implementations of interfaces in the assembly then register them as theirs type like transient, scoped or singleton. It also enables the interceptor 
        builder.RegisterAssemblyTypes(array2).AssignableTo<ITransientDependency>().AsImplementedInterfaces().InstancePerDependency().EnableInterfaceInterceptors();
        builder.RegisterAssemblyTypes(array2).AssignableTo<IScopedDependency>().AsImplementedInterfaces().InstancePerLifetimeScope().EnableInterfaceInterceptors();
        builder.RegisterAssemblyTypes(array2).AssignableTo<ISingletonDependency>().AsImplementedInterfaces().SingleInstance().EnableInterfaceInterceptors();

        // Filter out the generic types to register as generic in AutoFac
        var genericTypes = source1.Where(type =>
        {
            if (!type.IsGenericTypeDefinition || !type.IsClass)
                return false;
            return typeof(ITransientDependency).IsAssignableFrom(type) || typeof(IScopedDependency).IsAssignableFrom(type) || typeof(ISingletonDependency).IsAssignableFrom(type);
        });
        // Filter out general class to register as general in AutoFac
        var generalTypes = source1.Where(type =>
        {
            if (!type.IsClass || type.IsGenericTypeDefinition || type.IsGenericType)
                return false;
            return typeof(ITransientDependency).IsAssignableFrom(type) || typeof(IScopedDependency).IsAssignableFrom(type) || typeof(ISingletonDependency).IsAssignableFrom(type);
        });
        // Register generic type
        foreach (var genericType in genericTypes)
        {
            var genericTypeInterface = GetGenericInterface(genericType);
            if (typeof(ITransientDependency).IsAssignableFrom(genericType))
                builder.RegisterGeneric(genericType).As(genericTypeInterface).InstancePerDependency();
            if (typeof(IScopedDependency).IsAssignableFrom(genericType))
                builder.RegisterGeneric(genericType).As(genericTypeInterface).InstancePerLifetimeScope();
            if (typeof(ISingletonDependency).IsAssignableFrom(genericType))
                builder.RegisterGeneric(genericType).As(genericTypeInterface).SingleInstance();
        }

        // Register general type
        foreach (var generalType in generalTypes)
        {
            // var genericTypeInterface = GetGeneralInterface(generalType);
            // if (typeof(ITransientDependency).IsAssignableFrom(generalType))
            //     builder.RegisterType(generalType).As(genericTypeInterface).InstancePerDependency();
            // if (typeof(IScopedDependency).IsAssignableFrom(generalType))
            //     builder.RegisterType(generalType).As(genericTypeInterface).InstancePerLifetimeScope();
            // if (typeof(ISingletonDependency).IsAssignableFrom(generalType))
            //     builder.RegisterType(generalType).As(genericTypeInterface).SingleInstance();
            var genericTypeInterface = GetGeneralInterface(generalType);
            if (genericTypeInterface == null)
                continue;
            if (typeof(ITransientDependency).IsAssignableFrom(generalType))
            {
                // Register as transient instance if generic type interface is ITransientDependency
                if (genericTypeInterface.FullName!.Equals(typeof(ITransientDependency).FullName))
                {
                    builder.RegisterType(generalType).InstancePerDependency();
                    continue;
                }

                // If not, register as transient instance with that interface
                builder.RegisterType(generalType).As(genericTypeInterface).InstancePerDependency();
                continue;
            }

            if (typeof(IScopedDependency).IsAssignableFrom(generalType))
            {
                if (genericTypeInterface.FullName!.Equals(typeof(IScopedDependency).FullName))
                {
                    builder.RegisterType(generalType).InstancePerLifetimeScope();
                    continue;
                }

                builder.RegisterType(generalType).As(genericTypeInterface).InstancePerLifetimeScope();
                continue;
            }

            if (typeof(ISingletonDependency).IsAssignableFrom(generalType))
            {
                if (genericTypeInterface.FullName!.Equals(typeof(ISingletonDependency).FullName))
                {
                    builder.RegisterType(generalType).SingleInstance();
                    continue;
                }

                builder.RegisterType(generalType).As(genericTypeInterface).SingleInstance();
            }
        }

        return builder;
    }

    /// <summary>
    /// Get generic interface of the given type
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private static Type GetGenericInterface(Type type)
    {
        return type.GetInterfaces().FirstOrDefault(i => i.IsGenericType) ?? throw new InvalidOperationException();
    }

    /// <summary>
    /// Get general interface of the given type
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private static Type GetGeneralInterface(Type type)
    {
        //return type.GetInterfaces().FirstOrDefault(i => i.IsInterface && i) ?? throw new InvalidOperationException();
        if (type.BaseType == null)
            return type.GetInterfaces().FirstOrDefault() ?? throw new InvalidOperationException();
        else
            // Only get directly implemented interface, not all hierarchy 
            return type.GetInterfaces().Except(type.BaseType.GetInterfaces()).FirstOrDefault() ?? throw new InvalidOperationException();
    }

    /// <summary>
    /// Register the dependencies with the container from an assembly
    /// </summary>
    /// <param name="builder">Container builder</param>
    /// <param name="assembly"></param>
    /// <returns>Container builder</returns>
    public static ContainerBuilder AutoRegisterModulesFromAssembly(this ContainerBuilder builder, Assembly assembly)
    {
        // Scan dll file in the location of type T
        var source1 = Directory.EnumerateFiles(Path.GetDirectoryName(assembly.Location) ?? throw new SystemException($"No dll found {nameof(AutoRegisterModules)}", null), "*.dll", SearchOption.TopDirectoryOnly)
            .Select(Assembly.LoadFrom)
            .SelectMany(x => x.GetExportedTypes());

        // Find all assemblies of type IModule
        var assemblies = source1.Where(type => typeof(IModule).IsAssignableFrom(type) && type.IsClass && !type.IsAbstract && type.IsPublic)
            .Select(type => type.Assembly)
            .Distinct()
            .ToArray();
        // Register all types in those assemblies https://autofac.readthedocs.io/en/latest/register/scanning.html
        builder.RegisterAssemblyModules(assemblies);

        // Find all assemblies which contain concrete class of ITransientDependency, IScopedDependency and ISingletonDependency
        var source2 = source1.Where(type => (typeof(ITransientDependency).IsAssignableFrom(type) || typeof(IScopedDependency).IsAssignableFrom(type) || typeof(ISingletonDependency).IsAssignableFrom(type)) && type.IsPublic);
        var array2 = source2.Select(type => type.Assembly).Distinct().ToArray();

        // Find the implementations of interfaces in the assembly then register them as theirs type like transient, scoped or singleton. It also enables the interceptor 
        builder.RegisterAssemblyTypes(array2).AssignableTo<ITransientDependency>().AsImplementedInterfaces().InstancePerDependency().EnableInterfaceInterceptors();
        builder.RegisterAssemblyTypes(array2).AssignableTo<IScopedDependency>().AsImplementedInterfaces().InstancePerLifetimeScope().EnableInterfaceInterceptors();
        builder.RegisterAssemblyTypes(array2).AssignableTo<ISingletonDependency>().AsImplementedInterfaces().SingleInstance().EnableInterfaceInterceptors();

        // Filter out the generic types to register as generic in AutoFac
        var genericTypes = source1.Where(type =>
        {
            if (!type.IsGenericTypeDefinition || !type.IsClass)
                return false;
            return typeof(ITransientDependency).IsAssignableFrom(type) || typeof(IScopedDependency).IsAssignableFrom(type) || typeof(ISingletonDependency).IsAssignableFrom(type);
        });
        // Filter out general class to register as general in AutoFac
        var generalTypes = source1.Where(type =>
        {
            if (!type.IsClass || type.IsGenericTypeDefinition || type.IsGenericType)
                return false;
            return typeof(ITransientDependency).IsAssignableFrom(type) || typeof(IScopedDependency).IsAssignableFrom(type) || typeof(ISingletonDependency).IsAssignableFrom(type);
        });
        foreach (var genericType in genericTypes)
        {
            var genericTypeInterface = GetGenericInterface(genericType);
            if (typeof(ITransientDependency).IsAssignableFrom(genericType))
                builder.RegisterGeneric(genericType).As(genericTypeInterface).InstancePerDependency();
            if (typeof(IScopedDependency).IsAssignableFrom(genericType))
                builder.RegisterGeneric(genericType).As(genericTypeInterface).InstancePerLifetimeScope();
            if (typeof(ISingletonDependency).IsAssignableFrom(genericType))
                builder.RegisterGeneric(genericType).As(genericTypeInterface).SingleInstance();
        }

        foreach (var generalType in generalTypes)
        {
            var genericTypeInterface = GetGeneralInterface(generalType);
            if (genericTypeInterface == null)
                continue;
            if (typeof(ITransientDependency).IsAssignableFrom(generalType))
            {
                if (genericTypeInterface.FullName!.Equals(typeof(ITransientDependency).FullName))
                {
                    builder.RegisterType(generalType).InstancePerDependency();
                    continue;
                }

                builder.RegisterType(generalType).As(genericTypeInterface).InstancePerDependency();
                continue;
            }

            if (typeof(IScopedDependency).IsAssignableFrom(generalType))
            {
                if (genericTypeInterface.FullName!.Equals(typeof(IScopedDependency).FullName))
                {
                    builder.RegisterType(generalType).InstancePerLifetimeScope();
                    continue;
                }

                builder.RegisterType(generalType).As(genericTypeInterface).InstancePerLifetimeScope();
                continue;
            }

            if (typeof(ISingletonDependency).IsAssignableFrom(generalType))
            {
                if (genericTypeInterface.FullName!.Equals(typeof(ISingletonDependency).FullName))
                {
                    builder.RegisterType(generalType).SingleInstance();
                    continue;
                }

                builder.RegisterType(generalType).As(genericTypeInterface).SingleInstance();
            }
        }

        return builder;
    }
}