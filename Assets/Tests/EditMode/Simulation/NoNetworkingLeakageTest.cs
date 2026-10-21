using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using LighthouseKeepers.Core.Simulation;
using NUnit.Framework;

namespace LighthouseKeepers.Tests.Simulation
{
    public sealed class NoNetworkingLeakageTest
    {
        static readonly string[] NetworkingNamespaces =
        {
            "Fusion", "Photon", "Unity.Netcode", "Unity.Networking", "UnityEngine.Networking", "System.Net",
            "LighthouseKeepers.Net", "LighthouseKeepers.Networking", "LighthouseKeepers.StormWatchNetworking"
        };

        static bool InNamespace(string value, string prefix) => value != null &&
            (value == prefix || value.StartsWith(prefix + ".", StringComparison.Ordinal));
        static bool NetworkingNamespace(string value) => NetworkingNamespaces.Any(p => InNamespace(value, p));
        static bool NetworkingAssembly(string value) => NetworkingNamespace(value) ||
            InNamespace(value, "LK.Net") || InNamespace(value, "LK.Networking") || InNamespace(value, "LK.StormWatch.Networking");
        static readonly Dictionary<short, OpCode> Opcodes = typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.FieldType == typeof(OpCode)).Select(f => (OpCode)f.GetValue(null)).ToDictionary(op => op.Value);

        static void InspectMethodBody(MethodBase method, Action<Type> inspect)
        {
            var body = method.GetMethodBody();
            if (body == null) return;
            foreach (var local in body.LocalVariables) inspect(local.LocalType);
            foreach (var handler in body.ExceptionHandlingClauses)
                if (handler.Flags == ExceptionHandlingClauseOptions.Clause) inspect(handler.CatchType);
            byte[] il = body.GetILAsByteArray();
            Type[] typeArguments = method.DeclaringType.IsGenericType ? method.DeclaringType.GetGenericArguments() : Type.EmptyTypes;
            Type[] methodArguments = method.IsGenericMethod ? method.GetGenericArguments() : Type.EmptyTypes;
            for (int offset = 0; offset < il.Length;)
            {
                short code = il[offset++];
                if (code == 0xfe) code = unchecked((short)(0xfe00 | il[offset++]));
                OpCode opcode = Opcodes[code];
                var operand = opcode.OperandType;
                if (operand == OperandType.InlineMethod || operand == OperandType.InlineField ||
                    operand == OperandType.InlineType || operand == OperandType.InlineTok)
                {
                    MemberInfo member = method.Module.ResolveMember(BitConverter.ToInt32(il, offset), typeArguments, methodArguments);
                    inspect(member as Type); inspect(member.DeclaringType);
                    if (member is FieldInfo field) inspect(field.FieldType);
                    if (member is MethodInfo called)
                    {
                        inspect(called.ReturnType);
                        foreach (Type argument in called.GetGenericArguments()) inspect(argument);
                    }
                    if (member is MethodBase callable)
                        foreach (ParameterInfo parameter in callable.GetParameters()) inspect(parameter.ParameterType);
                }
                switch (operand)
                {
                    case OperandType.InlineNone: break;
                    case OperandType.ShortInlineBrTarget:
                    case OperandType.ShortInlineI:
                    case OperandType.ShortInlineVar: offset++; break;
                    case OperandType.InlineVar: offset += 2; break;
                    case OperandType.InlineI8:
                    case OperandType.InlineR: offset += 8; break;
                    case OperandType.InlineSwitch: offset += 4 + 4 * BitConverter.ToInt32(il, offset); break;
                    case OperandType.InlineBrTarget:
                    case OperandType.InlineField:
                    case OperandType.InlineI:
                    case OperandType.InlineMethod:
                    case OperandType.InlineSig:
                    case OperandType.InlineString:
                    case OperandType.InlineTok:
                    case OperandType.InlineType:
                    case OperandType.ShortInlineR: offset += 4; break;
                    default: throw new NotSupportedException("Unrecognized IL operand in " + method);
                }
            }
        }

        static bool GameAssembly(Assembly assembly)
        {
            string name = assembly.GetName().Name;
            // These are the project's runtime assembly naming families. SDK/framework
            // assemblies implement networking; Editor/test tooling is not a game layer.
            bool project = name == "Assembly-CSharp" || name.StartsWith("LK.", StringComparison.Ordinal) ||
                name.StartsWith("LighthouseKeepers", StringComparison.Ordinal);
            return project && name != "LK.Editor" && !name.StartsWith("LK.Editor.", StringComparison.Ordinal) &&
                !name.EndsWith(".Editor", StringComparison.Ordinal) && !name.EndsWith("Tests", StringComparison.Ordinal) &&
                !name.Contains(".Tests.");
        }

        /// <summary>Reflection-only scanner. No Unity compilation pipeline, source paths,
        /// networking SDK or legacy-assembly exemptions are needed.</summary>
        public static IReadOnlyList<string> FindViolations(IEnumerable<Assembly> assemblies)
        {
            var violations = new HashSet<string>(StringComparer.Ordinal);
            foreach (Assembly assembly in assemblies)
            {
                string name = assembly.GetName().Name;
                if (name == "LK.Net") continue;
                foreach (AssemblyName reference in assembly.GetReferencedAssemblies())
                    if (NetworkingAssembly(reference.Name))
                        violations.Add(name + " -> assembly " + reference.Name);
                var visited = new HashSet<Type>();
                Action<Type> inspect = null;
                inspect = type =>
                {
                    if (type == null || !visited.Add(type)) return;
                    if (NetworkingNamespace(type.Namespace)) violations.Add(name + " -> type " + type.FullName);
                    if (type.HasElementType) inspect(type.GetElementType());
                    if (type.IsGenericType)
                        foreach (Type argument in type.GetGenericArguments()) inspect(argument);
                    if (type.IsGenericParameter)
                        foreach (Type constraint in type.GetGenericParameterConstraints()) inspect(constraint);
                };
                // A type-load failure deliberately fails rather than hiding unreadable references.
                foreach (Type type in assembly.GetTypes())
                {
                    inspect(type); inspect(type.BaseType);
                    foreach (Type contract in type.GetInterfaces()) inspect(contract);
                    foreach (MemberInfo member in type.GetMembers(BindingFlags.DeclaredOnly | BindingFlags.Public |
                        BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
                    {
                        if (member is FieldInfo field) inspect(field.FieldType);
                        if (member is PropertyInfo property) inspect(property.PropertyType);
                        if (member is EventInfo signal) inspect(signal.EventHandlerType);
                        if (member is MethodInfo method) inspect(method.ReturnType);
                        if (member is MethodBase callable)
                        {
                            foreach (ParameterInfo parameter in callable.GetParameters()) inspect(parameter.ParameterType);
                            if (callable.IsGenericMethod)
                                foreach (Type argument in callable.GetGenericArguments()) inspect(argument);
                            InspectMethodBody(callable, inspect);
                        }
                    }
                }
            }
            return violations.OrderBy(v => v, StringComparer.Ordinal).ToArray();
        }

        [Test] public void SimulationCoreHasNoUnityOrNetworkingAssemblyReferences()
        {
            var forbidden = typeof(SimulationHost).Assembly.GetReferencedAssemblies()
                .Where(a => a.Name.StartsWith("Unity", StringComparison.Ordinal) || NetworkingAssembly(a.Name));
            Assert.That(forbidden, Is.Empty);
        }

        [Test] public void OnlyLKNetMayReferenceNetworkingNamespaces()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(GameAssembly).ToArray();
            Assert.That(assemblies, Does.Contain(typeof(SimulationHost).Assembly), "The scan must include the runtime core.");
            Console.WriteLine("Networking boundary / loaded game assemblies: " +
                string.Join(", ", assemblies.Select(a => a.GetName().Name).OrderBy(n => n, StringComparer.Ordinal)));
            var violations = FindViolations(assemblies);
            Assert.That(violations, Is.Empty, "Networking belongs only in LK.Net; legacy game assemblies are not exempt:\n" +
                string.Join("\n", violations));
        }

        static Type NetworkProbe()
        {
            var assembly = AssemblyBuilder.DefineDynamicAssembly(
                new AssemblyName("Fusion.BoundaryProbe" + Guid.NewGuid().ToString("N")), AssemblyBuilderAccess.Run);
            return assembly.DefineDynamicModule("Fixture").DefineType("Fusion.Probe", TypeAttributes.Public).CreateType();
        }

        static Assembly GameProbe(string name, Type networkType, bool methodBodyOnly = false)
        {
            var assembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(name), AssemblyBuilderAccess.Run);
            var type = assembly.DefineDynamicModule("Fixture").DefineType("Game.Probe", TypeAttributes.Public);
            if (methodBodyOnly)
            {
                var il = type.DefineMethod("Connect", MethodAttributes.Public | MethodAttributes.Static,
                    typeof(void), Type.EmptyTypes).GetILGenerator();
                il.Emit(OpCodes.Newobj, networkType.GetConstructor(Type.EmptyTypes));
                il.Emit(OpCodes.Pop); il.Emit(OpCodes.Ret);
            }
            else type.DefineField("Connection", typeof(List<>).MakeGenericType(networkType), FieldAttributes.Public);
            Type created = type.CreateType();
            return created.Assembly;
        }

        [Test] public void ScannerDetectsNetworkingInsideGenericMemberSignatures()
        {
            var assembly = GameProbe("BoundaryProbe.Generic", NetworkProbe());
            Assert.That(FindViolations(new[] { assembly }), Has.Some.Contains("Fusion.Probe"));
        }

        [Test] public void ScannerDetectsNetworkingUsedOnlyInMethodBodies()
        {
            var assembly = GameProbe("BoundaryProbe.MethodBody", NetworkProbe(), methodBodyOnly: true);
            Assert.That(FindViolations(new[] { assembly }), Has.Some.Contains("Fusion.Probe"));
        }

        [Test] public void OnlyExactLKNetAssemblyIsAllowed()
        {
            Type network = NetworkProbe();
            Assert.That(FindViolations(new[] { GameProbe("LK.Net", network) }), Is.Empty);
            Assert.That(FindViolations(new[] { GameProbe("BoundaryProbe.LK.Net", network) }), Is.Not.Empty);
        }

        [Test] public void ScannerDetectsNetworkingNamespaceDeclaredInsideGameAssembly()
        {
            foreach (string networkingNamespace in NetworkingNamespaces)
            {
                var assembly = AssemblyBuilder.DefineDynamicAssembly(
                    new AssemblyName("BoundaryProbe.OwnNamespace" + Guid.NewGuid().ToString("N")), AssemblyBuilderAccess.Run);
                string type = networkingNamespace + ".AccidentalAdapter";
                assembly.DefineDynamicModule("Fixture").DefineType(type, TypeAttributes.Public).CreateType();
                Assert.That(FindViolations(new[] { assembly }), Has.Some.Contains(type));
            }
        }
    }
}
