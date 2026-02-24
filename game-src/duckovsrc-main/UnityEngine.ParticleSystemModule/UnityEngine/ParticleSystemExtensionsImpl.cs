using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine.Bindings;

namespace UnityEngine;

internal class ParticleSystemExtensionsImpl
{
	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(Name = "ParticleSystemScriptBindings::GetSafeCollisionEventSize")]
	internal static extern int GetSafeCollisionEventSize([NotNull("ArgumentNullException")] ParticleSystem ps);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(Name = "ParticleSystemScriptBindings::GetCollisionEventsDeprecated")]
	internal static extern int GetCollisionEventsDeprecated([NotNull("ArgumentNullException")] ParticleSystem ps, GameObject go, [Out] ParticleCollisionEvent[] collisionEvents);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(Name = "ParticleSystemScriptBindings::GetSafeTriggerParticlesSize")]
	internal static extern int GetSafeTriggerParticlesSize([NotNull("ArgumentNullException")] ParticleSystem ps, int type);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(Name = "ParticleSystemScriptBindings::GetCollisionEvents")]
	internal static extern int GetCollisionEvents([NotNull("ArgumentNullException")] ParticleSystem ps, [NotNull("ArgumentNullException")] GameObject go, [NotNull("ArgumentNullException")] List<ParticleCollisionEvent> collisionEvents);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(Name = "ParticleSystemScriptBindings::GetTriggerParticles")]
	internal static extern int GetTriggerParticles([NotNull("ArgumentNullException")] ParticleSystem ps, int type, [NotNull("ArgumentNullException")] List<ParticleSystem.Particle> particles);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(Name = "ParticleSystemScriptBindings::GetTriggerParticlesWithData")]
	internal static extern int GetTriggerParticlesWithData([NotNull("ArgumentNullException")] ParticleSystem ps, int type, [NotNull("ArgumentNullException")] List<ParticleSystem.Particle> particles, ref ParticleSystem.ColliderData colliderData);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(Name = "ParticleSystemScriptBindings::SetTriggerParticles")]
	internal static extern void SetTriggerParticles([NotNull("ArgumentNullException")] ParticleSystem ps, int type, [NotNull("ArgumentNullException")] List<ParticleSystem.Particle> particles, int offset, int count);
}
