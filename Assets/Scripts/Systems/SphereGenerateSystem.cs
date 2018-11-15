using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;

public class SphereGenerateSystem : JobComponentSystem {
    public static MeshInstanceRenderer sphereLook;
    public static EntityArchetype sphereArchetype;

    private ComponentGroup sphereGroup;
    private ComponentGroup sphereRequestsGroup;

    bool initialized = false;

    protected override JobHandle OnUpdate(JobHandle inputDeps) {
        if (!initialized) {
            Initialize();
            initialized = true;
        }
        return inputDeps;
    }

    public struct SphereGenerateJob : IJobParallelFor {
        public EntityCommandBuffer.Concurrent commandBuffer;

        [ReadOnly]
        public ComponentDataArray<SphereRequest> requests;

        public void Execute(int index) {
            commandBuffer.CreateEntity(index, sphereArchetype);
            commandBuffer.SetComponent(index, new Scale {
                Value = new float3(1.0f, 1.0f, 1.0f)
            });
            commandBuffer.SetComponent(index, new Position {
                Value = requests[index].origin
            });
            commandBuffer.AddSharedComponent(index, sphereLook);
        }
    }

    private void Initialize() {
        EntityManager entityManager = World.Active.GetOrCreateManager<EntityManager>();

        sphereLook = Bootstrap.GetLookFromPrototype("SpherePrototype");
        sphereArchetype = entityManager.CreateArchetype(typeof(Sphere), typeof(Position), typeof(Scale));
    }
}
