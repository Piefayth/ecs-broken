using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using Unity.Mathematics;

public class CubeSystem : ComponentSystem {
    public static MeshInstanceRenderer cubeLook;
    public static EntityArchetype cubeArchetype;

    bool initialized = false;

    protected override void OnUpdate() {
        if (!initialized) {
            Initialize();
            initialized = true;
        }
    }

    protected void Initialize() {
        EntityManager entityManager = World.Active.GetOrCreateManager<EntityManager>();

        cubeLook = Bootstrap.GetLookFromPrototype("GridRenderPrototype");
        cubeArchetype = entityManager.CreateArchetype(typeof(Cube), typeof(Position), typeof(Scale));

        Entity cube = entityManager.CreateEntity(cubeArchetype);
        entityManager.SetComponentData(cube, new Position {
            Value = new float3(0.0f, 0.0f, 0.0f)
        });

        entityManager.SetComponentData(cube, new Scale {
            Value = new float3(1.0f, 1.0f, 1.0f)
        });

        entityManager.SetComponentData(cube, new Cube {
            xSize = 8,
            ySize = 6,
            zSize = 4,
        });
    }
}
