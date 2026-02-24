using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Permissions;
using Unity.Burst;

[assembly: InternalsVisibleTo("Unity.Burst.CodeGen")]
[assembly: InternalsVisibleTo("Unity.Burst.Editor")]
[assembly: InternalsVisibleTo("Unity.Burst.Tests.UnitTests")]
[assembly: InternalsVisibleTo("Unity.Burst.Editor.Tests")]
[assembly: InternalsVisibleTo("Unity.Burst.Benchmarks")]
[assembly: BurstCompiler.StaticTypeReinit(typeof(BurstCompiler.BurstCompilerHelper.IsBurstEnabled_00000146_0024BurstDirectCall))]
[assembly: BurstCompiler.StaticTypeReinit(typeof(Unity_002EBurst_002EIntrinsics_002EDoSetCSRTrampoline_0000012A_0024BurstDirectCall))]
[assembly: BurstCompiler.StaticTypeReinit(typeof(Unity_002EBurst_002EIntrinsics_002EDoGetCSRTrampoline_0000012B_0024BurstDirectCall))]
[assembly: AssemblyVersion("0.0.0.0")]
