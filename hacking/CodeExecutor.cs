using System.Reflection;
namespace sutnance.hacking;

public interface ICodeExecutor
{
    Task<object> RunFunctionAsync(string namespaceName, string className, string methodName, object[] arguments, string? assemblyName = null);
}
public class CodeExecutor(IServiceProvider sp) : ICodeExecutor
{
    public async Task<object> RunFunctionAsync(
        string namespaceName,
        string className,
        string methodName,
        object[] arguments,
        string? assemblyName = null)
    {
        string fullTypeName = $"{namespaceName}.{className}";

        Type? type;

        if (string.IsNullOrWhiteSpace(assemblyName))
        {
            type = AppDomain.CurrentDomain
                .GetAssemblies()
                .Select(a => a.GetType(fullTypeName))
                .FirstOrDefault(t => t != null);
        }
        else
        {
            Assembly assembly = Path.Exists(assemblyName)
                ? Assembly.LoadFrom(assemblyName)
                : Assembly.Load(assemblyName);

            type = assembly.GetType(fullTypeName);
        }

        if (type == null)
            throw new TypeLoadException(
                $"Type '{fullTypeName}' was not found."
            );

        MethodInfo? method = type
            .GetMethods(
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Static |
                BindingFlags.Instance)
            .FirstOrDefault(m =>
            {
                if (m.Name != methodName)
                    return false;

                var parameters = m.GetParameters();

                if (parameters.Length != arguments.Length)
                    return false;

                for (int i = 0; i < parameters.Length; i++)
                {
                    var argument = arguments[i];
                    var parameterType = parameters[i].ParameterType;

                    if (argument == null)
                    {
                        if (parameterType.IsValueType &&
                            Nullable.GetUnderlyingType(parameterType) == null)
                            return false;

                        continue;
                    }

                    if (!parameterType.IsAssignableFrom(argument.GetType()))
                        return false;
                }

                return true;
            });

        if (method == null)
            throw new MissingMethodException(
                $"Method '{methodName}' with {arguments.Length} arguments " +
                $"was not found on type '{fullTypeName}'."
            );

        object? instance = null;

        if (!method.IsStatic){
            Type t = Type.GetType(fullTypeName)!;
            // instance = Activator.CreateInstance(t);
            instance = sp.GetRequiredService(t)!;
        }

        object ret = method.Invoke(instance, arguments) ?? new Object();
        if (ret is Task task)
            {
                await task;

                var resultProperty = task.GetType().GetProperty("Result");

                return resultProperty?.GetValue(task);
            }
        return ret;
    }
}
