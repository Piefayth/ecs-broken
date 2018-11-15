using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;

public class SphereGenerateBarrier : BarrierSystem { }
public class SphereCleanupBarrier : BarrierSystem { }

[UpdateAfter(typeof(SphereGenerateSystem))]
public class SphereOrchestrationSystem : JobComponentSystem {
    private ComponentGroup sphereRequestsGroup;
    private SphereGenerateBarrier generateBarrier;
    private SphereCleanupBarrier cleanupBarrier;

    bool initialized = false;

    protected override JobHandle OnUpdate(JobHandle inputDeps) {
        if (!initialized) {
            Initialize();
            initialized = true;
        }

        sphereRequestsGroup = GetComponentGroup(typeof(SphereRequest));
        if (sphereRequestsGroup.CalculateLength() == 0) {
            return inputDeps;
        }

        ComponentDataArray<SphereRequest> sphereRequests = sphereRequestsGroup.GetComponentDataArray<SphereRequest>();
        JobHandle sgj = new SphereGenerateSystem.SphereGenerateJob {
            requests = sphereRequests,
            commandBuffer = generateBarrier.CreateCommandBuffer().ToConcurrent(),
        }.Schedule(sphereRequests.Length, 64, inputDeps);

        EntityArray sphereRequestEntities = sphereRequestsGroup.GetEntityArray();
        JobHandle srcj = new SphereRequestCleanupJob {
            entities = sphereRequestEntities,
            commandBuffer = cleanupBarrier.CreateCommandBuffer().ToConcurrent(),
        }.Schedule(sphereRequestEntities.Length, 64, sgj);

        return srcj;
    }

    public struct SphereRequestCleanupJob : IJobParallelFor {
        public EntityCommandBuffer.Concurrent commandBuffer;

        [ReadOnly]
        public EntityArray entities;

        public void Execute(int index) {
            commandBuffer.DestroyEntity(index, entities[index]);
        }
    }

    private void Initialize() {
        generateBarrier = World.Active.GetOrCreateManager<SphereGenerateBarrier>();
        cleanupBarrier = World.Active.GetOrCreateManager<SphereCleanupBarrier>();
    }
}
