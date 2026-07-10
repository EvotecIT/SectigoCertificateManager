#if NET472
namespace System.Runtime.CompilerServices;

/// <summary>
/// Makes module initializers available to the test assembly on .NET Framework.
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
internal sealed class ModuleInitializerAttribute : Attribute {
}
#endif
