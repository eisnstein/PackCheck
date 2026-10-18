using Spectre.Console.Cli;

namespace PackCheck.Tests.Fakes;

/// <summary>
/// Minimal type registrar for tests. Unlike Spectre's FakeTypeRegistrar it
/// creates types through their constructors, resolving the parameters from
/// the registrations, so commands with injected services can be tested.
/// </summary>
public class TestTypeRegistrar : ITypeRegistrar
{
    private readonly Dictionary<Type, List<Func<TestTypeResolver, object?>>> _registrations = new();

    public void Register(Type service, Type implementation)
    {
        Add(service, resolver => resolver.Create(implementation));
    }

    public void RegisterInstance(Type service, object implementation)
    {
        Add(service, _ => implementation);
    }

    public void RegisterLazy(Type service, Func<object> factory)
    {
        Add(service, _ => factory());
    }

    public ITypeResolver Build()
    {
        return new TestTypeResolver(_registrations);
    }

    private void Add(Type service, Func<TestTypeResolver, object?> factory)
    {
        if (!_registrations.TryGetValue(service, out var factories))
        {
            factories = [];
            _registrations[service] = factories;
        }

        factories.Add(factory);
    }
}

public class TestTypeResolver(Dictionary<Type, List<Func<TestTypeResolver, object?>>> registrations) : ITypeResolver
{
    public object? Resolve(Type? type)
    {
        if (type is null)
        {
            return null;
        }

        // Spectre resolves e.g. IEnumerable<ICommandInterceptor> to get all registrations of a service
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
        {
            var elementType = type.GetGenericArguments()[0];
            var instances = registrations.TryGetValue(elementType, out var all)
                ? all.Select(factory => factory(this)).Where(i => i is not null).ToList()
                : [];

            var array = Array.CreateInstance(elementType, instances.Count);
            for (var i = 0; i < instances.Count; i++)
            {
                array.SetValue(instances[i], i);
            }

            return array;
        }

        if (registrations.TryGetValue(type, out var factories))
        {
            return factories[^1](this);
        }

        return type.IsAbstract || type.IsInterface ? null : Create(type);
    }

    public object? Create(Type type)
    {
        // Try the constructor with the most parameters first, like a DI container would.
        foreach (var constructor in type.GetConstructors().OrderByDescending(c => c.GetParameters().Length))
        {
            var arguments = constructor.GetParameters().Select(p => Resolve(p.ParameterType)).ToArray();
            if (arguments.All(a => a is not null))
            {
                return constructor.Invoke(arguments);
            }
        }

        return null;
    }
}
